using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.BaseSystem.DataStores.ScriptableObjects.Status;

public class CountDistance : MonoBehaviour
{
    [SerializeField]
    StatusData statusData;
    void Start()
    {
        Vector3 curposition = new Vector3();
        curposition = transform.position;
        statusData.PastPosition = curposition;
    }
}
