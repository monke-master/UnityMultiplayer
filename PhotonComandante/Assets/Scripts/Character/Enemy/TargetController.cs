using UnityEngine;

public class TargetController : MonoBehaviour
{

    [SerializeField] private float _targetDistance = 10f;
    [SerializeField] private LayerMask _targetLayerMask;
    
    private EnemyController _enemyController;
    private StateHolder _stateHolder;
    private float _rayY;

    // private void Awake()
    // {
    //     _enemyController = GetComponent<EnemyController>();
    //     _stateHolder = GetComponent<StateHolder>();
    //     _rayY = transform.localScale.y / 2 + transform.position.y;
    // }
    //
    //
    // void Update()                                                                                                      
    // {
    //     var direction = Vector2.right * _stateHolder.direction;
    //     
    //     RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, _rayY), direction, _targetDistance, _targetLayerMask);
    //     Debug.DrawRay(new Vector2(transform.position.x, _rayY), 
    //         direction * _targetDistance, Color.red);
    //     if (hit.collider != null)
    //     {
    //         // Debug.Log("Goooooooool");
    //         _enemyController.Follow(hit.transform);
    //     }
    // }
}
