package handlers

import (
	"HOPcardAPI/usecase"
	"encoding/json"
	"net/http"
)

type UserDataHandler struct {
	userDataUseCase usecase.UserDataUsecase
}

func NewUserDataHandler(uc usecase.UserDataUsecase) *UserDataHandler {
	return &UserDataHandler{userDataUseCase: uc}
}

type createUserDataRequest struct {
	Uuid     string  `json:"uuid"`
	Ratio    float64 `json:"ratio"`
	Distance float64 `json:"distance"`
}

func (h *UserDataHandler) CreateUserData(w http.ResponseWriter, r *http.Request) {
	var req createUserDataRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	uuid, err := h.userDataUseCase.SaveUserData(req.Uuid, req.Ratio, req.Distance)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	// UUIDをレスポンスとして返す
	response := map[string]string{"uuid": uuid}
	w.WriteHeader(http.StatusCreated)
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

func (h *UserDataHandler) GetUserData(w http.ResponseWriter, r *http.Request) {
	uuid := r.URL.Query().Get("uuid")
	if uuid == "" {
		http.Error(w, "Missing uuid parameter", http.StatusBadRequest)
		return
	}

	userData, err := h.userDataUseCase.GetUserDataByUuid(uuid)
	if err != nil {
		http.Error(w, err.Error(), http.StatusNotFound)
		return
	}

	response := map[string]interface{}{
		"ratio":        userData.Ratio,
		"distance":     userData.Distance,
		"change_count": userData.ChangeCount,
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}
