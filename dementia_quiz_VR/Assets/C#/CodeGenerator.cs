using UnityEngine;
using UnityEngine.UI;
using System;

public class CodeGenerator : MonoBehaviour
{
    public Text codeText; // 6桁のコードを表示するUIテキスト
    private string code;

    void Start()
    {
        // パーミッションのリクエスト
        RequestPermissions();

        // 6桁のコードを生成
        code = GenerateCode(6);
        codeText.text = "Your Code: " + code;

        // Bluetoothサーバーを開始
        StartBluetoothServer();
    }

    string GenerateCode(int length)
    {
        const string chars = "0123456789";
        var random = new System.Random();
        char[] stringChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    void StartBluetoothServer()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // ネイティブプラグインのクラスを取得
            AndroidJavaObject bluetoothServer = new AndroidJavaObject("com.example.bluetooth.BluetoothServer", activity, code);

            // サーバーを開始
            bluetoothServer.Call("startServer");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to start Bluetooth server: " + e.Message);
        }
#endif
    }

    void RequestPermissions()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string[] permissions = {
            "android.permission.BLUETOOTH",
            "android.permission.BLUETOOTH_ADMIN",
            "android.permission.ACCESS_FINE_LOCATION"
        };

        foreach (string permission in permissions)
        {
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
            {
                UnityEngine.Android.Permission.RequestUserPermission(permission);
            }
        }
#endif
    }

    // Javaプラグインから呼び出されるメソッド
    public void OnUUIDReceived(string uuid)
    {
        Debug.Log("Received UUID: " + uuid);
        // UUIDを処理（例：保存、認証、次のシーンへの遷移など）
    }
}