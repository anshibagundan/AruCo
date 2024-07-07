using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GetPlayerData : MonoBehaviour
{
    private static string positionurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    private static Player currentPlayer = new Player();

    // Playerオブジェクトを取得するためのプロパティ
    public static Player CurrentPlayer
    {
        get { return currentPlayer; }
    }

    // データを更新するメソッド
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

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;
                Player[] playerDataArray = JsonUtility.FromJson<PlayerArray>("{\"players\":" + json + "}").players;

                if (playerDataArray != null && playerDataArray.Length > 0)
                {
                    currentPlayer = playerDataArray[0];
                    Debug.Log($"Player data updated: Position({currentPlayer.getPosX()}, {currentPlayer.getPosY()}, {currentPlayer.getPosZ()}), " +
                              $"Rotation({currentPlayer.getRotX()}, {currentPlayer.getRotY()}, {currentPlayer.getRotZ()})");
                }
                else
                {
                    Debug.LogWarning("Failed to parse JSON or PlayerData is null");
                }
            }
        }
    }
}

[Serializable]
public class PlayerArray
{
    public Player[] players;
}