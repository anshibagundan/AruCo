using UnityEngine;
using WebSocketSharp;
using System.Collections;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using System;
using System.Collections.Generic;

public class TitleControler : MonoBehaviour
{
    private WebSocket wsQuizConnection;
    private bool canTransition = false;
    private bool isConnecting = false;

    [SerializeField]
    private StatusData statusData;

    private UUIDGetter uuidGetter;
    private void Start()
    {
        StartCoroutine(InitializeAndConnect());
    }

    private IEnumerator InitializeAndConnect()
    {
        // UUIDGetterのStartメソッドを待つ
        yield return new WaitForSeconds(0.1f);  // UUIDGetterの処理を待つための短い遅延

        // WebSocket接続を開始
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
        Debug.Log($"受け取ったQuizDiff: [{string.Join(", ", data.quiz_id)}]");
        Debug.Log($"受け取ったActDiff: {data.action_id}");
        statusData.QuizDiff = new List<int>(data.quiz_id);
        statusData.ActDiff = data.action_id;
        Debug.Log($"格納したQuizDiff: [{string.Join(", ", statusData.QuizDiff)}]");
        Debug.Log($"格納したActDiff: {statusData.ActDiff}");
        canTransition = true;  //格納出来たらフラグをtrueに
    }

    private void OnStartMessageReceived(object sender, MessageEventArgs e)
    {
        if (true)//テストようにiftrue。本番ではif(e.Data)に書き換え
        {
            Debug.Log("Start WebSocket received: {e.Data}");
            //StartQuizWebSocket(e.Data);
            StartQuizWebSocket(statusData.uuid); //テスト用にごり押しコード
        }
    }

    //クイズ難易度取得用wsに接続
    private void StartQuizWebSocket(string uuid)
    {
        try{
            string quizUrl = $"wss://teamhopcard-aa92d1598b3a.herokuapp.com/ws/difficulty/unity/{uuid}";
            wsQuizConnection = new WebSocket(quizUrl);
            wsQuizConnection.OnMessage += OnQuizDataReceived;
            wsQuizConnection.OnError += OnWebSocketError;
            wsQuizConnection.Connect();
            Debug.Log($"Quiz取得用WebSocket接続に成功しました");
        }
        catch (Exception e){
            Debug.Log($"Quiz取得用WebSocket接続に失敗しました: {e.Message}");
            LoadFallbackData();//ハードのjsonで読み込み
}
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