package handlers

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"HOPcardAPI/infrastructure/gpt"
	"HOPcardAPI/usecase"
	"fmt"
	"github.com/gorilla/mux"
	"github.com/gorilla/websocket"
	"log"
	"net/http"
	"strings"
	"sync"
)

// WebSocket管理の構造体
type ResultWebSocketHandler struct {
	upgrader           websocket.Upgrader
	androidConns       map[string]*websocket.Conn
	unityConns         map[string]*websocket.Conn
	mutex              sync.RWMutex
	quizRepository     repositories.QuizRepository
	actionRepository   repositories.ActionRepository
	userDataRepository repositories.UserDataRepository
	userUsecase        usecase.UserDataUsecase
}

// 新規WebSocketHandlerを作成する
func NewResultWebSocketHandler(quizRepo repositories.QuizRepository, actionRepo repositories.ActionRepository, userDataRepo repositories.UserDataRepository, userUsecase usecase.UserDataUsecase) *ResultWebSocketHandler {
	return &ResultWebSocketHandler{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool { return true },
		},
		androidConns:       make(map[string]*websocket.Conn),
		unityConns:         make(map[string]*websocket.Conn),
		quizRepository:     quizRepo,
		actionRepository:   actionRepo,
		userDataRepository: userDataRepo,
		userUsecase:        userUsecase,
	}
}

// Unity側のWebSocket接続を処理するメインメソッド
func (h *ResultWebSocketHandler) HandleResultUnityWebSocket(w http.ResponseWriter, r *http.Request) {
	uuid := mux.Vars(r)["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("接続のアップグレードに失敗しました: %v", err)
		http.Error(w, "接続のアップグレードに失敗しました", http.StatusInternalServerError)
		return
	}
	h.addUnityConnection(uuid, conn)
	defer h.cleanupConnection(uuid, conn, "Unity")

	for {
		var resultMsg models.ResultUnityMessage
		err := conn.ReadJSON(&resultMsg)
		if err != nil {
			log.Printf("Unity側からのメッセージ受信に失敗しました: %v", err)
			break
		}
		h.processUnityMessage(uuid, resultMsg)
	}
}

// Unityからのメッセージを処理する
func (h *ResultWebSocketHandler) processUnityMessage(uuid string, resultMsg models.ResultUnityMessage) {
	ratio := float64(countTrue(resultMsg.Cor)) / float64(len(resultMsg.Cor))

	if _, err := h.userUsecase.SaveUserData(uuid, ratio, resultMsg.Distance); err != nil {
		log.Printf("ユーザーデータの保存に失敗しました: %v", err)
		return
	}

	h.saveQuizResults(uuid, resultMsg.QuizID, resultMsg.Cor)
	h.saveActionResult(uuid, resultMsg.ActionID, resultMsg.Cor[len(resultMsg.Cor)-1])

	prompt, quizNames, difficulty := h.buildChatGPTPrompt(resultMsg.QuizID, resultMsg.ActionID, resultMsg.Cor)
	response, err := h.queryChatGPT(prompt)
	if err != nil {
		log.Printf("ChatGPT問い合わせに失敗しました: %v", err)
		return
	}
	h.sendResultToAndroid(uuid, resultMsg.Distance, quizNames, difficulty, resultMsg.Cor, response)
}

// クイズ結果を保存する
func (h *ResultWebSocketHandler) saveQuizResults(uuid string, quizIDs []int, cor []bool) {
	log.Printf("クイズ結果を保存します (UUID: %s)", uuid)
	for i, quizID := range quizIDs {
		if err := h.userUsecase.SaveUserQuizResult(uuid, quizID, cor[i]); err != nil {
			log.Printf("クイズ結果の保存に失敗しました (quizID: %d): %v", quizID, err)
		}
	}
}

// アクション結果を保存する
func (h *ResultWebSocketHandler) saveActionResult(uuid string, actionID int, correct bool) {
	log.Printf("アクション結果を保存します (UUID: %s)", uuid)
	if err := h.userUsecase.SaveUserActionResult(uuid, actionID, correct); err != nil {
		log.Printf("アクション結果の保存に失敗しました (actionID: %d): %v", actionID, err)
	}
}

