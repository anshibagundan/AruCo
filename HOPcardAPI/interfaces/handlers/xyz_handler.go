package handlers

import (
	"HOPcardAPI/domain/models"
	"github.com/gorilla/mux"
	"github.com/gorilla/websocket"
	"net/http"
	"sync"
)

type XYZWebSocketHandler struct {
	upgrader     websocket.Upgrader
	androidConns map[string]*websocket.Conn
	unityConns   map[string]*websocket.Conn
	mutex        sync.RWMutex
}

func XYZNewWebSocketHandler() *XYZWebSocketHandler {
	return &XYZWebSocketHandler{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true
			},
		},
		androidConns: make(map[string]*websocket.Conn),
		unityConns:   make(map[string]*websocket.Conn),
	}
}

func (h *XYZWebSocketHandler) HandleXYZUnityWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	h.mutex.RLock()
	_, androidExists := h.androidConns[uuid]
	h.mutex.RUnlock()

	if !androidExists {
		http.Error(w, "No matching Android connection", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		http.Error(w, "接続のアップグレードに失敗しました", http.StatusInternalServerError)
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
		var xyzMsg models.XYZMessage
		err := conn.ReadJSON(&xyzMsg)
		if err != nil {
			break
		}

		// Android側に転送
		h.mutex.RLock()
		if androidConn, exists := h.androidConns[uuid]; exists {
			androidConn.WriteJSON(xyzMsg)
		}
		h.mutex.RUnlock()
	}
}

func (h *XYZWebSocketHandler) HandleXYZAndroidWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		http.Error(w, "接続のアップグレードに失敗しました", http.StatusInternalServerError)
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
		var xyzMsg models.XYZMessage
		err := conn.ReadJSON(&xyzMsg)
		if err != nil {
			break
		}

		// Unity側に転送
		h.mutex.RLock()
		if unityConn, exists := h.unityConns[uuid]; exists {
			unityConn.WriteJSON(xyzMsg)
		}
		h.mutex.RUnlock()
	}
}
