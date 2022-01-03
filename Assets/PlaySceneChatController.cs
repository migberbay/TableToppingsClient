using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlaySceneChatController : MonoBehaviour
{

    public TMP_InputField chatInput;
    public GameObject messagePrefab;
    public GameObject messagesGrid;
    public RTSCamera cam;
    public GridDeactivationController gridc;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisableKeyboardInputs(){
        cam.areControlsActive = false;
        gridc.keyboardActive = false;
    }

    public void ReEnableKeyboardInputs(){
        cam.areControlsActive = true;
        gridc.keyboardActive = true;
    }

    public void PostMessage(string message){
        var messageInstance = Instantiate(messagePrefab);
        messageInstance.GetComponentInChildren<TMP_Text>().text = "player n said: "+ message;
        messageInstance.transform.SetParent(messagesGrid.transform);
        chatInput.text="";
    }

    public void PostDiceRoll(int ammount, string formula){
        var messageInstance = Instantiate(messagePrefab);
        messageInstance.GetComponentInChildren<TMP_Text>().text = "player n rolled: "+ ammount.ToString() + " from formula: "+ formula;
        messageInstance.transform.SetParent(messagesGrid.transform);
    }
}
