using System;
using Photon.Pun;
using UnityEngine;

public class AnimatorController : MonoBehaviourPunCallbacks
{
    private StateHolder _stateHolder;
    private Animator _animator;
    private string _previousState = "";
    private bool _deathTriggered = false; // Флаг для предотвращения повторного проигрывания анимации смерти

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _stateHolder = GetComponent<StateHolder>();
    }

    private void OnEnable()
    {
        if (_stateHolder == null) return;
        _stateHolder.onRecharge += () => _animator.SetTrigger("Recharge");
        _stateHolder.onAttack += () => _animator.SetTrigger("Attack");
    }

    private void OnDisable()
    {
        if (_stateHolder == null) return;
        _stateHolder.onRecharge -= () => _animator.SetTrigger("Recharge");
        _stateHolder.onAttack -= () => _animator.SetTrigger("Attack");
    }

    private void Update()
    {
        if (_stateHolder == null) return;
        UpdateMovementState(_stateHolder.movementState);
        if (_stateHolder.shot) _animator.SetTrigger("Shot");
    }

    private void UpdateMovementState(State current)
    {
        if (!string.IsNullOrEmpty(_previousState))
        {
            _animator.SetBool(_previousState, false);
        }

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
                if (_stateHolder.ammoCount > 0)
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
                // Проигрываем анимацию смерти только один раз
                if (!_deathTriggered)
                {
                    _animator.SetTrigger("Dead");
                    _deathTriggered = true;
                }
                _previousState = "";
                break;
        }
    }
}
