using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using WebSocketSharp;

public class ActionController : MonoBehaviour
{
    private XRController rightController;
    private XRController leftController;
    private InputActionReference rightHandActions;
    private InputActionReference leftHandActions;

    public TextMeshProUGUI Actname;
    public TextMeshProUGUI Actsel_1;
    public TextMeshProUGUI Actsel_2;
    public StatusData statusData;

    private string GetUrl;
    private string PostUrl;
    private bool hasnotAct = false;
    private bool isProcessing = false;
    private bool isConnected = false;
    private int maxRetries = 5;
    private float retryDelay = 2f;

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void EnableInputActions()
    {
        if (rightHandActions != null)
            rightHandActions.action.Enable();
        if (leftHandActions != null)
            leftHandActions.action.Enable();
    }

    private void DisableInputActions()
    {
        if (rightHandActions != null)
            rightHandActions.action.Disable();
        if (leftHandActions != null)
            leftHandActions.action.Disable();
    }

    public void Start()
    {
        if (string.IsNullOrEmpty(statusData.UUID))
        {
            Debug.LogError("UUIDが設定されていません。");
            return;
        }

        GetUrl = "https://hopcardapi-4f6e9a3bf06d.herokuapp.com/getaction";
        PostUrl = $"wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/result/unity/{statusData.UUID}";

        SetupInitialDisplay();
        StartCoroutine(TryEstablishConnection());
    }

    private void SetupInitialDisplay()
    {
        if (Actsel_1 != null) Actsel_1.gameObject.SetActive(false);
        if (Actsel_2 != null) Actsel_2.gameObject.SetActive(false);
    }

    private IEnumerator TryEstablishConnection()
    {
        int retryCount = 0;
        while (!isConnected && retryCount < maxRetries)
        {
            yield return StartCoroutine(GetData());

            if (!hasnotAct)
            {
                isConnected = true;
                if (Actsel_1 != null) Actsel_1.gameObject.SetActive(true);
                if (Actsel_2 != null) Actsel_2.gameObject.SetActive(true);
            }
            else
            {
                retryCount++;
                if (retryCount < maxRetries)
                {
                    yield return new WaitForSeconds(retryDelay);
                }
                else
                {
                    Debug.LogError("最大リトライ回数を超過しました");
                }
            }
        }
    }

    private void Update()
    {
        if (!isConnected || isProcessing) return;

        if (CheckRightControllerButtons())
        {
            isProcessing = true;
            StartCoroutine(PostData("R"));
        }

        if (CheckLeftControllerButtons())
        {
            isProcessing = true;
            StartCoroutine(PostData("L"));
        }
    }

    private bool CheckRightControllerButtons()
    {
        if (rightController != null && rightController.inputDevice.isValid)
        {
            return rightHandActions?.action?.triggered ?? false;
        }
        return Keyboard.current != null && Keyboard.current.enterKey.isPressed;
    }

    private bool CheckLeftControllerButtons()
    {
        if (leftController != null && leftController.inputDevice.isValid)
        {
            return leftHandActions?.action?.triggered ?? false;
        }
        return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
    }

    private IEnumerator GetData()
    {
        string url = $"{GetUrl}?id={statusData.ActDiff}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            Debug.Log($"{GetUrl}?id={statusData.ActDiff}");
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("エラー: " + webRequest.error);
                hasnotAct = true;
            }
            else
            {
                string json = webRequest.downloadHandler.text;
                GetAction selectionData = JsonUtility.FromJson<GetAction>(json);

                if (selectionData != null)
                {
                    Actsel_1.text = selectionData.lef_sel;
                    Actsel_2.text = selectionData.rig_sel;
                    hasnotAct = false;
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
            bool isCorrect = (statusData.ActDiff % 2 == 0) ? (LorR == "R") : (LorR == "L");

            if (statusData.LR != null)
            {
                statusData.LR.Add(isCorrect);
            }
            else
            {
                Debug.LogError("LRリストが初期化されていません。新しいリストを作成します。");
                statusData.LR = new List<bool> { isCorrect };
            }

            PostResult payload = new PostResult
            {
                quiz_id = statusData.QuizDiff.ToArray(),
                action_id = statusData.ActDiff,
                cor = statusData.LR.ToArray()
            };

            string jsonPayload = JsonUtility.ToJson(payload);
            WebSocket ws = new WebSocket(PostUrl);
            bool messageReceived = false;
            bool connectionError = false;
            string errorMessage = null;

            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("WebSocket接続が開かれました");
                Debug.Log($"送信するデータ: {jsonPayload}");
                try
                {
                    ws.Send(jsonPayload);
                    Debug.Log($"送信したデータ: {jsonPayload}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"送信エラー: {ex.Message}");
                    errorMessage = ex.Message;
                    connectionError = true;
                }
            };

            ws.OnMessage += (sender, e) =>
            {
                Debug.Log("WebSocketでメッセージを受信しました: " + e.Data);
                messageReceived = true;
            };

            ws.OnError += (sender, e) =>
            {
                Debug.LogError("WebSocketエラー: " + e.Message);
                errorMessage = e.Message;
                connectionError = true;
            };

            ws.OnClose += (sender, e) =>
            {
                Debug.Log("WebSocket接続が閉じられました");
            };

            ws.ConnectAsync();

            float timeoutSeconds = 5.0f;
            float startTime = Time.time;

            while (!messageReceived && !connectionError && (Time.time - startTime) < timeoutSeconds)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (messageReceived)
            {
                Debug.Log("シーン遷移を実行します");
                SceneManager.LoadScene("TitleScene");
            }
            else if (connectionError)
            {
                Debug.LogError($"WebSocket通信でエラーが発生しました: {errorMessage}");
            }
            else
            {
                Debug.LogError("タイムアウトしました");
            }

            if (ws != null && ws.ReadyState == WebSocketState.Open)
            {
                ws.Close();
            }
        }
        else
        {
            Debug.LogError("コネクション失敗");
        }
        isProcessing = false;
    }
}