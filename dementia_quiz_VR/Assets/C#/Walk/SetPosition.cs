using UnityEngine;
using App.BaseSystem.DataStores.ScriptableObjects.Status;

public class SetPosition : MonoBehaviour
{
    [SerializeField]
    StatusData statusData;

    void Start()
    {
        if (statusData.LR.Count > 0)
        {
            // �ۑ����ꂽ�ʒu���擾
            Vector3 savedPos = statusData.MoveThrottle;

            // �v���C���[�̈ʒu��ݒ�
            transform.position = savedPos;

            // �K�v�ɉ����ĉ�]��ݒ�
            Quaternion savedRotation = Quaternion.Euler(0, statusData.rotY, 0);
            transform.rotation = savedRotation;

            Debug.Log("Position reloaded");
            Debug.Log("pos.x: " + savedPos.x);
            Debug.Log("pos.z: " + savedPos.z);
        }
    }
}