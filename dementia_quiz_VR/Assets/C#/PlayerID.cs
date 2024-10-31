using UnityEngine;

public class PlayerID : MonoBehaviour
{
    [HideInInspector] public int id;
    public GetActDifficulty act;

    private void Start()
    {
        id = act.GetQuizDifficulty();
    }
}