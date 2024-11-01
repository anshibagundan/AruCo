package handlers

import (
	"HOPcardAPI/usecase"
	"encoding/json"
	"net/http"
	"strconv"
)

type ActionHandler struct {
	actionUseCase usecase.ActionUseCase
}

func NewActionHandler(uc usecase.ActionUseCase) *ActionHandler {
	return &ActionHandler{actionUseCase: uc}
}

type ActionRequest struct {
	LefSel string `json:"lef_sel"`
	RigSel string `json:"rig_sel"`
}

func (h *ActionHandler) GetAction(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(r.URL.Query().Get("id"))
	if err != nil {
		http.Error(w, "Invalid id parameter", http.StatusBadRequest)
		return
	}

	action, err := h.actionUseCase.GetActionByID(id)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	req := ActionRequest{
		LefSel: action.LefSel,
		RigSel: action.RigSel,
	}

	w.Header().Set("Content-Type", "application/json")
	if err := json.NewEncoder(w).Encode(req); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
}
