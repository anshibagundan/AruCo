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


// VRコントローラーの入力を処理し、アクションの選択と結果の送信を管理するクラス

public class ActionController : MonoBehaviour
{
    // VRコントローラーの参照
    [SerializeField] private XRController rightController;
    [SerializeField] private XRController leftController;

    // Input System用のアクション参照
    [SerializeField] private InputActionReference rightHandActions;
    [SerializeField] private InputActionReference leftHandActions;

    // UI要素の参照
    public TextMeshProUGUI Actname;
    public TextMeshProUGUI Actsel_1;
    public TextMeshProUGUI Actsel_2;
    public TextMeshProUGUI WaitingText;

    // ステータスデータの参照
    public StatusData statusData;

    // API エンドポイントURL
    private string GetUrl;
    private string PostUrl;

    // 状態管理フラグ
    private bool hasnotAct = false;      // アクションデータが取得できていないかどうか
    private bool isProcessing = false;    // データ処理中かどうか
    private bool isConnected = false;     // サーバーに接続されているかどうか

    // 接続リトライ設定
    private int maxRetries = 5;          // 最大再試行回数
    private float retryDelay = 2f;       // 再試行間の待機時間（秒）

    
    // コンポーネントが有効になった時の処理
    
    private void OnEnable()
    {
        EnableInputActions();
    }

    
    // コンポーネントが無効になった時の処理
    private void OnDisable()
    {
        DisableInputActions();
    }

    
    // Input Actionを有効化
    private void EnableInputActions()
    {
        if (rightHandActions != null)
            rightHandActions.action.Enable();
        if (leftHandActions != null)
            leftHandActions.action.Enable();
    }

    
    // Input Actionを無効化
    private void DisableInputActions()
    {
        if (rightHandActions != null)
            rightHandActions.action.Disable();
        if (leftHandActions != null)
            leftHandActions.action.Disable();
    }

    
    // 初期化処理
    public void Start()
    {
        // UUIDの検証
        if (string.IsNullOrEmpty(statusData.UUID))
        {
            Debug.LogError("UUIDが設定されていません。");
            return;
        }

        // APIエンドポイントの設定
        GetUrl = "https://hopcardapi-4f6e9a3bf06d.herokuapp.com/getaction";
        PostUrl = $"wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/result/unity/{statusData.UUID}/";

        SetupInitialDisplay();
        StartCoroutine(TryEstablishConnection());
    }

   
    // 初期UI表示の設定
    private void SetupInitialDisplay()
    {
        if (WaitingText != null)
        {
            WaitingText.text = "少々お待ちください";
            WaitingText.gameObject.SetActive(true);
        }
        if (Actsel_1 != null) Actsel_1.gameObject.SetActive(false);
        if (Actsel_2 != null) Actsel_2.gameObject.SetActive(false);
    }

    
    // サーバーとの接続確立を試行
    private IEnumerator TryEstablishConnection()
    {
        int retryCount = 0;
        while (!isConnected && retryCount < maxRetries)
        {
            yield return StartCoroutine(GetData());

            if (!hasnotAct)
            {
                isConnected = true;
                if (WaitingText != null) WaitingText.gameObject.SetActive(false);
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
                    if (WaitingText != null) WaitingText.text = "接続に失敗しました";
                    Debug.LogError("最大リトライ回数を超過しました");
                }
            }
        }
    }

    
    // 毎フレームの更新処理
    private void Update()
    {
        if (!isConnected || isProcessing) return;

        // 右コントローラーの入力チェック
        if (CheckRightControllerButtons())
        {
            isProcessing = true;
            StartCoroutine(PostData("R"));
        }

        // 左コントローラーの入力チェック
        if (CheckLeftControllerButtons())
        {
            isProcessing = true;
            StartCoroutine(PostData("L"));
        }
    }

    
    // 右コントローラーのボタン入力をチェック
    
    private bool CheckRightControllerButtons()
    {
        // VRコントローラーのチェック
        if (rightController != null && rightController.inputDevice.isValid)
        {
            return rightHandActions?.action?.triggered ?? false;
        }

        // デバッグ用キーボード入力
        return Keyboard.current != null && Keyboard.current.enterKey.isPressed;
    }

    
    // 左コントローラーのボタン入力をチェック
    
    private bool CheckLeftControllerButtons()
    {
        // VRコントローラーのチェック
        if (leftController != null && leftController.inputDevice.isValid)
        {
            return leftHandActions?.action?.triggered ?? false;
        }

        // デバッグ用キーボード入力
        return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
    }

    
    // サーバーからアクションデータを取得
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
                SelectionData selectionData = JsonUtility.FromJson<SelectionData>(json);

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

    
    // 選択結果をサーバーに送信
    // <param name="LorR">左右の選択（"L" または "R"）</param>
    private IEnumerator PostData(string LorR)
    {
        if (!hasnotAct)
        {
            // 正解判定
            bool isCorrect = (statusData.ActDiff % 2 == 0) ? (LorR == "R") : (LorR == "L");

            // 結果の記録
            if (statusData.LR != null)
            {
                statusData.LR.Add(isCorrect);
            }
            else
            {
                Debug.LogError("LRリストが初期化されていません。新しいリストを作成します。");
                statusData.LR = new List<bool> { isCorrect };
            }

            // 送信データの準備
            PostDataPayload payload = new PostDataPayload
            {
                quiz_id = statusData.QuizDiff.ToArray(),
                action_id = statusData.ActDiff,
                cor = statusData.LR.ToArray()
            };

            string jsonPayload = JsonUtility.ToJson(payload);
            bool isCompleted = false;
            string errorMessage = null;

            // WebSocket通信
            using (WebSocket ws = new WebSocket(PostUrl))
            {
                ws.OnOpen += (sender, e) =>
                {
                    Debug.Log("WebSocket接続が開かれました");
                    ws.Send(jsonPayload);
                };

                ws.OnMessage += (sender, e) =>
                {
                    Debug.Log("WebSocketでメッセージを受信しました: " + e.Data);
                    isCompleted = true;
                    ws.Close();
                };

                ws.OnError += (sender, e) =>
                {
                    Debug.LogError("WebSocketエラー: " + e.Message);
                    errorMessage = e.Message;
                    isCompleted = true;
                    ws.Close();
                };

                ws.OnClose += (sender, e) =>
                {
                    Debug.Log("WebSocket接続が閉じられました");
                };

                ws.ConnectAsync();

                while (!isCompleted)
                {
                    yield return null;
                }
            }

            // 通信結果の処理
            if (string.IsNullOrEmpty(errorMessage))
            {
                SceneManager.LoadScene("TitleScene");
            }
            else
            {
                Debug.LogError("WebSocket通信中のエラー: " + errorMessage);
            }
        }
        else
        {
            Debug.LogError("コネクション失敗");
        }
        isProcessing = false;
    }

    
    // 選択肢データのシリアライズ用クラス
    [Serializable]
    public class SelectionData
    {
        public string lef_sel;
        public string rig_sel;
    }

    
    // 送信データのシリアライズ用クラス
    [Serializable]
    public class PostDataPayload
    {
        public int[] quiz_id;
        public int action_id;
        public bool[] cor;
    }
}