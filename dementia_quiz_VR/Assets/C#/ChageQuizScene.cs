using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeQuizScene : MonoBehaviour
{
    private bool IsChanged = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called"); // ?f?o?b?O???O???
        if (other.gameObject.CompareTag("QuizCollider") && IsChanged == false)
        {
            Debug.Log("QuizCollider detected"); // ?f?o?b?O???O???
            SceneManager.LoadScene("QuizScene");
            IsChanged = true;
        }
    }
}
