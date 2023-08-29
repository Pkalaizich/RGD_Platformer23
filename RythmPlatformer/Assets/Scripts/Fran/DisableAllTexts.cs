using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisableAllTexts : MonoBehaviour
{    
    private void Awake()
    {       
       foreach (TextMeshPro text in GetComponentsInChildren<TextMeshPro>())
       {
            text.enabled = false;
       }
    }
    //Dejo un start vacio para tener la checkbox de desactivar o no el componente.
    private void Start()
    {

    }
}
