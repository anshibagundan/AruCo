using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.XR;
using WebSocketSharp;

public class QuizController : MonoBehaviour
{
    public TextMeshProUGUI Quizname;
    public TextMeshProUGUI Quizsel_1;
    public TextMeshProUGUI Quizsel_2;
    private const string difficultyGetUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-selects/";
    private const string baseGetUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quizzes/";
    private const string postUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-tfs/";
    private int difficulty = 1;
    private string geturl;
    private int randomIndex = 1;
    private int quizDataId = 1;
    private int[] AskedQuestionList;
    private bool hasNotQuiz = false;
    private bool isFinalQuiz = false;
    private bool fullAskedQuiz = false;
    private bool isAnswered = false;
    private const int maxAttempts = 30;

    private WebSocket ws;
    private bool canTransition = false;

    // PlayerPositionのURL
    private const string Geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    private const string deleteurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/destroy_all";
    Player playerData;

    public void Start()
    {
        StartCoroutine(GetData());
        ws = new WebSocket("wss://teamhopcard-aa92d1598b3a.herokuapp.com/ws/hop/quiz/");
        ws.OnMessage += OnMessageReceived;
        ws.Connect();
    }

    private void Update()
    {
        // 右コントローラーのボタン入力を検出
        if (CheckRightControllerButtons() && !isAnswered)
        {
            isAnswered = true;
            StartCoroutine(PostAndLoadScene("R"));
        }

        // Qキーが押された瞬間を検出
        if (Input.GetKeyDown(KeyCode.Return) && !isAnswered)
        {
            isAnswered = true;
            StartCoroutine(PostAndLoadScene("R"));
        }

        // 左コントローラーのボタン入力を検出
        if (CheckLeftControllerButtons() && !isAnswered)
        {
            isAnswered = true;
            StartCoroutine(PostAndLoadScene("L"));
        }

        // Eキーが押された瞬間を検出
        if (Input.GetKeyDown(KeyCode.Space) && !isAnswered)
        {
            isAnswered = true;
            StartCoroutine(PostAndLoadScene("L"));
        }

        if (canTransition)
        {
            // シーン遷移の処理を行う
            SceneManager.LoadScene("New_WalkScene");
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
            // 他のボタンも必要に応じて追加
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
            // 他のボタンも必要に応じて追加
        }
        return false;
    }

    // クイズを取得する関数
    private IEnumerator GetData()
    {
        // クイズの難易度取得
        using (UnityWebRequest webRequest = UnityWebRequest.Get(difficultyGetUrl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;

                QuizSelDiff[] QuizSelDiffDataArray = JsonHelper.FromJson<QuizSelDiff>(json);

                if (QuizSelDiffDataArray != null && QuizSelDiffDataArray.Length > 0)
                {
                    QuizSelDiff QuizSelDiffData = QuizSelDiffDataArray[0];
                    difficulty = QuizSelDiffData.select_diff;
                }
                else
                {
                    Debug.LogWarning("No quizdiff found.");
                }
            }
        }

        // クイズの難易度に合わせてURLを指定
        geturl = baseGetUrl + "?difficulty=" + difficulty;

        // 出題済みクイズをGet
        using (UnityWebRequest webRequest = UnityWebRequest.Get(postUrl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;

                QuizTF[] quizTFDataArray = JsonHelper.FromJson<QuizTF>(json);

                if (quizTFDataArray != null && quizTFDataArray.Length > 0)
                {
                    if (quizTFDataArray.Length == 2)
                    {
                        AskedQuestionList = new int[2] { quizTFDataArray[0].quiz, quizTFDataArray[1].quiz };
                    }
                    else if (quizTFDataArray.Length == 1)
                    {
                        AskedQuestionList = new int[1] { quizTFDataArray[0].quiz };
                    }
                    else if (quizTFDataArray.Length >= 3)
                    {
                        Debug.LogWarning("Asked Quiz Remained");
                        fullAskedQuiz = true;
                    }
                }
                else
                {
                    Debug.LogWarning("No Askedquiz found.");
                }
            }
        }

        // 難易度に合わせてクイズを取得
        using (UnityWebRequest webRequest = UnityWebRequest.Get(geturl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;

                Quiz[] quizDataArray = JsonHelper.FromJson<Quiz>(json);

                if (quizDataArray != null && quizDataArray.Length > 0)
                {
                    int maxAttempts = 30; // 最大試行回数
                    int attempts = 0; // 試行回数のカウンター

                    while (attempts < maxAttempts)
                    {
                        randomIndex = UnityEngine.Random.Range(0, quizDataArray.Length);
                        Quiz QuizData = quizDataArray[randomIndex];
                        quizDataId = quizDataArray[randomIndex].id;

                        if (AskedQuestionList == null || AskedQuestionList.Length == 0)
                        {
                            Quizname.text = QuizData.name;
                            Quizsel_1.text = "1: " + QuizData.sel_1;
                            Quizsel_2.text = "2: " + QuizData.sel_2;
                            break;
                        }
                        else if (AskedQuestionList.Length == 1)
                        {
                            if (AskedQuestionList[0] != quizDataId)
                            {
                                Quizname.text = QuizData.name;
                                Quizsel_1.text = "1: " + QuizData.sel_1;
                                Quizsel_2.text = "2: " + QuizData.sel_2;
                                break;
                            }
                        }
                        else if (AskedQuestionList.Length == 2)
                        {
                            if (AskedQuestionList[0] != quizDataId && AskedQuestionList[1] != quizDataId)
                            {
                                Quizname.text = QuizData.name;
                                Quizsel_1.text = "1: " + QuizData.sel_1;
                                Quizsel_2.text = "2: " + QuizData.sel_2;
                                isFinalQuiz = true;
                                break;
                            }
                        }

                        attempts++; // 試行回数をインクリメント
                    }

                    if (attempts == maxAttempts)
                    {
                        Debug.LogError("Failed to find a quiz that hasn't been asked.");
                        hasNotQuiz = true;
                    }
                }
                else
                {
                    Debug.LogWarning("No quiz found.");
                    hasNotQuiz = true;
                }
            }
        }
    }

    // コルーチンでプレイヤーのデータを送信し、シーンを遷移する
    private IEnumerator PostAndLoadScene(string direction)
    {
        yield return StartCoroutine(postPlayer(direction));
        SceneManager.LoadScene("New_WalkScene");
    }

    // データ送信
    private IEnumerator postPlayer(string direction)
    {
        WWWForm form = new WWWForm();
        form.AddField("rl", direction);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(postUrl, form))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Post successful");
            }
        }
    }


    // スキップボタンが出たら、falseにする
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