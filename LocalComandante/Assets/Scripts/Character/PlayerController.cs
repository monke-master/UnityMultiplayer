using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private StateHolder _stateHolder;

    private void Awake()
    {
        _stateHolder = GetComponent<StateHolder>();
    }


    void Start()
    {
        _stateHolder.health.OnValueChanged += Health_OnChangedValues;
    }

    

    void Update()
    {
        
    }


    private void Health_OnChangedValues(float _, float health)
    {
        Debug.Log(health);
        if (health <= 0)
        {
            _stateHolder.movementState.Value = (State.Dead);
            GetComponent<PlayerInputController>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;
            enabled = false;
            // LevelController.OnDefeat();
        }
    }

}
