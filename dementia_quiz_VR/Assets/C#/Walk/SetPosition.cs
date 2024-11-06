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
            // 保存された位置を取得
            Vector3 savedPos = statusData.MoveThrottle;

            // プレイヤーの位置を設定
            transform.position = savedPos;

            // 必要に応じて回転を設定
            Quaternion savedRotation = Quaternion.Euler(0, statusData.rotY, 0);
            transform.rotation = savedRotation;

            Debug.Log("Position reloaded");
            Debug.Log("pos.x: " + savedPos.x);
            Debug.Log("pos.z: " + savedPos.z);
        }
    }
}