package handlers

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/usecase"
	"github.com/gorilla/mux"
	"github.com/gorilla/websocket"
	"net/http"
	"sync"
)

type DifficultyWebSocketHandler struct {
	upgrader          websocket.Upgrader
	difficultyUsecase *usecase.DifficultyUsecase
	androidConns      map[string]*websocket.Conn
	unityConns        map[string]*websocket.Conn
	mutex             sync.RWMutex
}

func NewDifficultyWebSocketHandler(difficultyUsecase *usecase.DifficultyUsecase) *DifficultyWebSocketHandler {
	return &DifficultyWebSocketHandler{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true
			},
		},
		difficultyUsecase: difficultyUsecase,
		androidConns:      make(map[string]*websocket.Conn),
		unityConns:        make(map[string]*websocket.Conn),
	}
}

func (h *DifficultyWebSocketHandler) HandleAndroidWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUID is required", http.StatusBadRequest)
		return
	}

	// Unity側の接続が存在するか確認
	h.mutex.RLock()
	_, unityExists := h.unityConns[uuid]
	h.mutex.RUnlock()

	if !unityExists {
		http.Error(w, "No matching Unity connection", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		http.Error(w, "Could not upgrade connection", http.StatusInternalServerError)
		return
	}

	h.mutex.Lock()
	h.androidConns[uuid] = conn
	h.mutex.Unlock()

	defer func() {
		h.mutex.Lock()
		delete(h.androidConns, uuid)
		h.mutex.Unlock()
		conn.Close()
	}()

	for {
		var androidMsg models.AndroidDifficultyMessage
		err := conn.ReadJSON(&androidMsg)
		if err != nil {
			break
		}

		unityMsg, err := h.difficultyUsecase.ProcessDifficultyData(androidMsg.Difficulty)
		if err != nil {
			continue
		}

		// Unity側の接続にのみメッセージを送信
		h.mutex.RLock()
		if unityConn, exists := h.unityConns[uuid]; exists {
			err = unityConn.WriteJSON(unityMsg)
		}
		h.mutex.RUnlock()

		// Android側には受信確認のみ送信
		err = conn.WriteJSON(unityMsg)
		if err != nil {
			continue
		}
	}
}

func (h *DifficultyWebSocketHandler) HandleUnityWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUID is required", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		http.Error(w, "Could not upgrade connection", http.StatusInternalServerError)
		return
	}

	h.mutex.Lock()
	h.unityConns[uuid] = conn
	h.mutex.Unlock()

	defer func() {
		h.mutex.Lock()
		delete(h.unityConns, uuid)
		h.mutex.Unlock()
		conn.Close()
	}()

	for {
		_, _, err := conn.ReadMessage()
		if err != nil {
			break
		}
	}
}
