using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementController : MonoBehaviour
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
    private int _lastDirection = 1; // Для отслеживания изменения направления

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _stateHolder = GetComponent<StateHolder>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (_stateHolder != null)
        {
            _lastDirection = _stateHolder.direction;
        }
    }
    
    private void Start()
    {
        // Инициализируем правильный поворот спрайта при старте
        if (_stateHolder != null)
        {
            ApplySpriteFlip(_stateHolder.direction < 0);
            _lastDirection = _stateHolder.direction;
        }
    }

    private void FixedUpdate()
    {
        Vector3 pos = legsColliderTransform.position;
        _isGrounded = Physics2D.OverlapCircle(pos, jumpOffset, groundMask);
        
        // Применяем поворот спрайта на основе синхронизированного direction
        UpdateSpriteFlip();
    }
    
    private void UpdateSpriteFlip()
    {
        if (_stateHolder == null) return;
        
        // Если direction изменился, обновляем поворот спрайта
        if (_lastDirection != _stateHolder.direction)
        {
            _lastDirection = _stateHolder.direction;
            ApplySpriteFlip(_stateHolder.direction < 0);
        }
    }
    
    private void ApplySpriteFlip(bool flip)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (flip ? -1 : 1);
        transform.localScale = scale;
        // if (spriteRenderer != null)
        // {
        //     spriteRenderer.flipX = flip;
        // }
        // else
        // {
        //     // Если нет SpriteRenderer, используем localScale
        //     Vector3 scale = transform.localScale;
        //     scale.x = Mathf.Abs(scale.x) * (flip ? -1 : 1);
        //     transform.localScale = scale;
        // }
    }

    public void HorizontalMovement(float direction, float axisDirection, bool isRun)
    {
        float boost = isRun ? runBoost : 1f;
        if (axisDirection != 0 && axisDirection*_stateHolder.direction < 0)
        {
            // Меняем направление - поворот спрайта применится автоматически в FixedUpdate
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
