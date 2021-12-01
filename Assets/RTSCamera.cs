using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCamera : MonoBehaviour
{
    public float panSpeed = 30f;
    public float scrollSpeed = 20f;
    public float panBorderThickness = 20f;
    public BoxCollider mapLimit;
    public float minY = 20;

    void Update(){
        Vector3 pos = transform.position;
        bool moveTop = Input.mousePosition.y >= Screen.height - panBorderThickness;
        bool moveLeft = Input.mousePosition.x <= panBorderThickness;
        bool moveRight = Input.mousePosition.x >= Screen.width - panBorderThickness;
        bool moveDown = Input.mousePosition.y <= panBorderThickness;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;
        if(pos.y < 20){pos.y = 20;}


        if(moveTop){
            pos.z += panSpeed * Time.deltaTime * (pos.y/10);
        }
        if(moveLeft){
            pos.x += -panSpeed * Time.deltaTime * (pos.y/10);
        }
        if(moveRight){
            pos.x += panSpeed * Time.deltaTime * (pos.y/10);
        }
        if(moveDown){
            pos.z += -panSpeed * Time.deltaTime * (pos.y/10);
        }
        


        if(mapLimit.bounds.Contains(pos)){
            transform.position = pos;
        }

    }
}
