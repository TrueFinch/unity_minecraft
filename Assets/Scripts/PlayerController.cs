using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6;
    public float jumpSpeed = 8;
    public float gravity = 20;
    public float cameraSensitivity = 4;

    public int destroyDistance = 5;

    Vector3 moveDir = Vector3.zero;

    float yRotation = 0;
    float yRot;
    float xRot;
    public Camera cam;
    WorldGenerator world;
    BlockType selectedBlock = BlockType.GRASS;
    UnityEngine.UI.RawImage grassIcon;
    UnityEngine.UI.RawImage stoneIcon;
    UnityEngine.UI.RawImage waterIcon;
    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.FindGameObjectWithTag("World").GetComponent<WorldGenerator>();
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        grassIcon = GameObject.Find("GrassIcon").GetComponent<UnityEngine.UI.RawImage>();
        stoneIcon = GameObject.Find("StoneIcon").GetComponent<UnityEngine.UI.RawImage>();
        waterIcon = GameObject.Find("WaterIcon").GetComponent<UnityEngine.UI.RawImage>();
        
        SetSelectedBlock(BlockType.GRASS);
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        MouseLook();
        MouseClick();
        HandleKeyboardClick();
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
        yRot = -Input.GetAxis("Mouse Y") * cameraSensitivity;
        xRot = Input.GetAxis("Mouse X") * cameraSensitivity;
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

    private void MouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BlockController bc;
            var ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2F, Screen.height / 2F, 0));
            if (Physics.Raycast(ray, out var hit, destroyDistance))
            {
                if (bc = hit.transform.GetComponent<BlockController>())
                {
                    if (bc.canBeDestroyed)
                    {
                        Vector3Int pos = new Vector3Int(
                            Mathf.FloorToInt(bc.transform.position.x),
                            Mathf.FloorToInt(bc.transform.position.y),
                            Mathf.FloorToInt(bc.transform.position.z)
                        );
                        world.DestroyBlock(pos);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(ray, out var hit, destroyDistance))
            {
                if (hit.transform.GetComponent<BlockController>())
                {
                    var pos = hit.transform.position + hit.normal;
                    Vector3Int posInt = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                    world.CreateBlock(posInt, selectedBlock);
                }
            }
        }
    }

    private void HandleKeyboardClick()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetSelectedBlock(BlockType.GRASS);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetSelectedBlock(BlockType.STONE);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetSelectedBlock(BlockType.WATER);
        }
    }

    private void SetSelectedBlock(BlockType type)
    {
        selectedBlock = type;
        grassIcon.GetComponent<RectTransform>().transform.localScale =
            Vector3.one * (type == BlockType.GRASS ? 0.7F : 0.5F);
        stoneIcon.GetComponent<RectTransform>().transform.localScale =
            Vector3.one * (type == BlockType.STONE ? 0.7F : 0.5F);
        waterIcon.GetComponent<RectTransform>().transform.localScale =
            Vector3.one * (type == BlockType.WATER ? 0.7F : 0.5F);
    }
}
