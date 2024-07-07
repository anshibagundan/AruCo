using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GetPlayerDataLR : MonoBehaviour
{
    private static string positionurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/lrs/";
    private static PlayerLR currentPlayerLR = new PlayerLR();

    public static PlayerLR CurrentPlayerLR
    {
        get { return currentPlayerLR; }
    }

    public static IEnumerator UpdatePlayerData()
    {
        yield return GetPlayerCoroutine();
    }

    private static IEnumerator GetPlayerCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(positionurl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string json = webRequest.downloadHandler.text;
                Debug.Log("Received JSON: " + json);

                try
                {
                    PlayerLR[] playerDataLRArray = JsonUtility.FromJson<PlayerLRArray>("{\"lrs\":" + json + "}").lrs;

                    if (playerDataLRArray != null && playerDataLRArray.Length > 0)
                    {
                        currentPlayerLR = playerDataLRArray[0];
                        Debug.Log($"PlayerLR data updated: Position({currentPlayerLR.getLR()}), ID: {currentPlayerLR.id}");
                    }
                    else
                    {
                        Debug.LogWarning("PlayerData array is null or empty");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON parsing error: {e.Message}\nJSON: {json}");
                }
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
            }
        }
    }
}

[Serializable]
public class PlayerLRArray
{
    public PlayerLR[] lrs;
}

