using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using UnityEngine.XR;

public class ActionController : MonoBehaviour
{
    public TextMeshProUGUI Actname;
    public TextMeshProUGUI Actsel_1;
    public TextMeshProUGUI Actsel_2;
    public StatusData statusData;  // statusDataはアセット上に存在するScriptableObject

    private const string GetUrl = "https://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/difficulty/unity/{uuid}/";
    private const string PostUrl = "https://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/result/unity/{uuid}/";

    private bool hasnotAct = false;
    private bool isProcessing = false;
    private string uuid;

    public void Start()
    {
        uuid = statusData.UUID;
        //いちおうUUIDの確認
        if (string.IsNullOrEmpty(uuid))
        {
            Debug.LogError("UUIDが設定されていません。");
            return;
        }
        StartCoroutine(GetData());
    }

    private void Update()
    {
        if (isProcessing) return;

        // 右コントローラーのボタン入力を検出
        if (CheckRightControllerButtons())
        {
            isProcessing = true;
            StartCoroutine(PostData("R"));
        }

        // 左コントローラーのボタン入力を検出
        if (CheckLeftControllerButtons())
        {
            isProcessing = true;
            StartCoroutine(PostData("L"));
        }
    }

    private bool CheckRightControllerButtons()
    {
        // 右コントローラーの全てのボタンを確認
        InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightController.isValid)
        {
            bool buttonValue;
            if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out buttonValue) && buttonValue)
                return true;
            if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonValue) && buttonValue)
                return true;
            if (rightController.TryGetFeatureValue(CommonUsages.gripButton, out buttonValue) && buttonValue)
                return true;
            if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out buttonValue) && buttonValue)
                return true;
        }

        return false;
    }

    private bool CheckLeftControllerButtons()
    {
        // 左コントローラーの全てのボタンを確認
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftController.isValid)
        {
            bool buttonValue;
            if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out buttonValue) && buttonValue)
                return true;
            if (leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonValue) && buttonValue)
                return true;
            if (leftController.TryGetFeatureValue(CommonUsages.gripButton, out buttonValue) && buttonValue)
                return true;
            if (leftController.TryGetFeatureValue(CommonUsages.triggerButton, out buttonValue) && buttonValue)
                return true;
        }

        return false;
    }
    //クイズデータ取得
    private IEnumerator GetData()
    {
        // APIに問い合わせ
        string url = GetUrl.Replace("{uuid}", uuid) + "?id=" + statusData.ActDiff;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("エラー: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;

                // JSONレスポンスをパース
                SelectionData selectionData = JsonUtility.FromJson<SelectionData>(json);

                if (selectionData != null)
                {
                    Actsel_1.text = selectionData.lef_sel;
                    Actsel_2.text = selectionData.rig_sel;
                }
                else
                {
                    Debug.LogWarning("GetData_ActDiff:データが見つかりません");
                    hasnotAct = true;
                }
            }
        }
    }

    private IEnumerator PostData(string LorR)
    {
        if (!hasnotAct)
        {
            // 偶奇で正解判定
            bool isCorrect = false;
            isCorrect = (statusData.ActDiff % 2 == 0) ? (LorR == "R") : (LorR == "L");//なんかかっこいい書き方

            // statusData.LRに結果を保存
            if (statusData.LR != null)
            {
                statusData.LR.Add(isCorrect);
            }
            else
            {
                Debug.LogError("LRリストが初期化されていません。新しいリストを作成します。");
                statusData.LR = new List<bool> { isCorrect };
            }

            // 送信するデータを作成
            PostDataPayload payload = new PostDataPayload
            {
                quiz_id = statusData.QuizDiff.ToArray(),
                action_id = statusData.ActDiff,
                cor = statusData.LR.ToArray()
            };

            string jsonPayload = JsonUtility.ToJson(payload);

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            string url = PostUrl.Replace("{uuid}", uuid);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Debug-Mode", "true");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // リクエスト成功時の処理
                    SceneManager.LoadScene("TitleScene");
                }
                else
                {
                    Debug.LogError("Error: " + request.error);
                }
            }
        }
        else
        {
            Debug.LogError("コネクション失敗");
        }
        isProcessing = false;
    }

    [Serializable]
    public class SelectionData
    {
        public string lef_sel;
        public string rig_sel;
    }

    [Serializable]
    public class PostDataPayload
    {
        public int[] quiz_id;
        public int action_id;
        public bool[] cor;
    }
}