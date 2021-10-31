using UnityEngine;

public class FlyCamera : MonoBehaviour {

    private float moveSpeed = 0.05f;
    private float rotateSpeedYaw = 25f;
    private float rotateSpeedPitch = .5f;


    void Update () {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
            transform.position += moveSpeed * new Vector3(-Input.GetAxisRaw("Vertical"), 0, Input.GetAxisRaw("Horizontal"));
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            transform.Rotate(-Input.GetAxis("Mouse ScrollWheel")* rotateSpeedYaw, 0.0f, 0.0f); 
        }

        if(Input.GetKey(KeyCode.Q)){
            transform.Rotate(0.0f, -rotateSpeedPitch, 0.0f);
        }

        if(Input.GetKey(KeyCode.E)){
            transform.Rotate(0.0f, rotateSpeedPitch, 0.0f); 
        }
    }

}