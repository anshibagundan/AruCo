package handlers

import (
	"HOPcardAPI/domain/models"
	"github.com/gorilla/mux"
	"github.com/gorilla/websocket"
	"log"
	"net/http"
	"sync"
)

type ScreenShareHandler struct {
	upgrader     websocket.Upgrader
	androidConns map[string]*websocket.Conn
	unityConns   map[string]*websocket.Conn
	mutex        sync.RWMutex
}

func NewScreenShareHandler() *ScreenShareHandler {
	return &ScreenShareHandler{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true
			},
		},
		androidConns: make(map[string]*websocket.Conn),
		unityConns:   make(map[string]*websocket.Conn),
	}
}

func (h *ScreenShareHandler) HandleUnityWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	log.Printf("Unityクライアント接続要求: UUID=%s", uuid)

	h.mutex.RLock()
	_, androidExists := h.androidConns[uuid]
	h.mutex.RUnlock()

	if !androidExists {
		log.Printf("Android接続が見つかりません: UUID=%s", uuid)
		http.Error(w, "Android接続が見つかりません", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("Unity WebSocketアップグレード失敗: %v", err)
		http.Error(w, "WebSocket接続の確立に失敗しました", http.StatusInternalServerError)
		return
	}

	log.Printf("Unityクライアント接続成功: UUID=%s", uuid)

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
		var msg models.CastMessage
		err := conn.ReadJSON(&msg)
		if err != nil {
			break
		}

		// Android側に転送
		h.mutex.RLock()
		if androidConn, exists := h.androidConns[uuid]; exists {
			if err := androidConn.WriteJSON(msg); err != nil {
				break
			}
		}
		h.mutex.RUnlock()
	}
}

func (h *ScreenShareHandler) HandleAndroidWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	log.Printf("Androidクライアント接続要求: UUID=%s", uuid)

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("Android WebSocketアップグレード失敗: %v", err)
		http.Error(w, "WebSocket接続の確立に失敗しました", http.StatusInternalServerError)
		return
	}

	log.Printf("Androidクライアント接続成功: UUID=%s", uuid)

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
		var msg models.CastMessage
		err := conn.ReadJSON(&msg)
		if err != nil {
			break
		}

		// Unity側に転送
		h.mutex.RLock()
		if unityConn, exists := h.unityConns[uuid]; exists {
			if err := unityConn.WriteJSON(msg); err != nil {
				break
			}
		}
		h.mutex.RUnlock()
	}
}
