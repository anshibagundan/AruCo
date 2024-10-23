package handlers

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/usecase"
	"encoding/json"
	"net/http"
	"strconv"
)

type UUIDHandler struct {
	uuidUseCase usecase.UUIDUseCase
}

func NewUUIDHandler(uc usecase.UUIDUseCase) *UUIDHandler {
	return &UUIDHandler{uuidUseCase: uc}
}

type createUUIDRequest struct {
	Code int `json:"code"`
}

func (h *UUIDHandler) CreateUUID(w http.ResponseWriter, r *http.Request) {
	var req createUUIDRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	uuid, err := h.uuidUseCase.CreateUUID(req.Code)
	if err != nil {
		if err.Error() == "code already exists" {
			// 既存のコードの場合は409 Conflictを返す
			w.WriteHeader(http.StatusConflict)
			json.NewEncoder(w).Encode(map[string]string{
				"error": "Code already exists",
			})
			return
		}
		// その他のエラーは500 Internal Server Error
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	response := models.UUIDResponse{
		UUID: uuid.UUID,
		Code: uuid.Code,
	}

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
