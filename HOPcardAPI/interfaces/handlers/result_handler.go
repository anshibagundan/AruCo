package handlers

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"HOPcardAPI/usecase"
	"bytes"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"strings"
	"sync"
	"time"

	"github.com/gorilla/mux"
	"github.com/gorilla/websocket"
)

type ResultWebSocketHandler struct {
	upgrader           websocket.Upgrader
	androidConns       map[string]*websocket.Conn
	unityConns         map[string]*websocket.Conn
	mutex              sync.RWMutex
	quizRepository     repositories.QuizRepository
	actionRepository   repositories.ActionRepository
	userDataRepository repositories.UserDataRepository
	userusecase        *usecase.UserDataUsecase
}

// ResultNewWebSocketHandler は、Unity側とAndroid側のWebSocket接続を管理するハンドラーを生成する
func ResultNewWebSocketHandler(quizRepo repositories.QuizRepository, actionRepo repositories.ActionRepository, userDataRepo repositories.UserDataRepository, userUsecase *usecase.UserDataUsecase) *ResultWebSocketHandler {
	return &ResultWebSocketHandler{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true
			},
		},
		androidConns:       make(map[string]*websocket.Conn),
		unityConns:         make(map[string]*websocket.Conn),
		quizRepository:     quizRepo,
		actionRepository:   actionRepo,
		userDataRepository: userDataRepo,
		userusecase:        userUsecase,
	}
}

// HandleResultUnityWebSocket は、Unity側からのWebSocket接続要求を処理する
func (h *ResultWebSocketHandler) HandleResultUnityWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	// Android側の接続を確認する
	h.mutex.RLock()
	_, androidExists := h.androidConns[uuid]
	h.mutex.RUnlock()
	if !androidExists {
		http.Error(w, "No matching Android connection", http.StatusBadRequest)
		return
	}

	// Unity側の接続を確立する
	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("接続のアップグレードに失敗しました : %v", err)
		http.Error(w, "接続のアップグレードに失敗しました", http.StatusInternalServerError)
		return
	}
	h.mutex.Lock()
	h.unityConns[uuid] = conn
	h.mutex.Unlock()

	defer func() {
		h.mutex.Lock()
		delete(h.unityConns, uuid)
		h.mutex.Unlock()
		conn.Close() // 接続を閉じる
	}()

	for {
		var resultMsg models.ResultUnityMessage
		err := conn.ReadJSON(&resultMsg)
		if err != nil {
			log.Printf("Unity側からの受信に失敗しました: %v", err)
			break
		}

		log.Printf("Unity側からのメッセージを受信しました: %v", resultMsg)
		// ユーザーデータの格納処理
		ratio := float64(countTrue(resultMsg.Cor)) / float64(len(resultMsg.Cor))
		userData := models.UserData{
			Ratio:    ratio,
			Distance: resultMsg.Distance,
		}

		// ユーザーデータをデータベースに保存
		if err, _ := h.userusecase.SaveUserData(uuid, userData.Ratio, userData.Distance); err == "" {
			log.Printf("ユーザーデータの保存に失敗しました: %v", err)
			continue
		}

		// ChatGPTへのプロンプトを準備
		prompts, quiznames, difficulty, err := h.buildChatGPTPrompt(resultMsg.QuizID, resultMsg.ActionID, resultMsg.Cor)
		if err != nil {
			log.Printf("ChatGPTプロンプト作成に失敗しました: %v", err)
			continue
		}
		prompt := fmt.Sprintf("以下のデータは高齢者向けの認知症予防の問題を解いた結果で，Difficultyは問題の難易度，Nameは問題名，Detailは問題の説明でどのようなジャンルの問題かを示しています．:\n%s", strings.Join(prompts, "\n"))
		log.Println("生成されたプロンプト:", prompt)

		// ChatGPTに問い合わせ
		chatGPTResponse, err := AskChatGPT(prompt)
		if err != nil {
			log.Printf("ChatGPT呼び出しに失敗しました: %v", err)
			continue
		}
		log.Printf("ChatGPTからの応答: %s", chatGPTResponse)

		//Android側にデータを送信
		resultAndroidMsg := models.ResultAndroidMessage{
			QuizNames:  quiznames,
			Difficulty: difficulty,
			Cor:        resultMsg.Cor,
			Distance:   resultMsg.Distance,
			Message:    chatGPTResponse,
		}
		h.mutex.RLock()
		if androidConn, exists := h.androidConns[uuid]; exists {
			err := androidConn.WriteJSON(resultAndroidMsg)
			if err != nil {
				log.Printf("Android側への送信に失敗しました: %v", err)
			}
		}
		h.mutex.RUnlock()
	}
}

