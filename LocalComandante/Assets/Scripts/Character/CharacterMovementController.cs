using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterMovementController : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float runBoost = 2f;
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpOffset;
    [SerializeField] private Transform legsColliderTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Rigidbody2D _rigidbody;
    private bool _isGrounded = false;
    private StateHolder _stateHolder;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _stateHolder = GetComponent<StateHolder>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        Vector3 pos = legsColliderTransform.position;
        _isGrounded = Physics2D.OverlapCircle(pos, jumpOffset, groundMask);
    }

    public void HorizontalMovement(float direction, float axisDirection, bool isRun)
    {
        float boost = isRun ? runBoost : 1f;
        if (axisDirection != 0 && axisDirection*_stateHolder.direction < 0)
        {
            
            transform.Rotate(new Vector2(0, -180));
            _stateHolder.direction *= -1;
        }
        _rigidbody.velocity = new Vector2(speedCurve.Evaluate(direction)*speed*boost, _rigidbody.velocity.y);
    }
    
    public void JumpIfCan()
    {
        Debug.Log(_isGrounded);
        if (_isGrounded)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpForce);
        }
    }

}
