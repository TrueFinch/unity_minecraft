using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScript : MonoBehaviour
{
    public string[] ignoreCollisionWithTags;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
