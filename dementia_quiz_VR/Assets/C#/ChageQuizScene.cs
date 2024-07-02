using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeQuizScene : MonoBehaviour
{
    private bool IsChanged_1st = false;
    private bool IsChanged_2nd = false;
    private bool IsChanged_3rd = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called"); // ?f?o?b?O???O???
        if (other.gameObject.CompareTag("QuizCollider_1st") && IsChanged_1st == false)
        {
            Debug.Log("QuizCollider detected"); //
            SceneManager.LoadScene("QuizScene");
            IsChanged_1st = true;
        }
        else if (other.gameObject.CompareTag("QuizCollider_2nd") && IsChanged_2nd == false)
        {
            Debug.Log("QuizCollider detected"); //
            SceneManager.LoadScene("QuizScene");
            IsChanged_2nd = true;
        }
        else if (other.gameObject.CompareTag("QuizCollider_3rd") && IsChanged_3rd == false)
        {
            Debug.Log("QuizCollider detected"); //
            SceneManager.LoadScene("QuizScene");
            IsChanged_3rd = true;
        }
    }
}
