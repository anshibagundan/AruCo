using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TransformNewWalkPos : MonoBehaviour
{
    //PlayerPositionのurl
    private const String geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    private const String deleteurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/destroy_all/";
    //quizTFのurl
    private const String Geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-tfs/";
    // PlayerLRのURL
    private const string getUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/lrs/";
    private const string deleteUrl = "http://teamhopcard-aa92d1598b3a.herokuapp.com/lrs/destroy_all/";

    Player playerData;
    PlayerLR playerLR;

    public Vector3 savedPosition;//DBから取得した座標をストックする
    public Quaternion savedRotation;//DBから取得した回転をストックする

    public Player getPlayerArray()
    {
        StartCoroutine(getPlayer());
        return playerData;
    }
    public PlayerLR getPlayerLRArray()
    {
        StartCoroutine(getLR());
        return playerLR;
    }

    void OnEnable()
    {
        // シーンのロード時に呼ばれるイベントに登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // シーンのロード時に呼ばれるイベントから解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("NewWalkScene Reloaded");

        // すべてのコルーチンが完了するまで待機
        StartCoroutine(InitializeDataCoroutine());
    }

    IEnumerator InitializeDataCoroutine()
    {
        yield return StartCoroutine(getPlayer());
        yield return StartCoroutine(GetQuizTFCoroutine());
        yield return StartCoroutine(getLR());

        // データが取得できたことを確認
        if (playerData != null)
        {
            savedPosition.x = playerData.getPosX();
            savedPosition.y = playerData.getPosY();
            savedPosition.z = playerData.getPosZ();

            savedRotation.x = playerData.getRotX();
            savedRotation.y = playerData.getRotY();
            savedRotation.z = playerData.getRotZ();

            Debug.Log(savedPosition);

            // シーン1に戻ったときにDBに保存していた位置に戻る
            transform.position = savedPosition;
            transform.rotation = savedRotation;
            rotatePlayer();
            Debug.Log("PositionLoaded");

            StartCoroutine(deletePlayerPos());
            StartCoroutine(deletePlayerLR());
        }
        else
        {
            Debug.LogError("playerData is null");
        }
    }

    private void rotatePlayer()
    {
        StartCoroutine(RotatePlayerCoroutine());
    }

    private IEnumerator RotatePlayerCoroutine()
    {
        // 回転する角度を設定
        float targetAngle = 0f;


        if (playerLR.getLR() == "L")
        {
            targetAngle = 90f;
        }
        else if (playerLR.getLR() == "R" && quizTFCount < 3)
        {
            targetAngle = -90f;
        }

        // 現在の角度
        float startAngle = transform.eulerAngles.y;
        float endAngle = startAngle + targetAngle;
        float elapsedTime = 0f;
        float duration = 2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, elapsedTime / duration);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentAngle, transform.eulerAngles.z);
            yield return null;
        }
        Debug.Log("rotationing...");
        // 最後に正確な角度を設定
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, endAngle, transform.eulerAngles.z);
    }

    //DBからpositionを取得
    private IEnumerator getPlayer()
    {
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

                Player[] PlayerDataArray = JsonHelper.FromJson<Player>(json);

                if (PlayerDataArray != null && PlayerDataArray.Length > 0)
                {
                    playerData = PlayerDataArray[0];
                }
                else
                {
                    Debug.LogWarning("No Askedquiz found.");
                }
            }
        }
    }

    //LR取得
    private IEnumerator getLR()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(getUrl))
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

                PlayerLR[] PlayerLR = JsonHelper.FromJson<PlayerLR>(json);

                if (PlayerLR != null && PlayerLR.Length > 0)
                {
                    playerLR = PlayerLR[0];
                }
                else
                {
                    Debug.LogWarning("No Askedquiz found.");
                }
            }
        }
    }

    //AllDelete Pos
    public IEnumerator deletePlayerPos()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Delete(deleteurl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
        }
    }

    //AllDelete LR
    public IEnumerator deletePlayerLR()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Delete(deleteUrl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
        }
    }

    //ここから長さ取得
    private int quizTFCount = 0;

    [System.Serializable]
    public class QuizTFWrapper
    {
        public QuizTF[] Items;
    }

    private IEnumerator GetQuizTFCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Geturl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                quizTFCount = 0;
            }
            else
            {
                string json = webRequest.downloadHandler.text;
                QuizTFWrapper wrapper = JsonUtility.FromJson<QuizTFWrapper>("{\"Items\":" + json + "}");

                if (wrapper != null && wrapper.Items != null)
                {
                    quizTFCount = wrapper.Items.Length;
                }
                else
                {
                    Debug.LogError("Failed to parse JSON or Items array is null");
                    quizTFCount = 0;
                }
            }
        }
    }
}
