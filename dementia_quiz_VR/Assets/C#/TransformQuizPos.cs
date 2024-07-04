using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChangeQtoW : MonoBehaviour
{
    private String geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    private String deleteurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/destroy_all";
    Player playerData;
    // Start is called before the first frame update
    public Player getPlayerArray()
    {
        StartCoroutine(getPlayer());
        return playerData;
    }
    private IEnumerator getPlayer() {
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

    public IEnumerator postPlayer()
    {
        WWWForm form = new WWWForm();
        form.AddField("pos_x","Ç±Ç±Ç…åªç›ínÇì¸ÇÍÇÈ");
        using (UnityWebRequest webRequest = UnityWebRequest.Post(geturl,form))
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
            else
            {
                string json = webRequest.downloadHandler.text;

                Player[] PlayerDataArray = JsonHelper.FromJson<Player>(json);

                if (PlayerDataArray != null && PlayerDataArray.Length > 0){}
                else
                {
                    Debug.LogWarning("No Askedquiz found.");
                }
            }

        }

    }
}
