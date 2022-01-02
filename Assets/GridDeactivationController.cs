using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDeactivationController : MonoBehaviour
{
    public GameObject grid;
    public bool gridtoggle = false;

    void Update()
    {   
        if(gridtoggle){
            grid.SetActive(true);
        }else{
            grid.SetActive(false);
        }

        if(Input.GetKeyDown(KeyCode.G)){
            gridtoggle = gridtoggle ? false : true;
        }
    }
}
