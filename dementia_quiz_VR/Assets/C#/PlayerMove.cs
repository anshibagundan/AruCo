using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 200;

    private Rigidbody _rigidbody;
    private InputMove _gameInputs;
    private Vector2 _moveInputValue;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // RigidbodyのConstraints設定
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        // Actionスクリプトのインスタンス生成
        _gameInputs = new InputMove();

        // Actionイベント登録
        _gameInputs.Player.Move.started += OnMove;
        _gameInputs.Player.Move.performed += OnMove;
        _gameInputs.Player.Move.canceled += OnMove;

        // Input Actionを動作させるために、入力を有効化
        _gameInputs.Enable();
    }

    private void OnDestroy()
    {
        // インスタンス生成したActionクラスはIDisposableを実装しているので、
        // 忘れずDisposeを呼ぶ
        _gameInputs?.Dispose();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        // Moveアクションの値を取得
        _moveInputValue = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        // 移動入力がある場合
        if (_moveInputValue != Vector2.zero)
        {
            // 移動方向のベクトルを計算
            Vector3 moveDirection = new Vector3(
                _moveInputValue.x,
                0,
                _moveInputValue.y
            ).normalized;

            // 目標速度を計算
            Vector3 targetVelocity = moveDirection * _moveSpeed;

            // 既存のY軸速度を保持しつつ、目標速度を設定
            targetVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = targetVelocity;
        }
        else
        {
            // 移動入力がない場合、X軸とZ軸の速度を0に設定し、Y軸の速度を保持
            Vector3 velocity = _rigidbody.velocity;
            velocity.x = 0;
            velocity.z = 0;
            _rigidbody.velocity = velocity;
        }
    }
}