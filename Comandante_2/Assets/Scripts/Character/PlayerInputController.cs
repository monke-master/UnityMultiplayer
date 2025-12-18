using System;
using UnityEngine;


public class PlayerInputController : MonoBehaviour
{
    public Action onPause;
    
    private CharacterMovementController _movementController;
    private StateHolder _stateHolder;
    private StateHolder.ObservableState _movementState;

    private void Awake()
    {
        _movementController = GetComponent<CharacterMovementController>();
        _stateHolder = GetComponent<StateHolder>();
        _movementState = _stateHolder.movementState;
    }

    void Start()
    {
        _movementState.SetValue(State.Idle);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            onPause.Invoke();
        }
        
        var direction = Input.GetAxis("Horizontal");
        var axisDirection = Input.GetAxisRaw("Horizontal");
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_stateHolder.ammoCount.Value == 0) return;
            _stateHolder.onRecharge?.Invoke();
        }
        
        if (Input.GetButtonDown("Jump"))
        {
            _movementController.JumpIfCan();
        }

        if (axisDirection == 0 && _movementState.Value != State.Shot && _movementState.Value != State.Aim)
        { 
            _movementState.SetValue(State.Idle);
        } 
        else
        {
            _movementState.SetValue(State.Walk);

            bool isRun = false;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isRun = true;
                _movementState.SetValue(State.Run);
            }

            if (Mathf.Abs(direction) > 0.01f)
            {
                _movementController.HorizontalMovement(direction, axisDirection, isRun);
            }
        }
        
        if (Input.GetKey(KeyCode.R))
        {
            if (_stateHolder.clipAmmo.Value == 0) return;
            _stateHolder.movementState.SetValue(State.Aim);
            _stateHolder.movementState.SetValue(State.Shot);
            return;
        }
        
        if (Input.GetKey(KeyCode.E))
        {
            _stateHolder.movementState.SetValue(State.Aim);
            return;
        }

        
    }
    
}
