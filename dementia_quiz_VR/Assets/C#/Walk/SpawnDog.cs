using UnityEngine;

public class SpawnDog : MonoBehaviour
{
    public GameObject dogPrefab; // インスペクターで設定
    public Vector3 baseSpawnPosition; // インスペクターで設定
    public Vector3 targetPosition; // インスペクターで設定
    public float moveSpeed = 2.0f; // インスペクターで設定
    public int numberOfDogs = 1; // 出現させる犬の数をインスペクターで設定
    public Quaternion rotation;// インスペクターで設定


    public void SpawnDogs()
    {
        for (int i = 0; i < numberOfDogs; i++)
            {
                // ランダムな出現位置を計算 (minSpawnRange 以上 maxSpawnRange 未満)
                float randomXOffset = Random.Range(50.0f, 80.0f) * (Random.value > 0.5f ? 1 : -1);
                float randomZOffset = Random.Range(50.0f, 80.0f) * (Random.value > 0.5f ? 1 : -1);
                Vector3 spawnPosition = new Vector3(
                    baseSpawnPosition.x + randomXOffset,
                    baseSpawnPosition.y,
                    baseSpawnPosition.z + randomZOffset
                );

                // 出現位置で"rotation"度回転させて dog prefab を生成する
                GameObject dog = Instantiate(dogPrefab, spawnPosition, rotation);

                // レンダラーを有効にする
                Renderer[] renderers = dog.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                }

                // スケールを設定する
                dog.transform.localScale = new Vector3(5, 5, 5);

                // Animator コンポーネントを取得し、アニメーターコントローラーを設定する
                Animator animator = dog.GetComponent<Animator>();
                if (animator != null)
                {
                    // ループするようにアニメーションを再生
                    animator.SetBool("isWalking", true);
                }

                // DogMover スクリプトを追加して移動を開始する
                DogMover dogMover = dog.AddComponent<DogMover>();
                dogMover.Initialize(targetPosition, moveSpeed);
        }
    }
}