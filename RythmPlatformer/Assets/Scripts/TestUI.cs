using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    [SerializeField] private RectTransform dotContainer;
    [SerializeField] private Image indicator;

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
}
