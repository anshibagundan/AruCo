using UnityEngine;
using App.BaseSystem.DataStores.ScriptableObjects.Status;

public class PlayerID : MonoBehaviour
{
    [HideInInspector] public int id;
    //public GetActDifficulty act;
    [SerializeField]
    private StatusData statusData;

    private void Start()
    {

        id =(int) statusData.ActDiff;
        //Å´égÇÌÇ»Ç¢
        //id = act.GetQuizDifficulty();
    }
}