// buildChatGPTPrompt は、問題の詳細情報を元にChatGPTへのプロンプトを作成する
func (h *ResultWebSocketHandler) buildChatGPTPrompt(quizIDs []int, actionID int, cor []bool) ([]string, []string, string, error) {
	fmt.Printf("quizIDs: %v\n", quizIDs)
	var prompts []string
	var quiznames []string
	for i, quizID := range quizIDs {
		quiz, err := h.quizRepository.FindByID(quizID)
		if err != nil {
			return nil, nil, "", fmt.Errorf("問題の取得に失敗しました: %v", err)
		}

		// Difficultyを人間が理解しやすい形式に変換
		difficultyStr := "Unknown"
		switch quiz.Difficulty {
		case 1:
			difficultyStr = "簡単"
		case 2:
			difficultyStr = "ふつう"
		case 3:
			difficultyStr = "難しい"
		}

		// corを元に正解不正解を表現
		correctness := "不正解"
		if cor[i] {
			correctness = "正解"
		}

		log.Printf("prompt: %s", fmt.Sprintf("Difficulty: %s, Name: %s, Detail: %s, %s"))

		prompts = append(prompts, fmt.Sprintf("Difficulty: %s, Name: %s, Detail: %s, %s",
			difficultyStr, quiz.Name, quiz.Detail, correctness))
		quiznames = append(quiznames, quiz.Name)
	}

	action, err := h.actionRepository.FindOneByID(actionID)
	if err != nil {
		return nil, nil, "", fmt.Errorf("アクションの取得に失敗しました: %v", err)
	}

	difficultyStr := "Unknown"
	switch action.Difficulty {
	case 1:
		difficultyStr = "簡単"
	case 2:
		difficultyStr = "ふつう"
	case 3:
		difficultyStr = "難しい"
	}

	correctness := "不正解"
	if cor[len(cor)-1] {
		correctness = "正解"
	}
	prompts = append(prompts, fmt.Sprintf("\n 運動中や途中のクイズの間に出現した犬の数を数える問題は %sで，難易度は %sでした．この問題は注意力を問う問題です．この認知症予防の問題3問と注意力を問う問題から高齢者向けの認知症予防の観点で評価とアドバイスを問題名を使わずに2文以内で簡潔に評価してください．主語を「あなた」で答えてください．", correctness, difficultyStr))

	return prompts, quiznames, difficultyStr, nil
}

// countTrue は、boolの配列からtrueの数をカウントする
func countTrue(cor []bool) int {
	count := 0
	for _, v := range cor {
		if v {
			count++
		}
	}
	return count
}

// HandleResultAndroidWebSocket は、Android側からのWebSocket接続要求を処理する
func (h *ResultWebSocketHandler) HandleResultAndroidWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	// Android側の接続を確立する
	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("接続のアップグレードに失敗しました: %v", err)
		http.Error(w, "接続のアップグレードに失敗しました", http.StatusInternalServerError)
		return
	}
	h.mutex.Lock()
	h.androidConns[uuid] = conn
	h.mutex.Unlock()

	defer func() {
		h.mutex.Lock()
		delete(h.androidConns, uuid)
		h.mutex.Unlock()
		conn.Close() // 接続を閉じる
	}()

	for {
		var resultMsg models.ResultAndroidMessage
		err := conn.ReadJSON(&resultMsg)
		if err != nil {
			log.Printf("Android側からの受信に失敗しました: %v", err)
			break
		}

		// Unity側に結果を転送する
		h.mutex.RLock()
		if unityConn, exists := h.unityConns[uuid]; exists {
			err := unityConn.WriteJSON(resultMsg)
			if err != nil {
				log.Printf("Unity側への送信に失敗しました: %v", err)
			}
		}
		h.mutex.RUnlock()
	}
}

const url = "https://api.openai.com/v1/chat/completions"

// AskChatGPT は、与えられたプロンプトに基づいてChatGPTに問い合わせを行う関数です。
func AskChatGPT(prompt string) (string, error) {
	apiKey := os.Getenv("OPENAI_API_KEY")
	requestBody := map[string]interface{}{
		"model": "gpt-3.5-turbo",
		"messages": []map[string]string{
			{"role": "user", "content": prompt},
		},
		"max_tokens": 200,
	}

	jsonData, err := json.Marshal(requestBody)
	if err != nil {
		return "", fmt.Errorf("リクエストボディのマーシャルに失敗しました: %v", err)
	}

	req, err := http.NewRequest("POST", url, bytes.NewBuffer(jsonData))
	if err != nil {
		return "", fmt.Errorf("リクエストの作成に失敗しました: %v", err)
	}
	req.Header.Set("Authorization", "Bearer "+apiKey)
	req.Header.Set("Content-Type", "application/json")

	client := &http.Client{Timeout: 10 * time.Second}
	resp, err := client.Do(req)
	if err != nil {
		return "", fmt.Errorf("リクエストの実行に失敗しました: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", fmt.Errorf("リクエストが失敗しました: %s", resp.Status)
	}

	var responseBody struct {
		Choices []struct {
			Message struct {
				Content string `json:"content"`
			} `json:"message"`
		} `json:"choices"`
	}

	if err := json.NewDecoder(resp.Body).Decode(&responseBody); err != nil {
		return "", fmt.Errorf("レスポンスのデコードに失敗しました: %v", err)
	}

	if len(responseBody.Choices) == 0 {
		return "", fmt.Errorf("レスポンスにチョイスがありません")
	}

	return responseBody.Choices[0].Message.Content, nil
}
