using UnityEngine;

public class SetD : MonoBehaviour
{
    [HideInInspector] public int id;
    public GetActDifficulty act;

    private void Start()
    {
        id = act.GetQuizDifficulty();
    }
}