using App.BaseSystem.DataStores.ScriptableObjects.Status;
using UnityEngine;

public class TriggerSpawn : MonoBehaviour
{
    private SpawnDog spawnDogScript;

    private void Start()
    {
        spawnDogScript = FindObjectOfType<SpawnDog>();

        if (spawnDogScript == null)
        {
            Debug.LogError("SpawnDog script not found in the scene.");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DogCollider"))
        {
            TriggerSpawnDogs(other);
        }
    }

    private void TriggerSpawnDogs(Collider other)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        int id = GetIdFromPlayer(player);//playerID���擾
            //ID�ɂ���ď�������
            switch (id)
        {
            case 1:
                SpawnSpace1(0);
                SpawnSpace2(3);
                SpawnSpace3(0);
                break;
            case 2:
                SpawnSpace1(2);
                SpawnSpace2(0);
                SpawnSpace3(0);
                break;
            case 3:
                SpawnSpace1(0);
                SpawnSpace2(5);
                SpawnSpace3(0);
                break;
            case 4:
                SpawnSpace1(1);
                SpawnSpace2(0);
                SpawnSpace3(0);
                break;
            case 5:
                SpawnSpace1(0);
                SpawnSpace2(2);
                SpawnSpace3(0);
                break;
            case 6:
                SpawnSpace1(0);
                SpawnSpace2(0);
                SpawnSpace3(3);
                break;
            case 7:
                SpawnSpace1(1);
                SpawnSpace2(2);
                SpawnSpace3(0);
                break;
            case 8:
                SpawnSpace1(0);
                SpawnSpace2(2);
                SpawnSpace3(3);
                break;
            case 9:
                SpawnSpace1(2);
                SpawnSpace2(0);
                SpawnSpace3(2);
                break;
            case 10:
                SpawnSpace1(3);
                SpawnSpace2(3);
                SpawnSpace3(0);
                break;
            case 11:
                SpawnSpace1(1);
                SpawnSpace2(0);
                SpawnSpace3(4);
                break;
            case 12:
                SpawnSpace1(0);
                SpawnSpace2(3);
                SpawnSpace3(3);
                break;
            case 13:
                SpawnSpace1(1);
                SpawnSpace2(1);
                SpawnSpace3(1);
                break;
            case 14:
                SpawnSpace1(2);
                SpawnSpace2(3);
                SpawnSpace3(1);
                break;
            case 15:
                SpawnSpace1(3);
                SpawnSpace2(3);
                SpawnSpace3(1);
                break;
            case 16:
                SpawnSpace1(2);
                SpawnSpace2(2);
                SpawnSpace3(3);
                break;
            case 17:
                SpawnSpace1(1);
                SpawnSpace2(3);
                SpawnSpace3(5);
                break;
            case 18:
                SpawnSpace1(3);
                SpawnSpace2(3);
                SpawnSpace3(2);
                break;

        }

    }

    private void SpawnSpace1(int num)
    {
        Vector3 baseSpawnPosition = new Vector3(77f, 60.6f, -1044.5f);
        Vector3 targetPosition = new Vector3(77f, 60.6f, -872.7f);
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        float moveSpeed = 50;

        GameObject dog = GameObject.FindGameObjectWithTag("Dog");
        spawnDogScript.dogPrefab = dog;
        spawnDogScript.baseSpawnPosition = baseSpawnPosition;
        spawnDogScript.targetPosition = targetPosition;
        spawnDogScript.moveSpeed = moveSpeed;
        spawnDogScript.numberOfDogs = num;
        spawnDogScript.rotation = rotation;

        // �����o��������
        spawnDogScript.SpawnDogs();
    }

