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

    private readonly UnityEvent gameStarted = new UnityEvent();

    private readonly UnityEvent gameEnded = new UnityEvent();

    private readonly UnityEvent gameWon = new UnityEvent();

    private readonly UnityEvent countdownEnded = new UnityEvent();

    public static UnityEvent OnProcessInputs => instance.processInputsEvent;
    public static UnityEvent OnThresholdEnter => instance.thresholdEnterEvent;
    public static UnityEvent OnBeatEnded => instance.beatEndedEvent;
    public static UnityEvent OnBadAction=> instance.badActionEvent;
    public static UnityEvent OnGameStarted => instance.gameStarted;
    public static UnityEvent OnGameEnded => instance.gameEnded;
    public static UnityEvent OnCountdownEnded => instance.countdownEnded;
    public static UnityEvent OnGameWon => instance.gameWon;


    private void Awake()
    {
        instance = this;
    }
}
