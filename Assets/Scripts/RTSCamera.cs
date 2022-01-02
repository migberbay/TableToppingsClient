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

    public bool areControlsActive = true, allowEdgeControl = true;

    private Transform cameraTransform;

    private Vector3 prevFrameMousePosition;

    private void Start() {
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    void Update(){
        if(!areControlsActive){
            return;
        }
        
        Vector3 pos = transform.position;
        bool moveTop = (Input.mousePosition.y >= Screen.height - panBorderThickness && allowEdgeControl) || Input.GetKey(KeyCode.W);
        bool moveLeft = (Input.mousePosition.x <= panBorderThickness && allowEdgeControl) || Input.GetKey(KeyCode.A);
        bool moveRight = (Input.mousePosition.x >= Screen.width - panBorderThickness && allowEdgeControl) || Input.GetKey(KeyCode.D);
        bool moveDown = (Input.mousePosition.y <= panBorderThickness && allowEdgeControl) || Input.GetKey(KeyCode.S);

        // CAMERA PAN

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;
        if(pos.y < minY){pos.y = minY;}

        // CAMERA ROTATION

        if(Input.GetMouseButton(1)){ // if right click is being pressed.
            // Bottom left = 0,0,0
            // Bottom right = 1920,0,0
            // Top left = 0,1080,0
            // Top right = 1920,1080 

            var prevX = prevFrameMousePosition.x;
            var prevY = prevFrameMousePosition.y;

            var currX = Input.mousePosition.x;
            var currY = Input.mousePosition.y;

            var deltaX = prevX - currX; // if positive => rotating to the right, else rotating to the left
            var deltaY = prevY - currY; // if positive => rotating up, else rotating down.
            
            
            if(Mathf.Abs(deltaX) > Mathf.Abs(deltaY)){
                this.transform.Rotate(new Vector3(0, deltaX/Screen.width * 125, 0), Space.World);
            }else{
                var rotvar = deltaY/Screen.height * 50;
                var newrot = cameraTransform.rotation.eulerAngles.x + rotvar;

                if(newrot < 70f && newrot > 30f){
                    cameraTransform.Rotate(new Vector3(rotvar, 0, 0), Space.Self);
                }
            }

        }

        // CAMERA MOVEMENT

        if(moveTop){
            pos += this.transform.forward * Time.deltaTime*panSpeed;
            // pos.z += panSpeed * Time.deltaTime * (pos.y/10);
        }
        if(moveLeft){
            pos += this.transform.right * Time.deltaTime*-panSpeed;
            // pos.x += -panSpeed * Time.deltaTime * (pos.y/10);
        }
        if(moveRight){
            pos += this.transform.right * Time.deltaTime*panSpeed;
            // pos.x += panSpeed * Time.deltaTime * (pos.y/10);
        }
        if(moveDown){
            pos += this.transform.forward * Time.deltaTime*-panSpeed;
            // pos.z += -panSpeed * Time.deltaTime * (pos.y/10);
        }

        if(mapLimit.bounds.Contains(pos)){
            transform.position = pos;
        }

        prevFrameMousePosition = Input.mousePosition;
    }
}
