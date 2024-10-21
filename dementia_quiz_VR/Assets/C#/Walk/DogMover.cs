using UnityEngine;

public class DogMover : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private Animator animator;
    private bool isMoving = true;

    // 初期化メソッド
    public void Initialize(Vector3 targetPosition, float moveSpeed)
    {
        this.targetPosition = targetPosition;
        this.moveSpeed = moveSpeed;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isMoving)
        {
            // 目標位置まで移動する
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            // 目標位置に到達したら移動を停止する
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                // 移動を停止し、アニメーションを終了する
                isMoving = false;
                DeleteDog(); // HideDog を DeleteDog に変更
            }
        }
    }

    // オブジェクトを削除するメソッド
    public void DeleteDog()
    {
        Destroy(gameObject); // このオブジェクトを削除
    }
}