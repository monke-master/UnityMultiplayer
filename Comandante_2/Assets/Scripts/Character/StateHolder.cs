
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityObservables;

public class StateHolder : MonoBehaviour
{
    
    [System.Serializable]
    public class ObservableState: Observable<State> {}

    public ObservableState movementState = new ObservableState() { Value = State.Idle};

    public Observable<bool> shot = new Observable<bool> { Value = false };

    public int direction = 1;

    public int MAX_HEALTH = 100;

    public Observable<float> health = new Observable<float>();
    
    public Action onRecharge;
    
    public Action onRechargeCompleted;
    
    public const int CLIP_CAPACITY = 30;
    
    public Observable<int> clipAmmo = new Observable<int> { Value = CLIP_CAPACITY };
    public Observable<int> ammoCount = new Observable<int> {  };

    public Action onAttack;
}
