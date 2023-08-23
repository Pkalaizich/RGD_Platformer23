using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayEvents : MonoBehaviour
{
    private static GameplayEvents instance;

    private readonly UnityEvent processInputsEvent = new UnityEvent();
    

    public static UnityEvent OnProcessInputs => instance.processInputsEvent;
   

    private void Awake()
    {
        instance = this;
    }
}
