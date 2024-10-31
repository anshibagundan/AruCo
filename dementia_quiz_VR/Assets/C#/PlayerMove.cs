using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5;

    private Rigidbody _rigidbody;
    private InputMove _gameInputs;
    private Vector2 _moveInputValue;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // Action�X�N���v�g�̃C���X�^���X����
        _gameInputs = new InputMove();

        // Action�C�x���g�o�^
        _gameInputs.Player.Move.started += OnMove;
        _gameInputs.Player.Move.performed += OnMove;
        _gameInputs.Player.Move.canceled += OnMove;

        // Input Action���@�\�����邽�߂ɂ́A
        // �L��������K�v������
        _gameInputs.Enable();
    }

    private void OnDestroy()
    {
        // ���g�ŃC���X�^���X������Action�N���X��IDisposable���������Ă���̂ŁA
        // �K��Dispose����K�v������
        _gameInputs?.Dispose();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        // Move�A�N�V�����̓��͎擾
        _moveInputValue = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        // �ړ����͂�����ꍇ
        if (_moveInputValue != Vector2.zero)
        {
            // �ړ������̃x�N�g�����v�Z
            Vector3 moveDirection = new Vector3(
                _moveInputValue.x,
                0,
                _moveInputValue.y
            ).normalized;

            // �ڕW���x���v�Z
            Vector3 targetVelocity = moveDirection * _moveSpeed;

            // �ڕW���x��K�p
            _rigidbody.velocity = targetVelocity;
        }
        else
        {
            // �ړ����͂��Ȃ��ꍇ�A���x��0�ɐݒ�
            _rigidbody.velocity = Vector3.zero;
        }
    }
}