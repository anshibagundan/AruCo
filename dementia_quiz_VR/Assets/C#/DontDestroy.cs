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
        // �V���O���g���p�^�[�����g�p���āA�C���X�^���X����ɕۂ�
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
        // ����V�[��1�̈ʒu���L�^����
        if (!hasInitialPosition)
        {
            initialPosition = transform.position;
            hasInitialPosition = true;
        }
    }

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
        if (scene.name == "Scene1")
        {
            // �V�[��1�ɖ߂����Ƃ��ɏ����ʒu�ɖ߂�
            transform.position = initialPosition;
        }
        else if (scene.name == "Scene2" && !hasFirstScene2Position)
        {
            // ����̃V�[��2�̈ʒu���L�^����
            firstScene2Position = transform.position;
            hasFirstScene2Position = true;
        }
        else if (scene.name == "Scene2" && hasFirstScene2Position)
        {
            // �V�[��2�ɖ߂����Ƃ��ɍŏ��̃V�[��2�̈ʒu�ɖ߂�
            transform.position = firstScene2Position;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}