    private void SpawnSpace2(int num)
    {   //1�ӏ���
        Vector3 baseSpawnPosition1 = new Vector3(666.2995f, 68.11707f, -365.1902f);
        Vector3 targetPosition1 = new Vector3(438.2995f, 68.11707f, -365.1902f);
        Quaternion rotation = Quaternion.Euler(0, 270, 0);
        float moveSpeed = 50;

        GameObject dog = GameObject.FindGameObjectWithTag("Dog");
        spawnDogScript.dogPrefab = dog;
        spawnDogScript.baseSpawnPosition = baseSpawnPosition1;
        spawnDogScript.targetPosition = targetPosition1;
        spawnDogScript.moveSpeed = moveSpeed;
        spawnDogScript.numberOfDogs = num;
        spawnDogScript.rotation = rotation;

        // �����o��������
        spawnDogScript.SpawnDogs();


        //2�ӏ���
        Vector3 baseSpawnPosition2 = new Vector3(666.2995f, 68.11707f, -1535.8f);
        Vector3 targetPosition2 = new Vector3(438.2995f, 68.11707f, -1535.8f);

        spawnDogScript.baseSpawnPosition = baseSpawnPosition2;
        spawnDogScript.targetPosition = targetPosition2;

        // �����o��������
        spawnDogScript.SpawnDogs();


    }

    private void SpawnSpace3(int num)
    {
        //1�ӏ���
        Vector3 baseSpawnPosition = new Vector3(-2f, 63f, 487f);
        Vector3 targetPosition = new Vector3(-2f, 63f, 290.899994f);
        Quaternion rotation1 = Quaternion.Euler(0, 180, 0);
        float moveSpeed = 50;

        GameObject dog = GameObject.FindGameObjectWithTag("Dog");
        spawnDogScript.dogPrefab = dog;
        spawnDogScript.baseSpawnPosition = baseSpawnPosition;
        spawnDogScript.targetPosition = targetPosition;
        spawnDogScript.moveSpeed = moveSpeed;
        spawnDogScript.numberOfDogs = num;
        spawnDogScript.rotation = rotation1;
        // �����o��������
        spawnDogScript.SpawnDogs();

        //2�ӏ���
        Vector3 baseSpawnPosition2 = new Vector3(1407f, 63f, 494.299988f);
        Vector3 targetPosition2 = new Vector3(1407f, 63f, 282.700012f);

        spawnDogScript.baseSpawnPosition = baseSpawnPosition2;
        spawnDogScript.targetPosition = targetPosition2;
        // �����o��������
        spawnDogScript.SpawnDogs();

        //3�ӏ���
        Vector3 baseSpawnPosition3 = new Vector3(-632f, 64.3000031f, -1906.09998f);
        Vector3 targetPosition3 = new Vector3(-632f, 64.3000031f, -2120f);

        spawnDogScript.baseSpawnPosition = baseSpawnPosition3;
        spawnDogScript.targetPosition = targetPosition3;
        // �����o��������
        spawnDogScript.SpawnDogs();

        //4�ӏ���
        Vector3 baseSpawnPosition4 = new Vector3(1671f, 64.3f, -1892f);
        Vector3 targetPosition4 = new Vector3(1671f, 64.3f, -2124f);

        spawnDogScript.baseSpawnPosition = baseSpawnPosition4;
        spawnDogScript.targetPosition = targetPosition4;
        // �����o��������
        spawnDogScript.SpawnDogs();



    }


    private int GetIdFromPlayer(GameObject player)
    {
        PlayerID playerIdComponent = player.GetComponent<PlayerID>();
        return playerIdComponent != null ? playerIdComponent.id : 1; // �f�t�H���g�l��1�ɐݒ�
    }
}
public class PlayerID : MonoBehaviour
{
    [HideInInspector]
    public int id;

    [SerializeField]
    private StatusData statusData;

    private void Start()
    {
        if (statusData != null)
        {
            id = (int)statusData.ActDiff;
        }
        else
        {
            Debug.LogError($"StatusData is not assigned on {gameObject.name}");
            id = 0;  // ƒfƒtƒHƒ‹ƒg’l‚ðÝ’è
        }
    }
}