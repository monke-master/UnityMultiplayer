using System;
using Unity.Netcode;
using UnityEngine;


public class PlayerInputController : NetworkBehaviour
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

    private void Awake()
    {
        _movementController = GetComponent<CharacterMovementController>();
        _stateHolder = GetComponent<StateHolder>();

        SetDefaultKeysIfNeeded();
    }

    void Start()
    {
        Debug.Log($"[PlayerInputController] Spawned on client {NetworkManager.Singleton.LocalClientId}, OwnerClientId={OwnerClientId}, IsOwner={IsOwner}");
        if (!IsOwner) return;
        _stateHolder.movementState.Value = (State.Idle);
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        // if (Input.GetKeyDown(pauseKey))
        // {
        //     onPause?.Invoke();
        // }

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
                Debug.Log("Reload");
                _stateHolder?.onRecharge?.Invoke();
            }
        }

        bool isInShotOrAim = (_stateHolder.movementState.Value == State.Shot || _stateHolder.movementState.Value == State.Aim);
        
        // Debug.Log("isInShorOrAim: " + isInShotOrAim);
        // Debug.Log("Approximately: "  + Mathf.Approximately(axisDirectionRaw, 0f));
        
        

        if (Mathf.Approximately(axisDirectionRaw, 0f) && !isInShotOrAim)
        {
            _stateHolder.movementState.Value = (State.Idle);
        }
        else if (!isInShotOrAim)
        {
            _stateHolder.movementState.Value = (State.Walk);

            bool isRun = Input.GetKey(runKey);
            if (isRun)
            {
                _stateHolder.movementState.Value = (State.Run);
            }

            if (Mathf.Abs(direction) > 0.01f)
            {
                _movementController.HorizontalMovement(direction, (int)Mathf.Round(axisDirectionRaw), isRun);
            }
        }

        if (Input.GetKey(fireKey) && _stateHolder != null && _stateHolder.ammoCount.Value > 0)
        {
            if (_stateHolder?.clipAmmo != null && _stateHolder.clipAmmo.Value <= 0) return;
            
            _stateHolder.movementState.Value = (State.Aim);
            _stateHolder.movementState.Value = (State.Shot);
            return; 
        } else if (_stateHolder.movementState.Value == State.Shot)
        {
            _stateHolder.movementState.Value = (State.Aim);
        }

        if (Input.GetKey(aimKey) && _stateHolder != null)
        {
            _stateHolder.movementState.Value = (State.Aim);
            return;
        }
        else if (_stateHolder.movementState.Value == State.Aim)
        {
            _stateHolder.movementState.Value = (State.Idle);
        }
    }

    void SetDefaultKeysIfNeeded()
    {
        if (!string.IsNullOrEmpty(horizontalAxisName)) return;
        
        if (leftKey != KeyCode.None || rightKey != KeyCode.None) return;
        
        leftKey = KeyCode.A;
        rightKey = KeyCode.D;
        runKey = KeyCode.LeftShift;
        reloadKey = KeyCode.Q;
        pauseKey = KeyCode.Escape;
        fireKey = KeyCode.E;
        aimKey = KeyCode.R;
    }
}