// ChatGPT用のプロンプトを構築する
func (h *ResultWebSocketHandler) buildChatGPTPrompt(quizIDs []int, actionID int, cor []bool) (string, []string, string) {
	var prompts []string
	var quizNames []string
	difficulty := ""

	for i, quizID := range quizIDs {
		quiz, _ := h.quizRepository.FindByID(quizID)
		difficultyStr := mapDifficulty(quiz.Difficulty)
		if i == 0 {
			difficulty = difficultyStr // 最初のクイズの難易度を設定
		}
		correctness := "正解"
		if !cor[i] {
			correctness = "不正解"
		}
		prompts = append(prompts, fmt.Sprintf("Difficulty: %s, Name: %s, Detail: %s, %s", difficultyStr, quiz.Name, quiz.Detail, correctness))
		quizNames = append(quizNames, quiz.Name)
	}

	action, _ := h.actionRepository.FindOneByID(actionID)
	actionCorrectness := "正解"
	if !cor[len(cor)-1] {
		actionCorrectness = "不正解"
	}
	actionPrompt := fmt.Sprintf("運動中や途中のクイズの間に出現した犬の数を数える問題は %sで，難易度は %sでした．この問題は注意力を問う問題です．この認知症予防の問題3問と注意力を問う問題から高齢者向けの認知症予防の観点で評価とアドバイスを問題名を使わずに2文以内で簡潔に評価してください．主語を「あなた」で答えてください", actionCorrectness, mapDifficulty(action.Difficulty))
	prompts = append(prompts, actionPrompt)

	fullPrompt := fmt.Sprintf("以下のデータは高齢者向けの認知症予防の問題を解いた結果で，Difficultyは問題の難易度，Nameは問題名，Detailは問題の説明でどのようなジャンルの問題かを示しています\n%s", strings.Join(prompts, "\n"))
	return fullPrompt, quizNames, difficulty
}

// ChatGPTへの問い合わせ
func (h *ResultWebSocketHandler) queryChatGPT(prompt string) (string, error) {
	response, err := gpt.AskChatGPT(prompt)
	if err != nil {
		return "", err
	}
	return response, nil
}

// Androidへのメッセージ送信
func (h *ResultWebSocketHandler) sendResultToAndroid(uuid string, distance float64, quizNames []string, difficulty string, cor []bool, message string) {
	result := models.ResultAndroidMessage{
		QuizNames:  quizNames,
		Difficulty: difficulty,
		Cor:        cor,
		Distance:   distance,
		Message:    message,
	}

	h.mutex.RLock()
	defer h.mutex.RUnlock()
	if androidConn, exists := h.androidConns[uuid]; exists {
		androidConn.WriteJSON(result)
	}
}

// Android 側からの WebSocket 接続要求を処理します
func (h *ResultWebSocketHandler) HandleResultAndroidWebSocket(w http.ResponseWriter, r *http.Request) {
	uuid := mux.Vars(r)["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("Android接続のアップグレードに失敗しました: %v", err)
		http.Error(w, "接続のアップグレードに失敗しました", http.StatusInternalServerError)
		return
	}

	h.addAndroidConnection(uuid, conn)
	defer h.cleanupConnection(uuid, conn, "Android")

	for {
		var resultMsg models.ResultAndroidMessage
		err := conn.ReadJSON(&resultMsg)
		if err != nil {
			log.Printf("Android側からの受信に失敗しました: %v", err)
			break
		}

		h.sendToUnity(uuid, resultMsg)
	}
}

// Android から Unity へのメッセージ転送
func (h *ResultWebSocketHandler) sendToUnity(uuid string, message models.ResultAndroidMessage) {
	h.mutex.RLock()
	defer h.mutex.RUnlock()
	if unityConn, exists := h.unityConns[uuid]; exists {
		err := unityConn.WriteJSON(message)
		if err != nil {
			log.Printf("Unity側への送信に失敗しました: %v", err)
		}
	}
}

// Android接続を追加
func (h *ResultWebSocketHandler) addAndroidConnection(uuid string, conn *websocket.Conn) {
	h.mutex.Lock()
	h.androidConns[uuid] = conn
	h.mutex.Unlock()
}

// Unity接続を追加
func (h *ResultWebSocketHandler) addUnityConnection(uuid string, conn *websocket.Conn) {
	h.mutex.Lock()
	h.unityConns[uuid] = conn
	h.mutex.Unlock()
}

// 接続をクリーンアップ
func (h *ResultWebSocketHandler) cleanupConnection(uuid string, conn *websocket.Conn, connType string) {
	h.mutex.Lock()
	defer h.mutex.Unlock()
	delete(h.androidConns, uuid)
	delete(h.unityConns, uuid)
	conn.Close()
	log.Printf("%s接続が閉じられました (UUID: %s)", connType, uuid)
}

// 正解数をカウント
func countTrue(cor []bool) int {
	count := 0
	for _, v := range cor {
		if v {
			count++
		}
	}
	return count
}

// 難易度をマッピング
func mapDifficulty(difficulty int) string {
	switch difficulty {
	case 1:
		return "簡単"
	case 2:
		return "ふつう"
	case 3:
		return "難しい"
	}
	return "不明"
}
