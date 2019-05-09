using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeSelector : MonoBehaviour {

    public Button Size100Button;
    public Button Size200Button;
    public Button Size400Button;

    private void Awake()
    {
        switch(DataBetweenScenes.getSize())
        {
            case 100: this.SetColor100(); break;
            case 200: this.SetColor200(); break;
            case 400: this.SetColor400(); break;
        }
    }

    public void SetSize100()
    {
        DataBetweenScenes.setSize(100);
        SetColor100();
    }

    public void SetSize200()
    {
        DataBetweenScenes.setSize(200);
        SetColor200();
    }

    public void SetSize400()
    {
        DataBetweenScenes.setSize(400);
        SetColor400();
    }

    private void SetColor100()
    {
        Size100Button.GetComponent<Image>().color = Color.yellow;
        Size200Button.GetComponent<Image>().color = Color.white;
        Size400Button.GetComponent<Image>().color = Color.white;
    }

    private void SetColor200()
    {
        Size100Button.GetComponent<Image>().color = Color.white;
        Size200Button.GetComponent<Image>().color = Color.yellow;
        Size400Button.GetComponent<Image>().color = Color.white;
    }

    private void SetColor400()
    {
        Size100Button.GetComponent<Image>().color = Color.white;
        Size200Button.GetComponent<Image>().color = Color.white;
        Size400Button.GetComponent<Image>().color = Color.yellow;
    }
}
