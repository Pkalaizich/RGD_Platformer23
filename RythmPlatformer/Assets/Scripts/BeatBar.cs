using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class BeatBar : MonoBehaviour
{
    [SerializeField] private float barDistance;
    [SerializeField] private float headWidth;
    [SerializeField] private int currentPos;
    private Image barImage;
    public bool rightSide;
    private int direction;
    private float beatDuration;
    private float thresholdDuration;
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
        //beatDuration = (RythmController.Instance.secPerBeat *(1-RythmController.Instance.THRESHOLD));
        beatDuration = RythmController.Instance.secPerBeat;
        thresholdDuration = RythmController.Instance.THRESHOLD * beatDuration;
        GameplayEvents.OnBeatEnded.AddListener(SetBarAnimation);
        GameplayEvents.OnBadAction.AddListener(ChangeBar);
        barImage.rectTransform.localScale = Vector3.one*((4f / (4f + currentPos*4f)));
    }
    private void SetBarAnimation()
    {
        Sequence movement = DOTween.Sequence();
        currentPos -= 1;
        if(currentPos< 0 )
        {            
            currentPos = 3;
            barImage.rectTransform.localPosition = new Vector2(barImage.rectTransform.localPosition.x - -1f * direction * (4*barDistance), barImage.rectTransform.localPosition.y);
            barImage.rectTransform.localScale = Vector3.zero;
            /*Color aux = barImage.color;
            aux.a = 0.6f;
            barImage.color = aux;*/
            //movement.Join(barImage.DOFade(1f, 0f));
        }
        if(currentPos ==0)
        {
            movement.Append(barImage.rectTransform.DOLocalMoveX(direction * (barDistance * currentPos +headWidth/2), beatDuration-thresholdDuration)).Join(barImage.rectTransform.DOScale(Vector3.one * (4f / (4f + currentPos * 4f)), beatDuration-thresholdDuration))
            .Append((barImage.rectTransform.DOLocalMoveX(0,thresholdDuration*0.95f))).Join(barImage.DOFade(0f, thresholdDuration*0.95f)).Join(barImage.rectTransform.DOScale(Vector3.one *0.5f, thresholdDuration * 0.95f));
        }
        else
        {
            movement.Append(barImage.rectTransform.DOLocalMoveX(direction * barDistance * currentPos, beatDuration)).Join(barImage.rectTransform.DOScale(Vector3.one * (4f / (4f + currentPos * 4f)), beatDuration)).Join(barImage.DOFade(1f, beatDuration));
        }        
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
