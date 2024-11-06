using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SimpleConsole : MonoBehaviour
{
    [SerializeField]
    private TMP_Text consoleText;

    [SerializeField]
    private int maxLines = 8;  // �\������ő�s��

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
            LogType.Error => "#FF0000",   // ��
            LogType.Warning => "#FFFF00", // ��
            LogType.Log => "#FFFFFF",     // ��
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

    // �f�o�b�O�p�̃e�X�g���b�Z�[�W�𑗐M���郁�\�b�h
    public void TestMessages()
    {
        Debug.Log("�ʏ�̃��O���b�Z�[�W");
        Debug.LogError("�G���[���b�Z�[�W");
    }
}