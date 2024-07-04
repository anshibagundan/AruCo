using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using UnityEngine;
using WebSocketSharp;

public class QuizController : MonoBehaviour
{

    public TextMeshProUGUI Quizname;
    public TextMeshProUGUI Quizsel_1;
    public TextMeshProUGUI Quizsel_2;
    private bool isRotating = false; // 回転中かどうかのフラグ
    private const string difficultyGetUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-selects/";
    private const string baseGetUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quizzes/";
    private const string posturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-tfs/";
    private int difficulty = 1;
    private String geturl;
    private int randomIndex = 1;
    private int quizDataId = 1;
    private int[] AskedQuestionList;
    private bool hasnotQuiz = false;
    private bool isfinalQuiz = false;
    private bool fullAskedQuiz = false;
    private Quaternion endRotation;
    private bool isAnswered = false;
    private const int maxAttempts = 30;

    private WebSocket ws;
    private bool canTransition = false;

    //PlayerPosiotionのurl
    private String Geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    private String deleteurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/destroy_all";
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
            StartCoroutine(postPlayer("R"));
            UnityEngine.SceneManagement.SceneManager.LoadScene("New_WalkScene");
        }

        // 左コントローラーのボタン入力を検出
        if (CheckLeftControllerButtons() && !isAnswered)
        {
            isAnswered = true;
            StartCoroutine(postPlayer("L"));
            UnityEngine.SceneManagement.SceneManager.LoadScene("New_WalkScene");
        }
        if (canTransition)
        {
            // シーン遷移の処理を行う
            UnityEngine.SceneManagement.SceneManager.LoadScene("New_WalkScene");
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

    //クイズを取得する関数
    private IEnumerator GetData()
    {
        //クイズの難易度取得
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

        //クイズの難易度に合わせてURLを指定
        geturl = baseGetUrl + "?difficulty=" + difficulty;

        //出題済みクイズをGet
        using (UnityWebRequest webRequest = UnityWebRequest.Get(posturl))
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


        //難易度に合わせてクイズを取得
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
                                isfinalQuiz = true;
                                break;
                            }
                        }

                        attempts++; // 試行回数をインクリメント
                    }

                    if (attempts == maxAttempts)
                    {
                        Debug.LogError("Failed to find a quiz that hasn't been asked.");
                        hasnotQuiz = true;
                    }
                }
                else
                {
                    Debug.LogWarning("No quiz found.");
                    hasnotQuiz = true;
                }
            }
        }
    }
    //データ送信
    public IEnumerator postPlayer(String LR)
    {
        WWWForm form = new WWWForm();
        form.AddField("LR", "LR");

        using (UnityWebRequest webRequest = UnityWebRequest.Post(geturl, form))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.LogWarning("No Askedquiz found.");
            }
        }
    }



    /* //クイズの正解不正解を送る
     private IEnumerator PostData(String LorR)
     {
         if (!hasnotQuiz)
         {
             //quizのidが奇数なら左が正解に，偶数なら右が正解にする
             WWWForm form = new WWWForm();

             if (quizDataId % 2 == 0)
             {
                 if (LorR == "R")
                 {
                     form.AddField("cor", "true");
                     form.AddField("quiz", quizDataId);
                 }
                 else if (LorR == "L")
                 {
                     form.AddField("cor", "false");
                     form.AddField("quiz", quizDataId);
                 }
             }
             else
             {
                 if (LorR == "R")
                 {
                     form.AddField("cor", "false");
                     form.AddField("quiz", quizDataId);
                 }
                 else if (LorR == "L")
                 {
                     form.AddField("cor", "true");
                     form.AddField("quiz", quizDataId);
                 }
             }

             //ここで正解不正解のデータを送る
             using (UnityWebRequest webRequest = UnityWebRequest.Post(posturl, form))
             {
                 webRequest.SetRequestHeader("X-Debug-Mode", "true");
                 yield return webRequest.SendWebRequest();

                 if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                     webRequest.result == UnityWebRequest.Result.ProtocolError)
                 {
                     Debug.LogError("Error: " + webRequest.error);
                 }
             }
         }
         else
         {
             Debug.LogError("no quiz found");
         }
     }*/

    //スキップボタンが出たら，falseにする
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