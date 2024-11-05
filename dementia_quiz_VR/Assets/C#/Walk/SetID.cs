using UnityEngine;
using App.BaseSystem.DataStores.ScriptableObjects.Status;

public class PlayerID : MonoBehaviour
{
    [HideInInspector]
    public int id;

    [SerializeField]
    private StatusData statusData;  

    private void Start()
    {
        if (statusData != null)
        {
            id = (int)statusData.ActDiff;
        }
        else
        {
            Debug.LogError($"StatusData is not assigned on {gameObject.name}");
            id = 0;  // デフォルト値を設定
        }
    }
}