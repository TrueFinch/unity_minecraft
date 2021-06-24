using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingEffect : MonoBehaviour
{
    public int blinkCount = 1;
    public float seconds = 1F;

    Renderer render;
    Color originalColor;
    Color blinkColor = Vector4.zero;
    public void StartBlinking(System.Action callBack)
    {
        StartCoroutine(blinkObj(blinkCount, seconds, callBack));
    }

    private void OnEnable()
    {
        render = gameObject.GetComponent<Renderer>();
        originalColor = render.material.color;
    }

    IEnumerator blinkObj(int blinkCount, float secondsm, System.Action callBack)
    {
        for (var i = 0; i < blinkCount; ++i)
        {
            render.material.color = render.material.color == originalColor
                                    ? blinkColor
                                    : originalColor;

            yield return new WaitForSeconds(seconds / blinkCount);
        }
        if (render.material.color == blinkColor)
        {
            render.material.color = originalColor;
        }
        callBack();
    }
}
