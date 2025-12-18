using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private StateHolder _stateHolder;

    private void Awake()
    {
        _stateHolder = GetComponent<StateHolder>();
    }

    public void Damage(float damage)
    {
        _stateHolder.health.SetValue(_stateHolder.health.Value - damage);
    }

    public void Heal(float heal)
    {
        _stateHolder.health.SetValue(Math.Min(_stateHolder.health.Value + heal, _stateHolder.MAX_HEALTH));
    }
}
