using UnityEngine;
using WebSocketSharp;
using System.Collections;

public class TitleControler : MonoBehaviour
{
    private WebSocket ws;
    private bool canTransition = false;

    private void Start()
    {
        ws = new WebSocket("wss://teamhopcard-aa92d1598b3a.herokuapp.com/ws/hop/start/");
        ws.OnMessage += OnMessageReceived;
        ws.Connect();
    }

    private void Update()
    {
        if (canTransition)
        {
            // シーン遷移の処理を行う
            UnityEngine.SceneManagement.SceneManager.LoadScene("New_WalkScene");
        }
    }

    private void OnMessageReceived(object sender, MessageEventArgs e)
    {
        if (e.IsText)
        {
            Debug.Log($"Received JSON data: {e.Data}");

            // JSONデータを受け取ったらシーン遷移フラグを立てる
            canTransition = true;
            Debug.Log("Transition allowed");
        }
        else
        {
            Debug.Log("Received non-text data");
        }
    }

    private void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }
}