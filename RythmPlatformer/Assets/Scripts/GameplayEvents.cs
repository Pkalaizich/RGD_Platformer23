using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayEvents : MonoBehaviour
{
    private static GameplayEvents instance;

    private readonly UnityEvent processInputsEvent = new UnityEvent();

    private readonly UnityEvent thresholdEnterEvent = new UnityEvent();

    private readonly UnityEvent beatEndedEvent = new UnityEvent();

    private readonly UnityEvent badActionEvent= new UnityEvent();

    public static UnityEvent OnProcessInputs => instance.processInputsEvent;
    public static UnityEvent OnThresholdEnter => instance.thresholdEnterEvent;
    public static UnityEvent OnBeatEnded => instance.beatEndedEvent;
    public static UnityEvent OnBadAction=> instance.badActionEvent;


    private void Awake()
    {
        instance = this;
    }
}
