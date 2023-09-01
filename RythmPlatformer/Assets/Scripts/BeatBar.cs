using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class BeatBar : MonoBehaviour
{
    [SerializeField] private float barDistance;
    [SerializeField] private int currentPos;
    private Image barImage;
    public bool rightSide;
    private int direction;
    private float beatDuration;
    [SerializeField] private Sprite failedBar;
    private WaitForSeconds wait = new WaitForSeconds(0.4f);
    [SerializeField] private Sprite normalBar;

    //private float alphaTime;
    private void Awake()
    {
        barImage= GetComponent<Image>();
        direction = rightSide ? 1 : -1;
    }
    private void Start()
    {
        beatDuration = (RythmController.Instance.secPerBeat *(1-RythmController.Instance.THRESHOLD));
        //alphaTime = RythmController.Instance.ThresholdDuration();

        //GameplayEvents.OnThresholdEnter.AddListener(SetBarAlpha);
        GameplayEvents.OnBeatEnded.AddListener(SetBarAnimation);
        GameplayEvents.OnBadAction.AddListener(ChangeBar);
    }
    private void SetBarAnimation()
    {
        Sequence movement = DOTween.Sequence();
        currentPos -= 1;
        if(currentPos< 0 )
        {            
            currentPos = 3;
            barImage.rectTransform.localPosition = new Vector2(barImage.rectTransform.localPosition.x - -1f * direction * (4*barDistance), barImage.rectTransform.localPosition.y);
            //movement.Append(barImage.rectTransform.DOLocalMoveX(direction * (4*350f),0f));
            //movement.Join(barImage.DOFade(1f, 0f));
        }                
        movement.Append(barImage.rectTransform.DOLocalMoveX(direction * barDistance * currentPos, beatDuration));
        /*if (currentPos == 0)
        {
            movement.Join(barImage.DOFade(0.2f, beatDuration-0.05f));
        }*/
        movement.SetEase(Ease.Linear);
        movement.Play();
    }

    private void SetBarAlpha()
    {
        
    }

    public void ChangeBar()
    {
        StartCoroutine(SpriteChange());
    }

    private IEnumerator SpriteChange()
    {
        barImage.sprite = failedBar;
        yield return wait;
        barImage.sprite = normalBar;
    }
}
