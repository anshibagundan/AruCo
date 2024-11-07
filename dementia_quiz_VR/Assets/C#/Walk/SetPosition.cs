using UnityEngine;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using UnityEngine.Assertions.Must;

public class SetPosition : MonoBehaviour
{
    [SerializeField]
    StatusData statusData;

    void Start()
    {
        if (statusData.LR.Count > 0)
        {
            

            // �K�v�ɉ����ĉ�]��ݒ�
            Quaternion savedRotation = Quaternion.Euler(0, statusData.rotY, 0);
            transform.rotation = savedRotation;

            Debug.Log("Position reloaded");
        }
    }
}