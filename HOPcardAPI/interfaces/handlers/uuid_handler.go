package handlers

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/usecase"
	"encoding/json"
	"fmt"
	"net/http"
	"strconv"
)

type UUIDHandler struct {
	uuidUseCase usecase.UUIDUseCase
}

func NewUUIDHandler(uc usecase.UUIDUseCase) *UUIDHandler {
	return &UUIDHandler{uuidUseCase: uc}
}

func (h *UUIDHandler) CreateUUID(w http.ResponseWriter, r *http.Request) {
	uuid, err := h.uuidUseCase.CreateUUID()
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	response := models.UUIDResponse{
		UUID: uuid.UUID,
		Code: uuid.Code,
	}
	fmt.Println(response)

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

func (h *UUIDHandler) GetUUID(w http.ResponseWriter, r *http.Request) {
	codeStr := r.URL.Query().Get("code")
	code, err := strconv.Atoi(codeStr)
	if err != nil {
		http.Error(w, "Invalid code parameter", http.StatusBadRequest)
		return
	}

	uuid, err := h.uuidUseCase.GetUUIDByCode(code)
	if err != nil {
		http.Error(w, err.Error(), http.StatusNotFound)
		return
	}

	response := models.UUIDResponse{
		UUID: uuid.UUID,
		Code: uuid.Code,
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}
