using System;
using Unity.Netcode;
using UnityEngine;

public class AnimatorController : NetworkBehaviour
{
    private StateHolder _stateHolder;
    private Animator _animator;
    private string _previousState = "";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _stateHolder = GetComponent<StateHolder>();
    }

    public override void OnNetworkSpawn()
    {
        // Подписываемся на изменения сетевых переменных
        _stateHolder.movementState.OnValueChanged += OnMovementStateChanged;
        _stateHolder.shot.OnValueChanged += OnShot;

        // Локальные события (например, перезарядка/атака) остаются локальными
        _stateHolder.onRecharge += () => _animator.SetTrigger("Recharge");
        _stateHolder.onAttack += () => _animator.SetTrigger("Attack");

        // Инициализация анимации текущим состоянием
        // OnMovementStateChanged(State.Idle, _stateHolder.movementState.Value);
        // OnShot(false, _stateHolder.shot.Value);
    }

    private void OnMovementStateChanged(State previous, State current)
    {
        // Сбрасываем предыдущий булев параметр, если был
        if (!string.IsNullOrEmpty(_previousState))
        {
            _animator.SetBool(_previousState, false);
        }
        
        Debug.Log(current);

        // Устанавливаем новый параметр
        switch (current)
        {
            case State.Idle:
                _previousState = "";
                break;
            case State.Walk:
                _animator.SetBool("Walk", true);
                _previousState = "Walk";
                break;
            case State.Run:
                _animator.SetBool("Run", true);
                _previousState = "Run";
                break;
            case State.Aim:
                if (_stateHolder.ammoCount.Value > 0)
                {
                    _animator.SetBool("Aim", true);
                    _previousState = "Aim";
                }
                break;
            case State.Shot:
                _animator.SetBool("Shot", true);
                _previousState = "Shot";
                break;
            case State.Dead:
                _animator.SetTrigger("Dead");
                _previousState = "";
                break;
        }
    }

    private void OnShot(bool previous, bool current)
    {
        if (current)
        {
            _animator.SetTrigger("Shot");
        }
    }
}
