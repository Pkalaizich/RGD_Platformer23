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
        playBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.StartGame();
            menuPanel.SetActive(false);
            playBtn.onClick.RemoveAllListeners();
        });
        silhouette.rectTransform.DOScale(Vector3.zero, 1f);
        GameplayEvents.OnGameEnded.AddListener(EndAnimation);
    }

    private void EndAnimation()
    {
        silhouette.rectTransform.DOScale(Vector3.one * 5.2f, 1f);
    }
}
