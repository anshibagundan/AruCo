using UnityEngine;

public class DogMover : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private Animator animator;
    private bool isMoving = true;

    // ���������\�b�h
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
            // �ڕW�ʒu�܂ňړ�����
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            // �ڕW�ʒu�ɓ��B������ړ����~����
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                // �ړ����~���A�A�j���[�V�������I������
                isMoving = false;
                DeleteDog(); // HideDog �� DeleteDog �ɕύX
            }
        }
    }

    // �I�u�W�F�N�g���폜���郁�\�b�h
    public void DeleteDog()
    {
        Destroy(gameObject); // ���̃I�u�W�F�N�g���폜
    }
}