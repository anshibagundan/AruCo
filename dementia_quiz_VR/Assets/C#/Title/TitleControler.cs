using UnityEngine;
using WebSocketSharp;
using System.Collections;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using System;
using System.Collections.Generic;

[Serializable]
public class GetQuizData
{
    public int[] quizid;
    public int actionid;
}

public class TitleControler : MonoBehaviour
{
    private WebSocket wsStartConnection; //画面遷移可能かを管理するフラグ受取用
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
        isConnecting = true;

        //websocket接続を試みる
        try
        {
            wsStartConnection = new WebSocket("wss://teamhopcard-aa92d1598b3a.herokuapp.com/ws/hop/start/");
            wsStartConnection.OnMessage += OnStartMessageReceived; //メッセージを受け取ったとき実行される
            wsStartConnection.OnError += OnWebSocketError; //エラーを受け取ったとき実行される
            wsStartConnection.Connect();
            Debug.Log("Start用WebSocket接続に成功しました");
            // タイムアウト用のコルーチン
            StartCoroutine(ConnectionTimeout());
        }
        catch (Exception e)
        {
            Debug.Log($"Start用WebSocket接続に失敗しました: {e.Message}");
        }
    }
    //タイムアウトした時用のコルーチン
    private IEnumerator ConnectionTimeout()
    {
        yield return new WaitForSeconds(5f);

        if (!wsStartConnection?.IsAlive ?? true)
        {
            Debug.LogWarning("WebSocket接続がタイムアウトしました");
        }
    }

    private void OnWebSocketError(object sender, ErrorEventArgs e)
    {
        Debug.LogError($"WebSocket error: {e.Message}");
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
            SetDataToStatus(fallbackData);
            Debug.Log("フォールバックデータを使用しました");
        }
        catch (Exception e)
        {
            Debug.LogError($"フォールバックデータの読み込みに失敗: {e.Message}");
        }
    }

    //データ格納処理
    private void SetDataToStatus(GetQuizData data)
    {
        statusData.QuizDiff = new List<int>(data.quizid);
        statusData.ActDiff = data.actionid;
        Debug.Log($"設定されたQuizDiffは: [{string.Join(", ", statusData.QuizDiff)}]");
        Debug.Log("設定されたActDiffは: {statusData.ActDiff}");
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
                SetDataToStatus(quizData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"データのパースに失敗: {ex.Message}");
                LoadFallbackData();
            }
        }
    }

    //canTransitionフラグがtrueになり次第画面遷移
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
        if (wsStartConnection != null && wsStartConnection.IsAlive)
        {
            wsStartConnection.Close();
        }
        if (wsQuizConnection != null && wsQuizConnection.IsAlive)
        {
            wsQuizConnection.Close();
        }
    }
}