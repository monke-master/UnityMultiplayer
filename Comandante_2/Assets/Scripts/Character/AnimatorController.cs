using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private StateHolder _stateHolder;
    private Animator _animator;
    private String _previousState = "";
    
    private void Awake()
    {
        _stateHolder = GetComponent<StateHolder>();
        _animator = GetComponent<Animator>();
        _stateHolder.movementState.OnChanged += OnMovementStateChanged;
        _stateHolder.shot.OnChanged += OnShot;
        _stateHolder.onRecharge += () =>
        {
            _animator.SetTrigger("Recharge");
        };
        _stateHolder.onAttack += () =>
        {
            _animator.SetTrigger("Attack");
        };
    }

    private void OnMovementStateChanged()
    {
        // Debug.Log(_stateHolder.movementState.Value);
        var state = _stateHolder.movementState.Value; 
        if (_previousState.Length > 0)
        {
            _animator.SetBool(_previousState, false);
        }
        switch (state)
        {
            case State.Walk:
            {
                _animator.SetBool("Walk", true);
                _previousState = "Walk";
                break;
            }
            case State.Run:
            {
                _animator.SetBool("Run", true);
                _previousState = "Run";
                break;
            }
            case State.Dead:
            {
                _animator.SetTrigger("Dead");
                break;
            }
            case State.Aim:
            {
                _animator.SetBool("Aim", true);
                _previousState = "Aim";
                break;
            }
            case State.Shot:
            {
                _animator.SetBool("Shot", true);
                _previousState = "Shot";
                break;
            }
        }
    }

    private void OnShot()
    {
        Debug.Log(_stateHolder.shot.Value);
        if (_stateHolder.shot.Value)
        {
            _animator.SetTrigger("Shot");
        }
    }
}
