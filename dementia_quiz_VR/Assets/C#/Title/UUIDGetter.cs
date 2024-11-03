using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using App.Models;

public class UUIDGetter : MonoBehaviour
{
    // APIのエンドポイントURL
    private const string apiUrl = "https://hopcardapi-4f6e9a3bf06d.herokuapp.com/createuuid/";

    // 参照するStatusData（ScriptableObject）
    [SerializeField]
    private StatusData statusData;

    void Start()
    {
        // UUIDが空の場合、新規作成する
        if (string.IsNullOrEmpty(statusData.UUID))
        {
            StartCoroutine(RequestUUID());
        }
        else
        {
            Debug.Log("UUIDが既に存在します。処理をスキップします。");
        }
    }

    private IEnumerator RequestUUID()
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
        {
            // リクエストヘッダーを設定
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // 空のリクエストボディを送信するための設定
            webRequest.uploadHandler = new UploadHandlerRaw(new byte[0]);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            // リクエストを送信し、レスポンスを待つ
            yield return webRequest.SendWebRequest();

            // エラーチェック
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("エラーが発生しました: " + webRequest.error);
            }
            else
            {
                // レスポンスを解析
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("APIレスポンス: " + responseText);

                GetUUID response = JsonUtility.FromJson<GetUUID>(responseText);

                // UUIDとSerialNumをStatusDataに格納
                statusData.UUID = response.uuid;
                statusData.SerialNum = response.serialNum;

                // ScriptableObjectの変更を保存
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(statusData);
                UnityEditor.AssetDatabase.SaveAssets();
#endif

                Debug.Log($"UUID: {statusData.UUID}, SerialNum: {statusData.SerialNum}を格納しました。");
            }
        }
    }
}