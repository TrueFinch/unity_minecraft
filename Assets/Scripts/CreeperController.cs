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
                    //Debug.Log("Start blinking");
                    effect.StartBlinking(() =>
                    {
                        isblinking = false;
                        var distanceToPlayer = Vector3.Distance(target.transform.position, transform.position);
                        if (distanceToPlayer <= destroyDistance)
                        {
                            Destroy(gameObject);
                            world.DestroySphere(Vector3Int.FloorToInt(transform.position), destroyDistance);
                        }
                        //Debug.Log("End blinking");
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

    private void StartBlinking()
    {
        throw new NotImplementedException();
    }

    private bool CheckNeedJump()
    {
        Vector3 creeperPos = transform.position;
        creeperPos.x = Mathf.Floor(creeperPos.x);
        creeperPos.y = Mathf.Floor(creeperPos.y);
        creeperPos.z = Mathf.Floor(creeperPos.z);
        Vector3 targetPos = target.transform.position;
        targetPos.x = Mathf.Floor(targetPos.x);
        targetPos.y = creeperPos.y;
        targetPos.z = Mathf.Floor(targetPos.z);

        RaycastHit hit;

        if (Physics.Raycast(creeperPos, targetPos, out hit, 1F))
        {
            return hit.transform.tag != "Creeper";
        }
        return false;
    }
}
