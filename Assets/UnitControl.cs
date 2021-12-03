using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControl : MonoBehaviour
{
    public List<Unit> selectedUnits = new List<Unit>();
    public List<Unit> ownedTokensInScene = new List<Unit>();

    private void Start() {
        //Create materials and assign the textures to them --> assigns these materials to the model
        //Create a humanoid avatar for the model.
        //Add a unit script to them which will assign them the other needed components
        //Assign the correct outline color to the units

        GetOwnedTokensFake();
    }

    void Update(){
        if(Input.GetMouseButtonDown(0)){
            RaycastHit hit;
            Ray ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, 150);
            Debug.DrawRay(Input.mousePosition,Camera.allCameras[0].transform.forward*150);

            Unit clicked = hit.transform.GetComponent<Unit>();
            bool cntrl_press = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightCommand) || Input.GetKey(KeyCode.LeftCommand);
            
            if(cntrl_press){
                if(selectedUnits.Contains(clicked)){
                    UnselectUnit(clicked);
                }else{
                    SelectUnit(clicked);
                }
            }else{
                UnselectAllUnits();
                SelectUnit(clicked);
            }
        }
    }


    //this is a temporary function, replace fot the actual one.
    private void GetOwnedTokensFake(){
        foreach(var u in GameObject.FindObjectsOfType(typeof(Unit))){
            ownedTokensInScene.Add((Unit)u);
            Debug.Log(u.name);
        }
    }

    #region unit_selection
    private void SelectUnit(Unit u){
        ownedTokensInScene.Add(u);
    }

    private void UnselectUnit(Unit u){
        ownedTokensInScene.Remove(u);
    }

    private void UnselectAllUnits(){
        foreach (var u in selectedUnits){
            var outline = u.GetComponent<Outline>();
            if(u.owned){
                outline.OutlineColor = Color.cyan;
            }else{
                outline.OutlineColor = Color.yellow;
            }
            ownedTokensInScene.Remove(u);
        }
    }

    #endregion
}
