using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MenusUI : MonoBehaviour
{
    [Header("Menu Elements")]
    [SerializeField] private Button playBtn;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Image silhouette;

    

    private void Start()
    {
        menuPanel.SetActive(true);
        playBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.StartGame();
            menuPanel.SetActive(false);
            playBtn.onClick.RemoveAllListeners();
        });
        silhouette.DOFade(1f, 0f);
        silhouette.rectTransform.DOScale(Vector3.zero, 1f);
        GameplayEvents.OnGameEnded.AddListener(EndAnimation);
    }

    public void EndAnimation()
    {
        silhouette.rectTransform.DOScale(Vector3.one * 5.2f, 1f);
    }
}
