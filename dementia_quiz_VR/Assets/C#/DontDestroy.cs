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
        // �V�[��1�ɖ߂����Ƃ��ɏ����ʒu�ɖ߂�
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