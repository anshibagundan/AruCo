using UnityEngine;
using UnityEngine.UI;
using System;

public class CodeGenerator : MonoBehaviour
{
    public Text codeText; // 6���̃R�[�h��\������UI�e�L�X�g
    private string code;

    void Start()
    {
        // �p�[�~�b�V�����̃��N�G�X�g
        RequestPermissions();

        // 6���̃R�[�h�𐶐�
        code = GenerateCode(6);
        codeText.text = "Your Code: " + code;

        // Bluetooth�T�[�o�[���J�n
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

            // �l�C�e�B�u�v���O�C���̃N���X���擾
            AndroidJavaObject bluetoothServer = new AndroidJavaObject("com.example.bluetooth.BluetoothServer", activity, code);

            // �T�[�o�[���J�n
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

    // Java�v���O�C������Ăяo����郁�\�b�h
    public void OnUUIDReceived(string uuid)
    {
        Debug.Log("Received UUID: " + uuid);
        // UUID�������i��F�ۑ��A�F�؁A���̃V�[���ւ̑J�ڂȂǁj
    }
}