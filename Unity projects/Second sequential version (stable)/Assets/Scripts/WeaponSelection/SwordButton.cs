using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwordButton : MonoBehaviour {

    public GameObject astronaut;
    public GameObject shieldButton;
    private bool isClicked;

    private void Start()
    {
        gameObject.SetActive(false);
        isClicked = false;
    }

    private void ChangeAlphaTo(float value)
    {
        Image image = gameObject.GetComponent<Image>();
        Color actualColor = image.color;
        actualColor.a = value;
        image.color = actualColor;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void NoClicked()
    {
        this.isClicked = false;
        ChangeAlphaTo(0.33f);
    }

    public void OnClick()
    {
        if (isClicked) return;
        isClicked = true;

        astronaut.GetComponent<PlayerController>().SetWeapon("sword");
        shieldButton.GetComponent<ShieldButton>().NoClicked();
        ChangeAlphaTo(1f);
    }
}
