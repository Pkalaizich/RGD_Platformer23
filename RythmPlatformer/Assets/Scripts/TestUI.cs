using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestUI : MonoBehaviour
{
    [SerializeField] private RectTransform dotContainer;
    [SerializeField] private Image indicator;
    [SerializeField] private TextMeshProUGUI testMessage;

    #region Singleton
    private static TestUI _instance;
    public static TestUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<TestUI>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    #endregion

    public void UpdateDotPosition(float percentage)
    {
        dotContainer.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 360, percentage));
    }
    
    public void SetIndicatorValid()
    {
        Color aux = indicator.color;
        aux.a = 0.5f;
        //indicator.color = Color.green;
        indicator.color = aux;
        
    }

    public void SetIndicatorInvalid() 
    {
        Color aux = indicator.color;
        aux.a = 0.25f;
        //indicator.color = Color.white;
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
}
