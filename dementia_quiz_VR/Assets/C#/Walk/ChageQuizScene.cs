using UnityEngine;
using UnityEngine.SceneManagement;
using App.BaseSystem.DataStores.ScriptableObjects.Status;

public class ChangeQuizScene : MonoBehaviour
{
    [SerializeField]
    StatusData statusData;
    private void OnTriggerEnter(Collider other)
    {
        Vector3 transform = other.transform.position; 
        Debug.Log("OnTriggerEnter called");
        Debug.Log(statusData.LR.Count);
        if (other.gameObject.CompareTag("1st_QuizCollider") && statusData.LR.Count == 0)
        {
            statusData.X = transform.x;
            statusData.Z = transform.z;
            Debug.Log("1st_QuizCollider detected");
            SceneManager.LoadScene("QuizScene");
        }
        if (other.gameObject.CompareTag("2nd_QuizCollider") && statusData.LR.Count == 1)
        {
            statusData.X = transform.x;
            statusData.Z = transform.z;
            Debug.Log("2nd_QuizCollider detected");
            SceneManager.LoadScene("QuizScene");
        }
        if (other.gameObject.CompareTag("3rd_QuizCollider") && statusData.LR.Count == 2)
        {
            statusData.X = transform.x;
            statusData.Z = transform.z;
            Debug.Log("3rd_QuizCollider detected");
            SceneManager.LoadScene("QuizScene");
        }
        else if(other.gameObject.CompareTag("Final_QuizCollider") && statusData.LR.Count == 3)
        {
            statusData.X = transform.x;
            statusData.Z = transform.z;
            Debug.Log("Final_QuizCollider detected");
            SceneManager.LoadScene("FinalQuizScene");
        }
    }
}
