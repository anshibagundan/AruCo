using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChangeQuizScene : MonoBehaviour
{
    //PlayerPos��PostUrl
    private String posturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    //quizTF��Geturl
    private const String Geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-tfs/";
    private const String deleteurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/destroy_all/";
    Player playerData;
    private Vector3 position;
    private Vector3 eulerRotation;
    //�������璷���擾
    private int quizTFCount = 0;

    void Start()
    {
        // GameObject�̈ʒu�Ɖ�]��������
        
        position = transform.position;
        eulerRotation = transform.eulerAngles;
        StartCoroutine(GetQuizTFCoroutine());

              
    }
    
    //Quiz�R���C�_�[����
    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("quizTFCount(QuizScene初め): " + quizTFCount);
        if (other.gameObject.CompareTag("QuizCollider_1st") && quizTFCount == 0)
        {
            Debug.Log("OnTriggerEnter called");
            Debug.Log(quizTFCount);
            Debug.Log("1stQuizCollider detected");
            StartCoroutine(deletePlayer());
            StartCoroutine(postPlayer());
            SceneManager.LoadScene("QuizScene");
        }
        else if (other.gameObject.CompareTag("QuizCollider_2nd") && quizTFCount == 1)
        {
            Debug.Log("OnTriggerEnter called");
            Debug.Log(quizTFCount);
            Debug.Log("2ndQuizCollider detected");
            StartCoroutine(deletePlayer());
            StartCoroutine(postPlayer());
            SceneManager.LoadScene("QuizScene");
        }
        else if (other.gameObject.CompareTag("QuizCollider_3rd") && quizTFCount == 2)
        {
            Debug.Log("OnTriggerEnter called");
            Debug.Log(quizTFCount);
            Debug.Log("3rdQuizCollider detected");
            StartCoroutine(deletePlayer());
            StartCoroutine(postPlayer());
            SceneManager.LoadScene("QuizScene");
        }
        else if (other.gameObject.CompareTag("QuizCollider_final") && quizTFCount == 2)
        {

        }
        else
        {
            Debug.Log(quizTFCount);
            Debug.Log("You missed something");
        }
    }
    //�f�[�^���M
    public IEnumerator postPlayer()
    {
        WWWForm form = new WWWForm();
        form.AddField("pos_x", transform.position.x.ToString());
        form.AddField("pos_y", transform.position.y.ToString());
        form.AddField("pos_z", transform.position.z.ToString());
        form.AddField("rot_x", transform.rotation.eulerAngles.x.ToString());
        form.AddField("rot_y", transform.rotation.eulerAngles.y.ToString());
        form.AddField("rot_z", transform.rotation.eulerAngles.z.ToString());

        using (UnityWebRequest webRequest = UnityWebRequest.Post(posturl, form))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("POST successful. Response: " + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
            }
        }
    }


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
                    Debug.Log("quizTFCount(QuizScene): " + quizTFCount);
                }
                else
                {
                    Debug.LogError("Failed to parse JSON or Items array is null");
                    quizTFCount = 0;
                }
            }
        }
    }

    //AllDelete Pos
    public IEnumerator deletePlayer()
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
}