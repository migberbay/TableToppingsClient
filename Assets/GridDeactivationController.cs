using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDeactivationController : MonoBehaviour
{
    public GameObject grid;
    public bool gridtoggle = false, keyboardActive = true;

    void Update()
    {   
        if(gridtoggle){
            grid.SetActive(true);
        }else{
            grid.SetActive(false);
        }

        if(Input.GetKeyDown(KeyCode.G) && keyboardActive){
            gridtoggle = gridtoggle ? false : true;
        }
    }
}
