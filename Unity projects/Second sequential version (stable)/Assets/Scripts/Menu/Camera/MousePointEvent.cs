using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePointEvent : MonoBehaviour {

    public void OnPointerEnter()
    {
        GameObject.Find("Mouse").GetComponent<MouseSkinManager>().Point("button");   //.SetTexture("point", true);
    }

    public void OnPointerExit()
    {
        GameObject.Find("Mouse").GetComponent<MouseSkinManager>().Unpoint("button");    //.UnsetTextureButton();
    }
}
