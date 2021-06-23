using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperController : MonoBehaviour
{
    public float moveSpeed = 6;
    public float jumpSpeed = 8;
    public float gravity = 20;
    public int destroyDistance = 5;

    GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        MoveCreeper();
    }

    private void MoveCreeper()
    {
        bool jumped = false;
        var distanceToPlayer = Vector3.Distance(target.transform.position, transform.position);
        Vector3 moveDir = Vector3.zero;
        if (distanceToPlayer <= destroyDistance)
        {
            //destroy logic
        }
        else
        {
            var character = GetComponent<CharacterController>();
            if (character.isGrounded)
            {
                moveDir = target.transform.position - transform.position;
                moveDir *= moveSpeed;
                moveDir /= distanceToPlayer;
                moveDir.y = 0;

                if (CheckNeedJump())
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
    }

    private bool CheckNeedJump()
    {
        Vector3 creeperPos = transform.position;
        creeperPos.y = Mathf.Floor(creeperPos.y);
        Vector3 targetPos = target.transform.position;
        targetPos.y = creeperPos.y;

        return Physics.Raycast(creeperPos, targetPos, 1);
    }
}
