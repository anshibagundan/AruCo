using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using Unity.VisualScripting;

public class SetPosition : MonoBehaviour
{
    [SerializeField]
    StatusData statusData;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = transform.position;
        if (statusData.LR.Count > 0)
        {
            pos.x = statusData.X;
            pos.z = statusData.Z;
            transform.position = pos;
            Debug.Log("position reloaded");
        }
    }
}
