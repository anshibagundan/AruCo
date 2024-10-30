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
	connections  map[string]*websocket.Conn
	mutex        sync.RWMutex
	androidConns map[string]bool
	unityConns   map[string]bool
}

// NewWebSocketHandler は新しい XYZWebSocketHandler を作成します。
func XYZNewWebSocketHandler() *XYZWebSocketHandler {
	return &XYZWebSocketHandler{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true // 本番環境では適切に設定する
			},
		},
		connections:  make(map[string]*websocket.Conn),
		androidConns: make(map[string]bool),
		unityConns:   make(map[string]bool),
	}
}

// HandleXYZUnityWebSocket は Unity からの WebSocket 接続を処理します。
func (h *XYZWebSocketHandler) HandleXYZUnityWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	// 接続をアップグレードする
	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		http.Error(w, "接続のアップグレードに失敗しました", http.StatusInternalServerError)
		return
	}

	// 接続をマップに追加する
	h.mutex.Lock()
	h.connections[uuid] = conn
	h.unityConns[uuid] = true
	h.mutex.Unlock()

	// 接続終了時のクリーンアップ
	defer func() {
		h.mutex.Lock()
		delete(h.connections, uuid)
		delete(h.unityConns, uuid)
		h.mutex.Unlock()
		conn.Close()
	}()

	// メッセージの受信ループ
	for {
		var xyzMsg models.XYZMessage
		err := conn.ReadJSON(&xyzMsg)
		if err != nil {
			break // メッセージの読み取りに失敗した場合、ループを抜ける
		}

		// Android接続が存在するか確認
		h.mutex.RLock()
		androidExists := h.androidConns[uuid]
		androidConn, androidConnExists := h.connections[uuid]
		h.mutex.RUnlock()

		// Android接続が存在する場合、メッセージを転送
		if androidExists && androidConnExists {
			err = androidConn.WriteJSON(xyzMsg)
			if err != nil {
				continue // 転送に失敗した場合、次のループに進む
			}
		}
	}
}

// HandleAndroidWebSocket は Android からの WebSocket 接続を処理します。
func (h *XYZWebSocketHandler) HandleXYZAndroidWebSocket(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	uuid := vars["uuid"]
	if uuid == "" {
		http.Error(w, "UUIDは必須です", http.StatusBadRequest)
		return
	}

	// 接続をアップグレードする
	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		http.Error(w, "接続のアップグレードに失敗しました", http.StatusInternalServerError)
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
		var xyzMsg models.XYZMessage
		err := conn.ReadJSON(&xyzMsg)
		if err != nil {
			break // メッセージの読み取りに失敗した場合、ループを抜ける
		}

		// Unity側の接続が存在する場合、メッセージを転送
		h.mutex.RLock()
		unityConn, exists := h.connections[uuid]
		isUnity := h.unityConns[uuid]
		h.mutex.RUnlock()

		if exists && isUnity {
			err = unityConn.WriteJSON(xyzMsg)
			if err != nil {
				continue // 転送に失敗した場合、次のループに進む
			}
		}
	}
}
