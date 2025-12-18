using System;
using UnityEngine;

public class AttackController: MonoBehaviour
{
    private GameObject _attack;

    private void Awake()
    {
        _attack = transform.Find("AttackPoint").gameObject;
        _attack.SetActive(false);
    }

    public void OnAttackStart()
    {
        _attack.SetActive(true);
    }
    
    public void OnAttackEnd()
    {
        _attack.SetActive(false);
    }
}