using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    public Transform camLookAt;
    public Transform camLookAtHolder;
    public Transform camFollow;
    Vector3 camFollowResting = new Vector3(0, 0, 1);
    public Cinemachine.CinemachineVirtualCamera playerCam;

    Vector2 mouseInput;
    Vector3 rotationalAxis;

    public float mouseLookSensitivity = 1;

    [SerializeField] PlayerCharacter pc;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        //// if player can input
        //mouseInput.x = Input.GetAxis("Mouse X");
        //mouseInput.y = Input.GetAxis("Mouse Y");

        //if (mouseInput != Vector2.zero)
        //{
        //    // if some input detected
        //    camLookAtHolder.rotation = camLookAtHolder.rotation * Quaternion.AngleAxis(Time.deltaTime * mouseLookSensitivity * mouseInput.x, Vector3.up);
        //    camLookAtHolder.rotation = camLookAtHolder.rotation * Quaternion.AngleAxis(Time.deltaTime * mouseLookSensitivity * mouseInput.y, Vector3.left);
        //}


        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //}
    }
}
