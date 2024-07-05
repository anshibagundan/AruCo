using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChangeQuizScene : MonoBehaviour
{
    private bool IsChanged_1st = false;
    private bool IsChanged_2nd = false;
    private bool IsChanged_3rd = false;
    private String geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    Player playerData;
    private Vector3 position;
    private Vector3 eulerRotation;

    void Start()
    {
        // GameObjectの位置と回転を初期化
        position = transform.position;
        eulerRotation = transform.eulerAngles;
              
    }

    // Start is called before the first frame update
    
    //Quizコライダー処理
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called");
        
        if (other.gameObject.CompareTag("QuizCollider_1st") && IsChanged_1st == false)
        {
            Debug.Log(IsChanged_1st);
            Debug.Log(IsChanged_2nd);
            Debug.Log(IsChanged_3rd);
            Debug.Log("1stQuizCollider detected");
            StartCoroutine(postPlayer());
            SceneManager.LoadScene("QuizScene");
            IsChanged_1st = true;
        }
        else if (other.gameObject.CompareTag("QuizCollider_2nd") && IsChanged_2nd == false)
        {
            Debug.Log(IsChanged_1st);
            Debug.Log(IsChanged_2nd);
            Debug.Log(IsChanged_3rd);
            Debug.Log("2ndQuizCollider detected");
            StartCoroutine(postPlayer());
            SceneManager.LoadScene("QuizScene");
            IsChanged_2nd = true;
        }
        else if (other.gameObject.CompareTag("QuizCollider_3rd") && IsChanged_3rd == false)
        {
            Debug.Log(IsChanged_1st);
            Debug.Log(IsChanged_2nd);
            Debug.Log(IsChanged_3rd);
            Debug.Log("3rdQuizCollider detected");
            StartCoroutine(postPlayer());
            SceneManager.LoadScene("QuizScene");
            IsChanged_3rd = true;
        }
    }
    //データ送信
    public IEnumerator postPlayer()
    {
        WWWForm form = new WWWForm();
        form.AddField("pos_x", "transform.position.x");
        form.AddField("pos_y", "transform.position.y");
        form.AddField("pos_z", "transform.position.z");
        form.AddField("rot_x", "transform.rotation.x");
        form.AddField("rot_y", "transform.rotation.y");
        form.AddField("rot_z", "transform.rotation.z");

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
}