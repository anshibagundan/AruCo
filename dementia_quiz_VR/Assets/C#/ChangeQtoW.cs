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
        // シーンのロード時に呼ばれるイベントに登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // シーンのロード時に呼ばれるイベントから解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        initializedPosition.x = 1220;
        initializedPosition.y = 90;
        initializedPosition.z = 0;


        // クイズシーン読み込み時、初期位置へ
        transform.position = initializedPosition;
        Debug.Log("PositionInitialized");

    }
}

  
