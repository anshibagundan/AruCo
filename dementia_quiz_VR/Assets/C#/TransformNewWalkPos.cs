using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TransformNewWalkPos : MonoBehaviour
{
    //PlayerPositionのurl
    private String geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    private String deleteurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/destroy_all";
    //quizTFのurl
    private String Geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-tfs/";
    // PlayerLRのURL
    private const string getUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/lrs/";
    private const string deleteUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/lrs/destroy_all";

    Player playerData;
    Player playerLR;

    Vector3 savedPosition;//DBから取得した座標をストックする
    Quaternion savedRotation;//DBから取得した回転をストックする

      public Player getPlayerArray()
    {
        StartCoroutine(getPlayer());
        return playerData;
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

        Debug.Log("NewWalkScene Reloded");
        //getPlayerのコルーティンを回し、DBから座標取得
        StartCoroutine(getPlayer());

        //GetTFのコルーティンを回し、DBから問題回答数数取得
        StartCoroutine(GetQuizTFCoroutine());

        //getLRのコルーティンを回し、DBからLR取得
        StartCoroutine(getLR());

        //DBとのやり取りが確認できないため一時的にコメントアウト

        //DBから取得した情報を座標と回転に分配して代入
        savedPosition.x = playerData.PosX;
        savedPosition.y = playerData.PosY;
        savedPosition.z = playerData.PosZ;

        savedRotation.x = playerData.RotX;
        savedRotation.y = playerData.RotY;
        savedRotation.z = playerData.RotZ;

        // シーン1に戻ったときにDBに保存していた位置に戻す
        transform.position = savedPosition;
        transform.rotation = savedRotation;
        rotatePlayer();
        Debug.Log("PositionLoaded");
        StartCoroutine(deletePlayerPos());
        StartCoroutine(deletePlayerLR());
    }

    private void rotatePlayer()
    {
        StartCoroutine(RotatePlayerCoroutine());
    }

    private IEnumerator RotatePlayerCoroutine()
    {
        // 回転する角度を設定
        float targetAngle = 0f;


        if (playerLR.LR == "L")
        {
            targetAngle = 90f;
        }
        else if (playerLR.LR == "R" && quizTFCount < 3)
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

                Player[] PlayerLR = JsonHelper.FromJson<Player>(json);

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
            else
            {
                string json = webRequest.downloadHandler.text;

                Player[] PlayerDataArray = JsonHelper.FromJson<Player>(json);

                if (PlayerDataArray != null && PlayerDataArray.Length > 0) { }
                else
                {
                    Debug.LogWarning("No Askedquiz found.");
                }
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
            else
            {
                string json = webRequest.downloadHandler.text;

                Player[] PlayerDataArray = JsonHelper.FromJson<Player>(json);

                if (PlayerDataArray != null && PlayerDataArray.Length > 0) { }
                else
                {
                    Debug.LogWarning("No Askedquiz found.");
                }
            }
        }
    }

    //ここから長さ取得
    private int quizTFCount = 0;

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
                QuizTF[] quizTFDataArray = JsonUtility.FromJson<QuizTF[]>("{\"Items\":" + json + "}");
                quizTFCount = quizTFDataArray.Length;
            }
        }
    }
}
