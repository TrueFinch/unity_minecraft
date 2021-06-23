using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6;
    public float jumpSpeed = 8;
    public float gravity = 20;
    Vector3 moveDir = Vector3.zero;

    float yRotation = 0;
    float yRot;
    float xRot;
    public float sensitivity = 4;
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        MouseLook();

    }

    void MovePlayer()
    {
        bool jumped = false;
        var character = GetComponent<CharacterController>();
        if (character.isGrounded)
        {
            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDir = transform.TransformDirection(moveDir);
            moveDir *= moveSpeed;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumped = true;
                moveDir.y = jumpSpeed;
            }
        }
        if (!jumped)
        {
            moveDir.y -= gravity * Time.deltaTime;
        }
        character.Move(moveDir * Time.deltaTime);
    }

    void MouseLook()
    {
        yRot = -Input.GetAxis("Mouse Y") * sensitivity;
        xRot = Input.GetAxis("Mouse X") * sensitivity;
        yRotation += yRot;
        yRotation = Mathf.Clamp(yRotation, -80, 80);

        if (xRot != 0)
        {
            transform.eulerAngles += new Vector3(0, xRot, 0);
        }
        if (yRot != 0)
        {
            cam.transform.eulerAngles = new Vector3(yRotation, transform.eulerAngles.y, 0);
        }
    }
}
