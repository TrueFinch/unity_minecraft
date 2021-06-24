using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScript : MonoBehaviour
{
    public string[] ignoreCollisionWithTags;

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var ignored in ignoreCollisionWithTags)
        {
            if (collision.gameObject.tag == ignored)
            {
                Physics.IgnoreCollision(
                    collision.gameObject.GetComponent<Collider>(), 
                    gameObject.GetComponent<Collider>()
                );
            }
        }
    }
}
