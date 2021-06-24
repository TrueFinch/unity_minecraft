using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionObject : MonoBehaviour
{
    Renderer render;
    public float displayTime;

    // Update is called once per frame
    void Update()
    {
        if (displayTime > 0)
        {
            displayTime -= Time.deltaTime;
            render.enabled = true;
        }
        else
        {
            render.enabled = false;
        }
    } 

    private void OnEnable()
    {
        render = gameObject.GetComponent<Renderer>();
        displayTime = -1;
    }

    public void HitOcclude(float time)
    {
        displayTime = time;
        render.enabled = true;
    }
}
