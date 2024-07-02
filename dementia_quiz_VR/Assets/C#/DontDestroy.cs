using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance;
    private Vector3 initialPosition;
    private Vector3 firstScene2Position;
    private bool hasInitialPosition = false;
    private bool hasFirstScene2Position = false;

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
        if (scene.name == "Scene1")
        {
            // シーン1に戻ったときに初期位置に戻す
            transform.position = initialPosition;
        }
        else if (scene.name == "Scene2" && !hasFirstScene2Position)
        {
            // 初回のシーン2の位置を記録する
            firstScene2Position = transform.position;
            hasFirstScene2Position = true;
        }
        else if (scene.name == "Scene2" && hasFirstScene2Position)
        {
            // シーン2に戻ったときに最初のシーン2の位置に戻す
            transform.position = firstScene2Position;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}