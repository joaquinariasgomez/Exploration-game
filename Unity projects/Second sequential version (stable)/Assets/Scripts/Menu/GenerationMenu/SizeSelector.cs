using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeSelector : MonoBehaviour {

    public Button Size100Button;
    public Button Size200Button;
    public Button Size400Button;

    public void SetSize100()
    {
        DataBetweenScenes.setSize(100);
        Size100Button.GetComponent<Image>().color = Color.yellow;
        Size200Button.GetComponent<Image>().color = Color.white;
        Size400Button.GetComponent<Image>().color = Color.white;
    }

    public void SetSize200()
    {
        DataBetweenScenes.setSize(200);
        Size100Button.GetComponent<Image>().color = Color.white;
        Size200Button.GetComponent<Image>().color = Color.yellow;
        Size400Button.GetComponent<Image>().color = Color.white;
    }

    public void SetSize400()
    {
        DataBetweenScenes.setSize(400);
        Size100Button.GetComponent<Image>().color = Color.white;
        Size200Button.GetComponent<Image>().color = Color.white;
        Size400Button.GetComponent<Image>().color = Color.yellow;
    }
}
