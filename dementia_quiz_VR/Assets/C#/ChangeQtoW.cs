using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TransformQuizPos : MonoBehaviour
{
    Vector3 initializedPosition;
     
    void OnEnable()
    {
        // �V�[���̃��[�h���ɌĂ΂��C�x���g�ɓo�^
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // �V�[���̃��[�h���ɌĂ΂��C�x���g�������
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        initializedPosition.x = 1220;
        initializedPosition.y = 90;
        initializedPosition.z = 0;


        // �N�C�Y�V�[���ǂݍ��ݎ��A�����ʒu��
        transform.position = initializedPosition;
        Debug.Log("PositionInitialized");

    }
}

  
