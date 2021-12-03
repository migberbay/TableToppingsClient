using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControl : MonoBehaviour
{
    public List<Unit> selectedUnits = new List<Unit>();
    public List<Unit> ownedTokensInScene = new List<Unit>();
    public GameObject centerPointObject;

    private void Start() {
        //Create materials and assign the textures to them --> assigns these materials to the model
        //Create a humanoid avatar for the model.
        //Add a unit script to them which will assign them the other needed components
        //Assign the correct outline color to the units

        GetOwnedTokens();
    }

    void Update(){
        //Unit raycast search
        if(Input.GetMouseButtonDown(0)){
            RaycastHit hit;
            Ray ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, 150);
            // Debug.DrawRay(Input.mousePosition,Camera.allCameras[0].transform.forward*150);

            if(hit.transform != null){
                Unit clicked = hit.transform.GetComponent<Unit>();
                if(clicked != null){
                    Debug.Log("We hit a unit");
                    bool cntrl_press = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightCommand) || Input.GetKey(KeyCode.LeftCommand);
                    if(cntrl_press){
                            if(selectedUnits.Contains(clicked)){
                                UnselectUnit(clicked);
                                centerPointObject.transform.position = CalculateCenterPoint();
                                if(selectedUnits.Count <= 1){
                                    centerPointObject.SetActive(false);
                                }
                            }else{
                                SelectUnit(clicked);
                                if(selectedUnits.Count > 1){
                                    centerPointObject.SetActive(true);
                                    centerPointObject.transform.position = CalculateCenterPoint();
                                }
                            }
                    }else{
                        UnselectAllUnits();
                        centerPointObject.SetActive(false);
                        SelectUnit(clicked);
                    }
                }else{ // If we hit elsewhere in terrain we try to move there with selected units.
                    Physics.Raycast(ray, out hit, 150, 1 << LayerMask.NameToLayer("Terrain"));
                    if(hit.transform != null){
                        Debug.Log("we hit the terrain at: " + hit.point + " moving units.");
                        if(selectedUnits.Count > 1){
                            var dist = Vector3.Distance(centerPointObject.transform.position, hit.point);
                            var dir = (hit.point - centerPointObject.transform.position).normalized;
                            foreach(var s in selectedUnits){
                                s.MoveTo(s.transform.position + dist*dir);
                                centerPointObject.transform.position = hit.point;
                            }
                        }
                    }else{ 
                        Debug.Log("we hit nothing.");
                    }
                    
                }
            }
        }

    }

    private Vector3 CalculateCenterPoint(){
        Vector3 centerPoint = new Vector3();
        var i = 0;
        foreach (var s in selectedUnits){
            i++;
            centerPoint += s.transform.position;
        }
        centerPoint /= i;
        return centerPoint;
    }


    //this is a temporary function, replace for the actual one when instantiation is done.
    private void GetOwnedTokens(){
        foreach(var u in GameObject.FindObjectsOfType(typeof(Unit))){
            ownedTokensInScene.Add((Unit)u);
            Debug.Log(u.name);
        }
    }

    #region unit_selection
    private void SelectUnit(Unit u){
        if(u.owned){
            Debug.Log("Selecting unit: " +  u.gameObject.name);
            selectedUnits.Add(u);
            u.GetComponent<Outline>().OutlineColor = Color.green;
        }else{
            Debug.Log("You dont own that!");
        }
    }

    private void UnselectUnit(Unit u){
        Debug.Log("Unselecting unit: " +  u.gameObject.name);
        if(u.owned){
            u.GetComponent<Outline>().OutlineColor = Color.cyan;
        }else{
            u.GetComponent<Outline>().OutlineColor = Color.yellow;
        }
        selectedUnits.Remove(u);
    }

    private void UnselectAllUnits(){
        centerPointObject.SetActive(true);
        foreach (var u in selectedUnits){
            var outline = u.GetComponent<Outline>();
            if(u.owned){
                outline.OutlineColor = Color.cyan;
            }else{
                outline.OutlineColor = Color.yellow;
            }
        }
        selectedUnits.Clear();
    }

    #endregion
}
