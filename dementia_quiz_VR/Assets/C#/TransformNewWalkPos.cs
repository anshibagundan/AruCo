using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TransformNewWalkPos : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 firstScene2Position;
    private bool hasInitialPosition = false;
    private bool hasFirstScene2Position = false;
    private bool isFirstLoad = true; // ����ǂݍ��݂𔻒肷��t���O
    //PlayerPosition��url
    private String geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/";
    private String deleteurl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/players/destroy_all";

    //quizTF��url
    private String Geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-tfs/";

    Player playerData;
    Vector3 savedPosition;//DB����擾�������W���X�g�b�N����
    Quaternion savedRotation;//DB����擾������]���X�g�b�N����
    Vector3 initializedPosition;

    public Player getPlayerArray()
    {
        StartCoroutine(getPlayer());
        return playerData;
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

        Debug.Log("NewWalkScene Reloded");
        //getPlayer�̃R���[�e�B�����񂵁ADB������W�擾
        StartCoroutine(getPlayer());

        //GetTF�̃R���[�e�B�����񂵁ADB������񓚐����擾
        StartCoroutine(GetQuizTFCoroutine());

        //DB����擾�����������W�Ɖ�]�ɕ��z���đ��
        savedPosition.x = playerData.PosX;
        savedPosition.y = playerData.PosY;
        savedPosition.z = playerData.PosZ;

        savedRotation.x = playerData.RotX;
        savedRotation.y = playerData.RotY;
        savedRotation.z = playerData.RotZ;

        // �V�[��1�ɖ߂����Ƃ���DB�ɕۑ����Ă����ʒu�ɖ߂�
        transform.position = savedPosition;
        transform.rotation = savedRotation;
        rotatePlayer();
        Debug.Log("PositionLoaded");
        
    }

    private void rotatePlayer()
    {
        StartCoroutine(RotatePlayerCoroutine());
    }

    private IEnumerator RotatePlayerCoroutine()
    {
        // ��]����p�x��ݒ�
        float targetAngle = 0f;

        if (playerData.LR == "L")
        {
            targetAngle = 90f;
        }
        else if (playerData.LR == "R" && quizTFCount < 3)
        {
            targetAngle = -90f;
        }

        // ���݂̊p�x
        float startAngle = transform.eulerAngles.y;
        float endAngle = startAngle + targetAngle;
        float elapsedTime = 0f;
        float duration = 2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, elapsedTime / duration);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentAngle, transform.eulerAngles.z);
            yield return null;
        }

        // �Ō�ɐ��m�Ȋp�x��ݒ�
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, endAngle, transform.eulerAngles.z);
    }

    //DB����position���擾
    private IEnumerator getPlayer()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(geturl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;

                Player[] PlayerDataArray = JsonHelper.FromJson<Player>(json);

                if (PlayerDataArray != null && PlayerDataArray.Length > 0)
                {
                    playerData = PlayerDataArray[0];
                }
                else
                {
                    Debug.LogWarning("No Askedquiz found.");
                }
            }
        }
    }

    //�������璷���擾
    private int quizTFCount = 0;

    private IEnumerator GetQuizTFCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Geturl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                quizTFCount = 0;
            }
            else
            {
                string json = webRequest.downloadHandler.text;
                QuizTF[] quizTFDataArray = JsonUtility.FromJson<QuizTF[]>("{\"Items\":" + json + "}");
                quizTFCount = quizTFDataArray.Length;
            }
        }
    }
}
