package handlers

import (
	"HOPcardAPI/infrastructure/gpt"
	"HOPcardAPI/usecase"
	"encoding/json"
	"fmt"
	"net/http"
)

type UserDataHandler struct {
	userDataUseCase usecase.UserDataUsecase
}

func NewUserDataHandler(uc usecase.UserDataUsecase) *UserDataHandler {
	return &UserDataHandler{userDataUseCase: uc}
}

// リクエスト構造体
type createUserDataRequest struct {
	Uuid     string  `json:"uuid"`
	Ratio    float64 `json:"ratio"`
	Distance float64 `json:"distance"`
	QuizID   [3]int  `json:"quiz_id"`   // 3つのクイズID
	ActionID int     `json:"action_id"` // 単一のアクションID
	Cor      [4]bool `json:"cor"`       // 4つの正解/不正解情報
}

// CreateUserData はユーザーデータを作成または更新し、クイズ結果とアクション結果を保存します
func (h *UserDataHandler) CreateUserData(w http.ResponseWriter, r *http.Request) {
	var req createUserDataRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	// ユーザーデータを保存または更新
	uuid, err := h.userDataUseCase.SaveUserData(req.Uuid, req.Ratio, req.Distance)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	// クイズ結果の保存
	for i, quizID := range req.QuizID {
		quizCorrect := req.Cor[i] // 各クイズに対する正解/不正解
		err := h.userDataUseCase.SaveUserQuizResult(req.Uuid, quizID, quizCorrect)
		if err != nil {
			http.Error(w, "Failed to save quiz result: "+err.Error(), http.StatusInternalServerError)
			return
		}
	}

	// アクション結果の保存
	actionCorrect := req.Cor[3] // アクションの正解/不正解は cor の4番目
	err = h.userDataUseCase.SaveUserActionResult(req.Uuid, req.ActionID, actionCorrect)
	if err != nil {
		http.Error(w, "Failed to save action result: "+err.Error(), http.StatusInternalServerError)
		return
	}

	response := map[string]string{"uuid": uuid}
	w.WriteHeader(http.StatusCreated)
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

// GetUserData はユーザーデータと関連するクイズ/アクション結果を取得し、ChatGPTのアドバイスを含めて返します
func (h *UserDataHandler) GetUserData(w http.ResponseWriter, r *http.Request) {
	uuid := r.URL.Query().Get("uuid")
	if uuid == "" {
		http.Error(w, "Missing uuid parameter", http.StatusBadRequest)
		return
	}

	// ユーザーデータを取得
	userData, err := h.userDataUseCase.GetUserDataByUuid(uuid)
	if err != nil {
		http.Error(w, err.Error(), http.StatusNotFound)
		return
	}

	// ChatGPTのアドバイスメッセージを取得
	message, err := h.getChatGPTMessage(userData)
	if err != nil {
		http.Error(w, "Failed to retrieve message from ChatGPT: "+err.Error(), http.StatusInternalServerError)
		return
	}

	response := map[string]interface{}{
		"uuid":                uuid,
		"ratio":               userData.Ratio,
		"distance":            userData.Distance,
		"change_count":        userData.ChangeCount,
		"quiz_correct_rates":  userData.QuizCorrectRates,
		"action_correct_rate": userData.ActionCorrectRate,
		"message":             message,
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

// getChatGPTMessage はChatGPTを利用してアドバイスを取得します
func (h *UserDataHandler) getChatGPTMessage(userData *usecase.UserDataResponse) (string, error) {
	// プロンプトを生成
	prompt := fmt.Sprintf("クイズの詳細から読み取れる認知症の状態とそれを基に具体的に認知症予防に関するアドバイスを2文で提供してください．認知度平均は今まで解いた合計の問題数の正答率です，主語をあなたで回答してください:\n"+
		"認知度平均: %.2f percent \n試行回数: %d\n\nquiz result:\n",
		userData.Ratio*100, userData.ChangeCount+1)

	for _, quiz := range userData.QuizCorrectRates {
		prompt += fmt.Sprintf("- クイズ名: %s, 正答率: %.2f, 詳細: %s\n", quiz.Name, quiz.CorrectRate, quiz.Detail)
	}

	for _, action := range userData.ActionCorrectRates {
		prompt += fmt.Sprintf("アクション正答率: %.2f\n", action.CorrectRate)
	}

	// ChatGPT APIに問い合わせ
	response, err := gpt.AskChatGPT(prompt)
	if err != nil {
		return "", err
	}
	return response, nil
}
