using UnityEngine;
using WebSocketSharp;
using System.Collections;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

public class TitleControler : MonoBehaviour
{
    private WebSocket ws;
    private bool canTransition = false;

    // APIのエンドポイントURL
    private const string getUrl = "https://example.com/api/endpoint/"; //仮置き

    // 参照するStatusData（ScriptableObject）
    [SerializeField]
    private StatusData statusData;

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
            canTransition = false; // シーン遷移が複数回行われないようにフラグをリセット

            // データを取得してからシーン遷移を行う
            StartCoroutine(GetDiffAndTransition());
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

    private IEnumerator GetDiffAndTransition()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(getUrl))
        {
            // リクエストを送信し、レスポンスを待つ
            yield return webRequest.SendWebRequest();

            // エラーチェック
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("データ取得時にエラーが発生しました: " + webRequest.error);
            }
            else
            {
                // レスポンスをテキストとして取得
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("データ取得のレスポンス: " + responseText);

                // レスポンスをパースしてGetDiffオブジェクトに変換
                GetDiff getDiff = JsonUtility.FromJson<GetDiff>(responseText);

                if (getDiff != null)
                {
                    // StatusDataにデータを格納
                    statusData.UUID = getDiff.uuid;
                    statusData.QuizDiff = new List<float>(Array.ConvertAll(getDiff.quiz_diff, item => (float)item));
                    statusData.ActDiff = (float)getDiff.act_diff;

                    // ScriptableObjectの変更を保存
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(statusData);
                    UnityEditor.AssetDatabase.SaveAssets();
#endif

                    // シーン遷移の処理を行う
                    UnityEngine.SceneManagement.SceneManager.LoadScene("New_WalkScene");
                }
                else
                {
                    Debug.LogError("取得したデータのパースに失敗しました");
                }
            }
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