using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionZero : MonoBehaviour
{
    Vector3 position = Vector3.zero;
    void Update()
    {
        // �J�����̈ʒu��e�I�u�W�F�N�g�̈ʒu�ɒ��ڐݒ�
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
