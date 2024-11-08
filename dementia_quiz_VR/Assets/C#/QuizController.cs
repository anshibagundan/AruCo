using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using UnityEngine.InputSystem;//enterキー入力のため

public class QuizController : MonoBehaviour
{

    public TextMeshProUGUI Question;
    public TextMeshProUGUI Quizlef;
    public TextMeshProUGUI Quizrig;
    public GameObject objectToRotate;
    public float rotationDuration = 1f; // 回転にかける時間
    private bool isRotating = false; // 回転中かどうかのフラグ
    private const string QetQuizurl = "https://hopcardapi-4f6e9a3bf06d.herokuapp.com/getquiz";
    private int difficulty = 1;
    private String geturl;
    private int randomIndex = 1;
    private int quizDataId = 1;
    private int[] AskedQuestionList;
    private bool hasnotQuiz = false;
    private bool isfinalQuiz = false;
    private bool fullAskedQuiz = false;
    private Quaternion endRotation;
    private bool isAnswered = false;//解答した後の処理のために必要な切り替えに必要な処理っぽい
    private const int maxAttempts = 30;
    private int Quizdiff;
    private GetQuiz QuizData;
    [SerializeField]
    private StatusData statusData;
    int LRCount = 0;//LR要素数の初期化

    public void Start()
    {
        StartCoroutine(GetData());
        //statusDataのアセットのLRの要素数を0にしないとエラーはくので注意
    }

    private void Update()
    {
        // 右コントローラーのボタン入力を検出
        if (CheckRightControllerButtons() && !isAnswered)
        {
            isAnswered = true;
            StartCoroutine(PostData());
           // StartCoroutine(RotateCoroutine());
        }

        // 左コントローラーのボタン入力を検出
        if (CheckLeftControllerButtons() && !isAnswered)
        {
            isAnswered = true;
            StartCoroutine(PostData());
            //StartCoroutine(RotateCoroutine());
        }

    }


    private bool CheckRightControllerButtons()
    {

        // Enterキーの確認
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.isPressed || Keyboard.current.numpadEnterKey.isPressed))
        {
            return true;
        }
        // 右コントローラーの全てのボタンを確認
        UnityEngine.XR.InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (rightController.isValid)
        {
            bool buttonValue;
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue)
                return true;
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonValue) && buttonValue)
                return true;
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out buttonValue) && buttonValue)
                return true;
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out buttonValue) && buttonValue)
                return true;
            // 他のボタンも必要に応じて追加
        }
        return false;
    }

    private bool CheckLeftControllerButtons()
    {
        // スペースキーの確認
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            return true;
        }
        // 左コントローラーの全てのボタンを確認
        UnityEngine.XR.InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftController.isValid)
        {
            bool buttonValue;
            if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue)
                return true;
            if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonValue) && buttonValue)
                return true;
            if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out buttonValue) && buttonValue)
                return true;
            if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out buttonValue) && buttonValue)
                return true;
            // 他のボタンも必要に応じて追加
        }
        return false;
    }

    //クイズを取得する関数
    private IEnumerator GetData()
    {
        // nullチェック
        if (statusData == null)
        {
            Debug.LogError("StatusData is not assigned!");
            yield break;
        }
        //StatusDataからQuizDIffを取得
        LRCount = statusData.LR.Count;
        Quizdiff =statusData.QuizDiff[LRCount];//要素数をクエリパラメータとする

        //クイズの難易度に合わせてURLを指定
        //元はdifficultyに問い合わせてた
        geturl = QetQuizurl + "?id=" + Quizdiff;

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
                /*
                 JSONファイルは以下のように帰ってくるはず
                "Question" : String,
                "lef_sel" : String,
                "rig_sel" : String
                ex:
                "Question" : "1+1は？",
                "lef_sel" : "2",
                "rig_sel" : "3"
                */
                string json = webRequest.downloadHandler.text;
                Debug.Log("受信したJSONデータ: " + json);

                try
                {
                    QuizData = JsonUtility.FromJson<GetQuiz>(json);
                    // パース成功
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON Parse Error: {e.Message}");
                }

                if (QuizData != null)
                {
                    Debug.Log($"パースしたデータ:" +
                        $"\n Question: {QuizData.Question}" +
                        $"\n Left: {QuizData.lef_sel}" +
                        $"\n Right: {QuizData.rig_sel}");

                    if (statusData.LR.Count == 0)
                    {
                        Question.text = QuizData.Question;
                        Quizlef.text = "1: " + QuizData.lef_sel;
                        Quizrig.text = "2: " + QuizData.rig_sel;
                    }
                    else if (statusData.LR.Count == 1)
                    {
                        Question.text = QuizData.Question;
                        Quizlef.text = "1: " + QuizData.lef_sel;
                        Quizrig.text = "2: " + QuizData.rig_sel;
                    }
                    else if (statusData.LR.Count == 2)
                    {
                        Question.text = QuizData.Question;
                        Quizlef.text = "1: " + QuizData.lef_sel;
                        Quizrig.text = "2: " + QuizData.rig_sel;
                        isfinalQuiz = true;

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

    //クイズの正解不正解を送る
    private IEnumerator PostData()
    {
        if (!hasnotQuiz)
        {
            //quizのidが偶数なら右が正解に，奇数なら左が正解にする
            if (IsEven(statusData.QuizDiff[LRCount]))//元はquizDataId % 2 == 0
            {
                statusData.LR.Add(true);
            }
            else
            {
                statusData.LR.Add(false);
            }

        }
        else
        {
            Debug.LogError("no quiz found");
        }
        if (!isfinalQuiz)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("New_WalkScene");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("New_WalkScene");
        }

        yield return null;
    }
    //回転用
    /*IEnumerator RotateCoroutine()
    {
           // isRotating = true;
                  Quaternion startRotation = objectToRotate.transform.rotation;


                    if (statusData.LR.Count == 1)
                    {
                        if (statusData.LR[0] == true)//元LorR == "R"
                        {
                            endRotation = startRotation * Quaternion.Euler(0, 90, 0);
                        }
                        else if (statusData.LR[0] == false)//元LorR == "L"
                        {
                            endRotation = startRotation * Quaternion.Euler(0, -90, 0);
                        }
                        else
                        {
                            endRotation = startRotation * Quaternion.Euler(0, 0, 0);
                        }

                    }
                    else if (statusData.LR.Count == 2)
                    {
                        if (statusData.LR[1] == false)//R
                        {
                            endRotation = startRotation * Quaternion.Euler(0, 90, 0);
                        }
                        else if (statusData.LR[1] == true)//L
                        {
                            endRotation = startRotation * Quaternion.Euler(0, -90, 0);
                        }
                        else
                        {
                            endRotation = startRotation * Quaternion.Euler(0, 0, 0);
                        }
                    }



                    float elapsedTime = 0;

                    while (elapsedTime < rotationDuration)
                    {
                        objectToRotate.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / rotationDuration);
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }objectToRotate.transform.rotation = endRotation;
         yield return null;
        isRotating = false;
        isAnswered = false;
        
        // 回転後にシーンを読み込む

    }*/
    /// <returns>True:偶数値　False:奇数値</returns>
    private bool IsEven(int num)
        {
            return (num % 2 == 0);
        }
    }
