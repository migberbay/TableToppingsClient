using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomInputField : TMP_InputField
{
    private void Start() {
        this.lineType = LineType.MultiLineNewline;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        Debug.Log ("Overrides InputField.OnSelect");
        //base.OnSelect(eventData);
        //ActivateInputField();
    }
 
    public override void OnDeselect(BaseEventData eventData)
    {
        Debug.Log ("Overrides InputField.Deselect");
        //DeactivateInputField();
        //base.OnDeselect(eventData);
    }
}
