using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TestUI : MonoBehaviour
{
    [SerializeField] private RectTransform dotContainer;
    [SerializeField] private Image indicator;
    [SerializeField] private RectTransform energyBar;
    [SerializeField] private TextMeshProUGUI testMessage;

    [SerializeField] private Image energyIndicator;

    [SerializeField] private Image bunnyHead;
    [SerializeField] private Sprite normalHead;
    [SerializeField] private Sprite failedHead;

    [SerializeField] private TextMeshProUGUI countdownTMP;
    private int beat = 0;

    WaitForSeconds wait = new WaitForSeconds(0.4f);
    private float animDuration;
    #region Singleton
    private static TestUI _instance;
    public static TestUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<TestUI>();
                //DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    #endregion

    private void Start()
    {
        animDuration = RythmController.Instance.ThresholdDuration();
        //animDuration = 0.2f;
        GameplayEvents.OnThresholdEnter.AddListener(RabbitHeadScale);
        GameplayEvents.OnThresholdEnter.AddListener(CountDownBeat);
        GameplayEvents.OnBadAction.AddListener(ChangeFace);        
    }
    public void UpdateDotPosition(float percentage)
    {
        //dotContainer.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 360, percentage));
    }
    
    public void SetIndicatorValid()
    {
        Color aux = indicator.color;
        aux.a = 0.5f;        
        indicator.color = aux;
        
    }

    public void SetIndicatorInvalid() 
    {
        Color aux = indicator.color;
        aux.a = 0.25f;        
        indicator.color = aux;
    }
   

    public void SetMessage(string message)
    {
        StartCoroutine(ShowMessage(message));
    }

    private IEnumerator ShowMessage(string message)
    {
        testMessage.gameObject.SetActive(true);
        testMessage.text = message;
        yield return new WaitForSeconds(0.2f);
        testMessage.gameObject.SetActive(false);
    }

    public void UpdateEnergyBar(float percentage)
    {
        energyIndicator.DOFillAmount(percentage, 0.2f);
    }

    public void RabbitHeadScale()
    {
        Sequence scaling = DOTween.Sequence();

        scaling.Append(bunnyHead.rectTransform.DOScale(Vector3.one * 1.1f, animDuration).SetEase(Ease.Linear)).Join(energyBar.DOScale(Vector3.one * 1.1f, animDuration).SetEase(Ease.Linear))
            .Append(bunnyHead.rectTransform.DOScale(Vector3.one, animDuration).SetEase(Ease.Linear)).Join(energyBar.DOScale(Vector3.one, animDuration).SetEase(Ease.Linear));
        scaling.Play();
    }

    public void ChangeFace()
    {
        StartCoroutine(SpriteChange());
    }

    private IEnumerator SpriteChange()
    {
        bunnyHead.sprite = failedHead;
        yield return wait;
        bunnyHead.sprite = normalHead;
    }

    private void CountDownBeat()
    {
        beat += 1;
        if(beat < 4) 
        {
            Sequence countdown = DOTween.Sequence();
            countdownTMP.text = beat.ToString();
            countdown.Append(countdownTMP.rectTransform.DOScale(Vector3.one,animDuration*2)).Join(countdownTMP.DOFade(1,animDuration*2)).
                Append(countdownTMP.rectTransform.DOScale(Vector3.zero, 0.1f)).Join(countdownTMP.DOFade(0, 0.1f));
            countdown.Play();
        }
        else
        {
            Sequence countdown = DOTween.Sequence();
            countdownTMP.text = "¡YA!";
            countdown.Append(countdownTMP.rectTransform.DOScale(Vector3.one, animDuration * 2)).Join(countdownTMP.DOFade(1, animDuration * 2)).
                Append(countdownTMP.rectTransform.DOScale(Vector3.zero, 0.1f)).Join(countdownTMP.DOFade(0, 0.1f));
            countdown.Play();
            GameplayEvents.OnThresholdEnter.RemoveListener(CountDownBeat);
            GameplayEvents.OnCountdownEnded?.Invoke();
        }
    }
}
