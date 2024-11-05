package handlers

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/usecase"
	"github.com/gorilla/mux"
	"github.com/gorilla/websocket"
	"log"
	"net/http"
	"sync"
	"time"
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

		// Android側にもメッセージを送信
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

	// Set up a ticker to send ping messages every 30 seconds
	ticker := time.NewTicker(30 * time.Second)
	defer ticker.Stop()

	// Goroutine to send ping messages to keep the connection alive
	go func() {
		for range ticker.C {
			h.mutex.RLock()
			if unityConn, exists := h.unityConns[uuid]; exists {
				err := unityConn.WriteMessage(websocket.PingMessage, []byte{})
				if err != nil {
					log.Printf("Failed to send ping to Unity: %v", err)
					h.mutex.RUnlock()
					return // Exit the goroutine if ping fails
				}
			}
			h.mutex.RUnlock()
		}
	}()

	// Main loop to handle incoming messages from the Unity client
	for {
		_, _, err := conn.ReadMessage()
		if err != nil {
			break
		}
	}
}
