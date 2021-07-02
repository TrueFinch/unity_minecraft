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

    bool isblinking = false;
    Vector3 moveDir = Vector3.zero;
    GameObject target;
    WorldGenerator world;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        world = GameObject.FindGameObjectWithTag("World").GetComponent<WorldGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveCreeper();
    }

    private void MoveCreeper()
    {
        var character = GetComponent<CharacterController>();
        bool jumped = false;
        var distanceToPlayer = Vector3.Distance(target.transform.position, transform.position);
        if (distanceToPlayer <= destroyDistance)
        {
            moveDir.x = 0;
            moveDir.z = 0;
            BlinkingEffect effect;
            if (!isblinking)
            {
                isblinking = true;
                if (effect = gameObject.GetComponent<BlinkingEffect>())
                {
                    effect.StartBlinking(() =>
                    {
                        isblinking = false;
                        var distanceToPlayer = Vector3.Distance(target.transform.position, transform.position);
                        if (distanceToPlayer <= destroyDistance)
                        {
                            Destroy(gameObject);
                            world.DestroySphere(Vector3Int.FloorToInt(transform.position), destroyDistance);
                        }
                    });
                }
            }
        }
        else
        {
            if (character.isGrounded)
            {
                moveDir = target.transform.position - transform.position;
                moveDir *= moveSpeed;
                moveDir /= distanceToPlayer;

                if (CheckNeedJump())
                {
                    jumped = true;
                    moveDir.y = jumpSpeed;
                }
            }
        }
        if (!jumped)
        {
            moveDir.y -= gravity * Time.deltaTime;
        }
        character.Move(moveDir * Time.deltaTime);
    }

    private bool CheckNeedJump()
    {
        Vector3 creeperPos = Vector3Int.FloorToInt(transform.position);
        Vector3 targetPos = Vector3Int.FloorToInt(target.transform.position);
        targetPos.y = creeperPos.y;

        RaycastHit hit;

        if (Physics.Raycast(creeperPos, targetPos, out hit, 1F))
        {
            return hit.transform.tag != "Creeper";
        }
        return false;
    }
}
