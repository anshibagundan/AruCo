using UnityEngine;
using UnityEngine.SceneManagement;
using App.BaseSystem.DataStores.ScriptableObjects.Status;

public class ChangeQuizScene : MonoBehaviour
{
    [SerializeField]
    StatusData statusData;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーの現在の位置と回転を取得
        Vector3 currentPosition = other.transform.position;
        Quaternion currentRotation = other.transform.rotation;

        Debug.Log("OnTriggerEnter called");
        Debug.Log(statusData.LR.Count);

        if (other.gameObject.CompareTag("1st_QuizCollider") && statusData.LR.Count == 0)
        {
            Debug.Log("1st_QuizCollider detected");
            UpdatePositionAndLoadQuiz(currentPosition, currentRotation);
        }
        if (other.gameObject.CompareTag("2nd_QuizCollider") && statusData.LR.Count == 1)
        {
            Debug.Log("2nd_QuizCollider detected");
            UpdatePositionAndLoadQuiz(currentPosition, currentRotation);
        }
        if (other.gameObject.CompareTag("3rd_QuizCollider") && statusData.LR.Count == 2)
        {
            UpdatePositionAndLoadQuiz(currentPosition, currentRotation);
            Debug.Log("3rd_QuizCollider detected");
        }
        else if (other.gameObject.CompareTag("Final_QuizCollider") && statusData.LR.Count == 3)
        {
            Debug.Log("Final_QuizCollider detected");
            SceneManager.LoadScene("FinalQuizScene");
        }
    }

    void UpdatePositionAndLoadQuiz(Vector3 position, Quaternion rotation)
    {
        // プレイヤーの位置を保存
        statusData.MoveThrottle = new Vector3(position.x, position.y, position.z);
        // Y軸の回転を保存(こっちの方がいいらしい)
        statusData.rotY = rotation.eulerAngles.y;
        // クイズシーンをロード
        SceneManager.LoadScene("QuizScene");
    }
}