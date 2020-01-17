using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagOnClick : MonoBehaviour {

    private bool isActive;
    private float noActiveAlphaValue = 0.33f;
    private float activeAlphaValue = 1f;

    // Use this for initialization
    void Start () {
        ChangeAlphaTo(noActiveAlphaValue);
        isActive = false;
    }

    private void ChangeAlphaTo(float value)
    {
        Image image = gameObject.GetComponent<Image>();
        Color actualColor = image.color;
        actualColor.a = value;
        image.color = actualColor;
    }
	
	public void OnClick()
    {
        isActive = !isActive;
        float alpha = activeAlphaValue;
        if(!isActive)
        {
            alpha = noActiveAlphaValue;
        }
        ChangeAlphaTo(alpha);
    }
}
