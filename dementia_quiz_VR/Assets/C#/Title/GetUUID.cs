using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Networking;
using System;
using App.BaseSystem.DataStores.ScriptableObjects.Status; // 名前空間を適宜調整してください

public class GetUUID : MonoBehaviour
{
    // APIのエンドポイントURL
    private const string postUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/createuuid/";
    private const string getUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/q/";

    // 参照するStatusData（ScriptableObject）
    [SerializeField]
    private StatusData statusData;

    // 生成された6桁の数字
    private int generatedNumber;

    void Start()
    {
        // 1. DBのUUIDが空かどうか確認し、空でなければ以降の処理はスキップする
        if (!string.IsNullOrEmpty(statusData.UUID))
        {
            Debug.Log("UUIDが既に存在します。処理をスキップします。");
            return;
        }

        // 2. UniqueNumSender.csを実行し、6桁コードをAPIに送る
        GenerateNumber();
        StartCoroutine(PostGeneratedNumber(generatedNumber));

        // 3. UUIDを取得するまで問い合わせを開始
        StartCoroutine(PollForUUID());
    }

    // 6桁の数字を生成するメソッド
    void GenerateNumber()
    {
        // デバイスの固有IDを取得
        string uniqueString = SystemInfo.deviceUniqueIdentifier;
        byte[] hashBytes;

        // SHA256ハッシュを生成
        using (SHA256 sha256 = SHA256.Create())
        {
            hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(uniqueString));
        }

        // ハッシュの最初の4バイトから整数を取得
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(hashBytes, 0, 4);
        }
        uint value = BitConverter.ToUInt32(hashBytes, 0);

        // 6桁の整数にマッピング（100000〜999999の範囲）
        int sixDigitNumber = (int)(100000 + (value % 900000));

        generatedNumber = sixDigitNumber;
        Debug.Log("生成された6桁の数字: " + generatedNumber);
    }

    // 生成した数字をAPIに送信するコルーチン
    private IEnumerator PostGeneratedNumber(int number)
    {
        // CreateUUIDクラスのインスタンスを作成
        CreateUUID createUUID = new CreateUUID();
        createUUID.num = number;

        // JSONにシリアライズ
        string jsonData = JsonUtility.ToJson(createUUID);

        // POSTリクエストを作成
        using (UnityWebRequest webRequest = new UnityWebRequest(postUrl, "POST"))
        {
            // リクエストヘッダーを設定
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // データを設定
            byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
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
                Debug.Log("6桁の数字を送信しました。レスポンス: " + webRequest.downloadHandler.text);
            }
        }
    }

    // APIからUUIDを取得するまで定期的に問い合わせるコルーチン
    private IEnumerator PollForUUID()
    {
        while (string.IsNullOrEmpty(statusData.UUID))
        {
            yield return StartCoroutine(GetUUIDFromAPI());
            // 一定時間待機してから再度問い合わせる（例：5秒待機）
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("UUIDを取得し、StatusDataに格納しました: " + statusData.UUID);
    }

    // APIからUUIDを取得するコルーチン
    private IEnumerator GetUUIDFromAPI()
    {
        // GETリクエストのURLを作成（例: https://.../quiz-tfs/uuid/{number}）
        string requestUrl = getUrl + generatedNumber.ToString();

        using (UnityWebRequest webRequest = UnityWebRequest.Get(requestUrl))
        {
            // リクエストを送信し、レスポンスを待つ
            yield return webRequest.SendWebRequest();

            // エラーチェック
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("UUID取得時にエラーが発生しました: " + webRequest.error);
            }
            else
            {
                // レスポンスを解析してUUIDを取得
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("UUID取得のレスポンス: " + responseText);

                // レスポンスをパースしてUUIDを取得
                UUIDResponse uuidResponse = JsonUtility.FromJson<UUIDResponse>(responseText);

                if (!string.IsNullOrEmpty(uuidResponse.uuid))
                {
                    // UUIDをStatusDataに格納
                    statusData.UUID = uuidResponse.uuid;

                    // ScriptableObjectの変更を保存
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(statusData);
                    UnityEditor.AssetDatabase.SaveAssets();
#endif
                }
                else
                {
                    Debug.Log("UUIDがまだ割り当てられていません。再度問い合わせます。");
                }
            }
        }
    }

    // UUID取得用のレスポンスクラス
    [Serializable]
    private class UUIDResponse
    {
        public string uuid;
    }
}