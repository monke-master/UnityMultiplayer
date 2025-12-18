using System;
using UnityEngine;


public class PlayerInputController : MonoBehaviour
{
    public Action onPause;

    [Header("Player identity")]
    [Range(1,4)] public int playerId = 1;

    [Header("Axis (optional)")]
    public string horizontalAxisName = ""; // e.g. "Horizontal_P1"

    [Header("Key bindings (used when axis name is empty)")]
    public KeyCode leftKey = KeyCode.None;
    public KeyCode rightKey = KeyCode.None;
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode aimKey = KeyCode.Mouse1;

    private CharacterMovementController _movementController;
    private StateHolder _stateHolder;
    private StateHolder.ObservableState _movementState;

    private void Awake()
    {
        _movementController = GetComponent<CharacterMovementController>();
        _stateHolder = GetComponent<StateHolder>();
        _movementState = _stateHolder.movementState;

        SetDefaultKeysIfNeeded();
    }

    void Start()
    {
        _movementState.SetValue(State.Idle);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            onPause?.Invoke();
        }

        float direction = 0f;
        float axisDirectionRaw = 0f;

        if (!string.IsNullOrEmpty(horizontalAxisName))
        {
            direction = Input.GetAxis(horizontalAxisName);
            axisDirectionRaw = Input.GetAxisRaw(horizontalAxisName);
        }
        else
        {
            float right = Input.GetKey(rightKey) ? 1f : 0f;
            float left = Input.GetKey(leftKey) ? -1f : 0f;
            direction = right + left;
            axisDirectionRaw = Mathf.Abs(direction) > 0.001f ? Mathf.Sign(direction) : 0f;
        }

        if (Input.GetKeyDown(reloadKey))
        {
            if (_stateHolder?.ammoCount != null && _stateHolder.ammoCount.Value == 0) 
            {
            }
            else
            {
                _stateHolder?.onRecharge?.Invoke();
            }
        }

        bool isInShotOrAim = (_movementState.Value == State.Shot || _movementState.Value == State.Aim);

        if (Mathf.Approximately(axisDirectionRaw, 0f) && !isInShotOrAim)
        {
            _movementState.SetValue(State.Idle);
        }
        else
        {
            _movementState.SetValue(State.Walk);

            bool isRun = Input.GetKey(runKey);
            if (isRun)
            {
                _movementState.SetValue(State.Run);
            }

            if (Mathf.Abs(direction) > 0.01f)
            {
                _movementController.HorizontalMovement(direction, (int)Mathf.Round(axisDirectionRaw), isRun);
            }
        }

        if (Input.GetKey(fireKey))
        {
            if (_stateHolder?.clipAmmo != null && _stateHolder.clipAmmo.Value == 0) return;
            _stateHolder?.movementState.SetValue(State.Aim);
            _stateHolder?.movementState.SetValue(State.Shot);
            return;
        }

        if (Input.GetKey(aimKey))
        {
            _stateHolder?.movementState.SetValue(State.Aim);
            return;
        }
    }

    void SetDefaultKeysIfNeeded()
    {
        if (!string.IsNullOrEmpty(horizontalAxisName)) return;
        
        if (leftKey != KeyCode.None || rightKey != KeyCode.None) return;

        switch (playerId)
        {
            case 1:
                leftKey = KeyCode.A;
                rightKey = KeyCode.D;
                runKey = KeyCode.LeftShift;
                reloadKey = KeyCode.Q;
                pauseKey = KeyCode.Escape;
                fireKey = KeyCode.E;
                aimKey = KeyCode.R;
                break;
            case 2:
                leftKey = KeyCode.LeftArrow;
                rightKey = KeyCode.RightArrow;
                runKey = KeyCode.RightShift;
                reloadKey = KeyCode.P;
                pauseKey = KeyCode.Escape;
                fireKey = KeyCode.O;
                aimKey = KeyCode.I;
                break;
            case 3:
                leftKey = KeyCode.J;
                rightKey = KeyCode.L;
                runKey = KeyCode.RightControl;
                reloadKey = KeyCode.U;
                pauseKey = KeyCode.Escape;
                fireKey = KeyCode.Y;
                aimKey = KeyCode.T;
                break;
            case 4:
                leftKey = KeyCode.F;
                rightKey = KeyCode.H;
                runKey = KeyCode.LeftControl;
                reloadKey = KeyCode.V;
                pauseKey = KeyCode.Escape;
                fireKey = KeyCode.B;
                aimKey = KeyCode.N;
                break;
            default:
                leftKey = KeyCode.A;
                rightKey = KeyCode.D;
                runKey = KeyCode.LeftShift;
                reloadKey = KeyCode.Q;
                pauseKey = KeyCode.Escape;
                fireKey = KeyCode.E;
                aimKey = KeyCode.R;
                break;
        }
    }
}
