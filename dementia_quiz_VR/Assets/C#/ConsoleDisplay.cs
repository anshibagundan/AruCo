using UnityEngine;
using TMPro;

public class ConsoleDisplay : MonoBehaviour
{
    public TextMeshProUGUI consoleText;

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
        consoleText.text += logString + "\n";
    }

    private void Update()
    {
        // テキストの自動スクロール
        consoleText.rectTransform.anchoredPosition = new Vector2(0, consoleText.rectTransform.sizeDelta.y);
    }
}