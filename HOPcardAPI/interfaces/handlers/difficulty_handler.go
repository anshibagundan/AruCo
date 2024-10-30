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
	connections       map[string]*websocket.Conn
	mutex             sync.RWMutex
	androidConns      map[string]bool
	unityConns        map[string]bool
}

func NewDifficultyWebSocketHandler(gameUsecase *usecase.DifficultyUsecase) *DifficultyWebSocketHandler {
	return &DifficultyWebSocketHandler{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true // 本番環境では適切に設定する
			},
		},
		difficultyUsecase: gameUsecase,
		connections:       make(map[string]*websocket.Conn),
		androidConns:      make(map[string]bool),
		unityConns:        make(map[string]bool),
	}
}

func (h *DifficultyWebSocketHandler) HandleAndroidWebSocket(w http.ResponseWriter, r *http.Request) {
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
	h.connections[uuid] = conn
	h.androidConns[uuid] = true
	h.mutex.Unlock()

	defer func() {
		h.mutex.Lock()
		delete(h.connections, uuid)
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

		// Unity側の接続が存在する場合、メッセージを転送
		h.mutex.RLock()
		unityConn, exists := h.connections[uuid]
		isUnity := h.unityConns[uuid]
		h.mutex.RUnlock()

		if exists && isUnity {
			err = unityConn.WriteJSON(unityMsg)
			if err != nil {
				continue
			}
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

	// Android側の接続が存在するか確認
	h.mutex.RLock()
	_, androidExists := h.androidConns[uuid]
	h.mutex.RUnlock()

	if !androidExists {
		http.Error(w, "No matching Android connection", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		http.Error(w, "Could not upgrade connection", http.StatusInternalServerError)
		return
	}

	h.mutex.Lock()
	h.connections[uuid] = conn
	h.unityConns[uuid] = true
	h.mutex.Unlock()

	defer func() {
		h.mutex.Lock()
		delete(h.connections, uuid)
		delete(h.unityConns, uuid)
		h.mutex.Unlock()
		conn.Close()
	}()

	// Unity側の接続を維持
	for {
		_, _, err := conn.ReadMessage()
		if err != nil {
			break
		}
	}
}
