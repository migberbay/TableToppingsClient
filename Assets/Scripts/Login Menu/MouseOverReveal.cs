using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseOverReveal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject infoText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        infoText.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoText.SetActive(false);
    }
}
