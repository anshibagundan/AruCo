using UnityEngine;
using WebSocketSharp;
using System.Collections;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
public class CameraPositionSender : MonoBehaviour
{
    public string websocketUrl = "wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/xyz/unity/"; // WebSocketサーバーのURL
    private WebSocket ws;
    StatusData statusData;

    private void Start()
    {
        // WebSocketの初期化と接続
        ws = new WebSocket(websocketUrl+statusData.uuid);
        ws.OnOpen += (sender, e) => Debug.Log("WebSocket Open");
        ws.OnMessage += (sender, e) => Debug.Log("WebSocket Message: " + e.Data);
        ws.OnError += (sender, e) => Debug.Log("WebSocket Error: " + e.Message);
        ws.OnClose += (sender, e) => Debug.Log("WebSocket Close");
        ws.OnClose += (sender, e) => Debug.LogWarning($"WebSocket Close: {e.Code}, {e.Reason}");

        ws.Connect();

        // カメラの座標を定期的に送信するコルーチンを開始
        StartCoroutine(SendCameraPosition());
    }

    private IEnumerator SendCameraPosition()
    {
        while (true)
        {
            if (ws.ReadyState == WebSocketState.Open)
            {
                // カメラの座標を取得
                Vector3 cameraPosition = transform.position;

                // 座標をJSON形式に変換
                string json = JsonUtility.ToJson(new HOPPosition(cameraPosition.x,cameraPosition.z));

                Debug.Log("Sending data: " + json);

                // WebSocketで送信
                ws.Send(json);
            }

            // 0.1秒ごとに更新
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnDestroy()
    {
        // WebSocketを閉じる
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }

    // カメラの座標データを格納するクラス
    [System.Serializable]
    public class HOPPosition
    {
        public float x;
        public float z;

        public HOPPosition(float x,float z)
        {
            this.x = x;
            this.z = z;
        }
    }
}