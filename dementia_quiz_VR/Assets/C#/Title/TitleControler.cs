using UnityEngine;
using WebSocketSharp;
using System.Collections;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using System;
using System.Collections.Generic;

public class TitleControler : MonoBehaviour
{
    private WebSocket wsQuizConnection; //クイズの難易度取得用
    private bool canTransition = false; //画面遷移管理用フラグ
    private bool isConnecting = false; //接続状況管理用フラグ

    [SerializeField]
    private StatusData statusData;

    private void Start()
    {
        ConnectToWebSocket();
    }

    private void ConnectToWebSocket()
    {
        if (isConnecting) return;
        //uuidがない場合はごり押し
        if (string.IsNullOrEmpty(statusData.UUID))
        {
            Debug.LogError("UUIDが設定されていません");
            LoadFallbackData();
            return;
        }

        isConnecting = true;

        try
        {
            string quizUrl = $"wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/difficulty/unity/{statusData.UUID}";
            wsQuizConnection = new WebSocket(quizUrl);
            wsQuizConnection.OnMessage += OnQuizDataReceived;
            wsQuizConnection.OnError += OnWebSocketError;
            wsQuizConnection.Connect();
            Debug.Log("Quiz取得用WebSocket接続に成功しました");
            // タイムアウト用のコルーチン
            StartCoroutine(ConnectionTimeout());
        }
        catch (Exception e)
        {
            Debug.Log($"Quiz取得用WebSocket接続に失敗しました: {e.Message}");
            LoadFallbackData();//ハードのjsonで読み込み
        }
    }

    //タイムアウトした時用
    private IEnumerator ConnectionTimeout()
    {
        yield return new WaitForSeconds(5f);//5秒待機

        if (!wsQuizConnection?.IsAlive ?? true)
        {
            Debug.LogWarning("WebSocket接続がタイムアウトしました");
            LoadFallbackData();//ごり押しデータ使用
        }
    }

    private void OnWebSocketError(object sender, ErrorEventArgs e)
    {
        Debug.LogError($"WebSocket error: {e.Message}");
        LoadFallbackData();
    }

    //フォールバックデータ(ハードのjson)読み込み (テスト・ミスった時用)
    private void LoadFallbackData()
    {
        if (canTransition) return; // 既にデータが設定されている場合は処理しない

        string fallbackJson = @"{
            ""quizid"": [1, 2, 3],
            ""actionid"": 1
        }";

        try
        {
            GetQuizData fallbackData = JsonUtility.FromJson<GetQuizData>(fallbackJson);
            SetData(fallbackData);
            Debug.Log("フォールバックデータを使用しました");
        }
        catch (Exception e)
        {
            Debug.LogError($"フォールバックデータの読み込みに失敗: {e.Message}");
        }
    }

    //データ格納処理
    private void SetData(GetQuizData data)
    {
        statusData.QuizDiff = new List<int>(data.quizid);
        statusData.ActDiff = data.actionid;
        Debug.Log($"受け取ったQuizDiff: [{string.Join(", ", statusData.QuizDiff)}]");
        Debug.Log($"受け取ったActDiff: {statusData.ActDiff}");
        canTransition = true;  //格納出来たらフラグをtrueに
    }

    //クイズを受け取り次第格納
    private void OnQuizDataReceived(object sender, MessageEventArgs e)
    {
        if (e.IsText)
        {
            Debug.Log($"Quiz WebSocket received: {e.Data}");
            try
            {
                GetQuizData quizData = JsonUtility.FromJson<GetQuizData>(e.Data);
                SetData(quizData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"データのパースに失敗: {ex.Message}");
                LoadFallbackData();
            }
        }
    }

    //データ格納が終わり次第画面遷移
    private void Update()
    {
        if (canTransition)
        {
            canTransition = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene("New_WalkScene");
        }
    }

    //画面が閉じたときのwebsocket接続のクローズ処理
    private void OnDestroy()
    {
        if (wsQuizConnection != null && wsQuizConnection.IsAlive)
        {
            wsQuizConnection.Close();
        }
    }
}