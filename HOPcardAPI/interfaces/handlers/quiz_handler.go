package handlers

import (
	"HOPcardAPI/usecase"
	"encoding/json"
	"net/http"
	"strconv"
)

type QuizHandler struct {
	quizUseCase usecase.QuizUseCase
}

func NewQuizHandler(uc usecase.QuizUseCase) *QuizHandler {
	return &QuizHandler{quizUseCase: uc}
}

type QuizRequest struct {
	Question string `json:"Question"`
	Lef_sel  string `json:"lef_sel"`
	Rig_sel  string `json:"rig_sel"`
}

func (h *QuizHandler) GetQuiz(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(r.URL.Query().Get("id"))
	if err != nil {
		http.Error(w, "Invalid id parameter", http.StatusBadRequest)
	}

	quiz, err := h.quizUseCase.GetQuizByID(id)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	req := QuizRequest{
		Question: quiz.Name,
		Lef_sel:  quiz.LefSel,
		Rig_sel:  quiz.RigSel,
	}

	w.Header().Set("Content-Type", "application/json")
	if err := json.NewEncoder(w).Encode(req); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
}
