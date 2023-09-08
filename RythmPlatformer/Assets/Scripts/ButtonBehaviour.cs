using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviour : MonoBehaviour,ISelectHandler,IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.UIHover);
    }

    public void OnSelect(BaseEventData eventData)
    {
        MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.UIHover);
    }

}
