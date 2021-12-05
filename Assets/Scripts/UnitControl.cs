using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControl : MonoBehaviour
{
    public List<Unit> selectedUnits = new List<Unit>();
    public List<Unit> ownedTokensInScene = new List<Unit>();
    public GameObject centerPointObject;
    public TerrainGridManager tManager;
    public MainSceneMenuController mController;

    float liquidLayerHeight;

    float timeSinceLastClicked = 0f, doubleClickThreshHold = .75f;
    bool doubleClickThisFrame = false;

    public bool areControlsActive = true;

    private void Start() {
        //Create materials and assign the textures to them --> assigns these materials to the model
        //Create a humanoid avatar for the model.
        //Add a unit script to them which will assign them the other needed components
        //Assign the correct outline color to the units

        GetOwnedTokens();
        liquidLayerHeight = tManager.liquidLayerHeightPercent*tManager.terrainMaxHeight;
    }

    void Update(){
        DoubleClickDetection(); // its important this function runs first so we can use the result in the following update functions.

        if(areControlsActive){
            UnitSelection();
            UnitRaiseOrLower();
            if(Input.GetKeyDown(KeyCode.Escape)){
                centerPointObject.SetActive(false);
                UnselectAllUnits();
            }
        }
        
        doubleClickThisFrame = false;
    }

    private void DoubleClickDetection(){
        if(Input.GetMouseButtonDown(0)){
            if(timeSinceLastClicked < doubleClickThreshHold)
                doubleClickThisFrame = true;
            
            timeSinceLastClicked = 0f;
        }else{
            timeSinceLastClicked += Time.deltaTime;
        }
    }

    private void UnitRaiseOrLower(){
        if(Input.GetKeyDown(KeyCode.Space)){
            bool cntrl_press = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightCommand) || Input.GetKey(KeyCode.LeftCommand);
            foreach(var u in selectedUnits){
                if(!cntrl_press){
                    if(u.transform.position.y > liquidLayerHeight){
                        u.TransitionState("flying");
                    }else{
                        u.TransitionState("swimming");
                    }
                    u.RaiseHeight();
                }else{
                    u.TransitionState("floored");
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)){
            foreach(var u in selectedUnits){
                u.LowerHeight();
            }
        }
    }

    private void UnitHit(RaycastHit hit){
        Unit clicked = hit.transform.GetComponent<Unit>();
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
            if(doubleClickThisFrame){
                mController.SpawnNewMenu();
            }
        }
    }

    private void TerrainHit(RaycastHit hit){
        if(selectedUnits.Count > 1){
            var dist = Vector3.Distance(centerPointObject.transform.position, hit.point);
            var dir = (hit.point - centerPointObject.transform.position).normalized;
            foreach(var s in selectedUnits){
                s.MoveTo(s.transform.position + dist*dir);
                centerPointObject.transform.position = hit.point;
            }
        }else if(selectedUnits.Count == 1){
                selectedUnits[0].MoveTo(hit.point);
        }
    }

    private void UnitSelection(){
        if(Input.GetMouseButtonDown(0)){
            RaycastHit hit;
            Ray ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, 150, 1 << LayerMask.NameToLayer("Units"));

            if(hit.transform != null){
                UnitHit(hit);
            }else{ // If we hit somewhere thats not a unity check if it is terrain.
                Physics.Raycast(ray, out hit, 150, 1 << LayerMask.NameToLayer("Terrain"));
                if(hit.transform != null){
                    TerrainHit(hit);
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

    #region UnitSelectionAux
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
        u.GetComponent<Outline>().OutlineColor = Color.cyan;
        selectedUnits.Remove(u);
    }

    private void UnselectAllUnits(){
        centerPointObject.SetActive(true);
        foreach (var u in selectedUnits){
            var outline = u.GetComponent<Outline>().OutlineColor = Color.cyan;
        }
        selectedUnits.Clear();
    }

    #endregion
}
