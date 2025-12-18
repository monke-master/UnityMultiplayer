using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    private StateHolder _stateHolder;
    private bool _deadHandled = false;

    private void Awake()
    {
        _stateHolder = GetComponent<StateHolder>();
    }

    private void Update()
    {
        if (_deadHandled || _stateHolder == null) return;

        if (_stateHolder.health <= 0)
        {
            _deadHandled = true;
            _stateHolder.movementState = State.Dead;
            var input = GetComponent<PlayerInputController>();
            if (input != null) input.enabled = false;
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
            enabled = false;
            // LevelController.OnDefeat();
        }
    }
}
