using UnityEngine;
using TMPro;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using System.Collections;

public class SerialNumDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text displayText;

    [SerializeField]
    private StatusData statusData;

    [Header("Update Settings")]
    [SerializeField]
    private float updateInterval = 2f; // 更新間隔（秒）

    private void Awake()
    {
        if (displayText == null || statusData == null)
        {
            Debug.LogError("Required components are not assigned!");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateRoutine()
    {
        while (true)
        {
            UpdateDisplay();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdateDisplay()
    {
        displayText.text = $"Serial Number: {statusData.SerialNum}";
    }
}