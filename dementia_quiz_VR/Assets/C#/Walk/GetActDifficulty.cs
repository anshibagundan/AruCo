/*
 * WalkingシーンでActionのidをgetする
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GetActDifficulty : MonoBehaviour
{
    private const string difficultyGetUrl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/act-selects/";
    private int ActId;

    public int GetQuizDifficulty()
    {
        StartCoroutine(GetUserOwnData());
        return ActId;
    }

    private IEnumerator GetUserOwnData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(difficultyGetUrl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;

                ActSelDiff[] ActSelDiffDataArray = JsonHelper.FromJson<ActSelDiff>(json);

                if (ActSelDiffDataArray != null && ActSelDiffDataArray.Length > 0)
                {
                    ActSelDiff ActSelDiffData = ActSelDiffDataArray[0];

                    ActId = ActSelDiffData.select_diff;
                }
                else
                {
                    Debug.LogWarning("No Actdiff found.");
                }
            }
        }
    }


}
