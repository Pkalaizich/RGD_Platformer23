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
    [SerializeField] private Button creditsButton;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button quitBtn;

    

    private void Start()
    {
        menuPanel.SetActive(true);
        
        playBtn.onClick.AddListener(() =>
        {
            MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.UIConfirm);
            GameManager.Instance.StartGame();
            menuPanel.SetActive(false);
            playBtn.onClick.RemoveAllListeners();
        });
        creditsButton.onClick.AddListener(() =>
        {
            creditsPanel.SetActive(true);
            MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.UIConfirm);
        });
        backBtn.onClick.AddListener(() =>
        {
            creditsPanel.SetActive(false);
            MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.UIConfirm);
        });
        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
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
