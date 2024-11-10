using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionZero : MonoBehaviour
{
    Vector3 position = Vector3.zero;
    void Update()
    {
        // カメラの位置を親オブジェクトの位置に直接設定
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
