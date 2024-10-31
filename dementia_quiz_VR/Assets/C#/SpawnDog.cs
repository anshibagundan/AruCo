using UnityEngine;

public class SpawnDog : MonoBehaviour
{
    public GameObject dogPrefab; // �C���X�y�N�^�[�Őݒ�
    public Vector3 baseSpawnPosition; // �C���X�y�N�^�[�Őݒ�
    public Vector3 targetPosition; // �C���X�y�N�^�[�Őݒ�
    public float moveSpeed = 2.0f; // �C���X�y�N�^�[�Őݒ�
    public int numberOfDogs = 1; // �o�������錢�̐����C���X�y�N�^�[�Őݒ�
    public Quaternion rotation;// �C���X�y�N�^�[�Őݒ�


    public void SpawnDogs()
    {
        for (int i = 0; i < numberOfDogs; i++)
            {
                // �����_���ȏo���ʒu���v�Z (minSpawnRange �ȏ� maxSpawnRange ����)
                float randomXOffset = Random.Range(50.0f, 80.0f) * (Random.value > 0.5f ? 1 : -1);
                float randomZOffset = Random.Range(50.0f, 80.0f) * (Random.value > 0.5f ? 1 : -1);
                Vector3 spawnPosition = new Vector3(
                    baseSpawnPosition.x + randomXOffset,
                    baseSpawnPosition.y,
                    baseSpawnPosition.z + randomZOffset
                );

                // �o���ʒu��"rotation"�x��]������ dog prefab �𐶐�����
                GameObject dog = Instantiate(dogPrefab, spawnPosition, rotation);

                // �����_���[��L���ɂ���
                Renderer[] renderers = dog.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                }

                // �X�P�[����ݒ肷��
                dog.transform.localScale = new Vector3(5, 5, 5);

                // Animator �R���|�[�l���g���擾���A�A�j���[�^�[�R���g���[���[��ݒ肷��
                Animator animator = dog.GetComponent<Animator>();
                if (animator != null)
                {
                    // ���[�v����悤�ɃA�j���[�V�������Đ�
                    animator.SetBool("isWalking", true);
                }

                // DogMover �X�N���v�g��ǉ����Ĉړ����J�n����
                DogMover dogMover = dog.AddComponent<DogMover>();
                dogMover.Initialize(targetPosition, moveSpeed);
        }
    }
}