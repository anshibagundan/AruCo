using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SimpleConsole : MonoBehaviour
{
    [SerializeField]
    private TMP_Text consoleText;

    [SerializeField]
    private int maxLines = 8;  // 表示する最大行数

    private Queue<string> logMessages = new Queue<string>();

    private void Awake()
    {
        if (consoleText == null)
        {
            Debug.LogError("TMP_Text component is not assigned!");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string color = type switch
        {
            LogType.Error => "#FF0000",   // 赤
            LogType.Warning => "#FFFF00", // 黄
            LogType.Log => "#FFFFFF",     // 白
            _ => "#FFFFFF"
        };

        string message = $"<color={color}>{logString}</color>";
        logMessages.Enqueue(message);

        while (logMessages.Count > maxLines)
        {
            logMessages.Dequeue();
        }

        consoleText.text = string.Join("\n", logMessages);
    }

    // デバッグ用のテストメッセージを送信するメソッド
    public void TestMessages()
    {
        Debug.Log("通常のログメッセージ");
        Debug.LogError("エラーメッセージ");
    }
}