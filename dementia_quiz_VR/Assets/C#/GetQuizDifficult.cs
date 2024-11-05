using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class GetQuizDifficulty : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    private const string url = "https://teamhopcard-aa92d1598b3a.herokuapp.com/act-selects/";

    public void FetchData()
    {
        StartCoroutine(GetData());
    }

    private IEnumerator GetData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
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

                QuizDifficulty[] quizDataArray = JsonHelper.FromJson<QuizDifficulty>(json);

                if (quizDataArray != null && quizDataArray.Length > 0)
                {
                    QuizDifficulty firstQuizData = quizDataArray[0];
                    resultText.text = "Difficulty: " + firstQuizData.select_diff.ToString();
                }
                else
                {
                    Debug.LogWarning("No quiz difficulty found.");
                }
            }
        }
    }
}

[System.Serializable]
public class QuizDifficulty
{
    public int id;
    public int select_diff;
}