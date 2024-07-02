using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance;
    private Vector3 initialPosition;
    private bool hasInitialPosition = false;

    void Awake()
    {
        // シングルトンパターンを使用して、インスタンスを一つに保つ
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 初回シーン1の位置を記録する
        if (!hasInitialPosition)
        {
            initialPosition = transform.position;
            hasInitialPosition = true;
        }
    }

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
        // シーン1に戻ったときに初期位置に戻す
        if (scene.name == "New_Walk")
        {
            transform.position = initialPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}