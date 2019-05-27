using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Attack_Defend : MonoBehaviour {

    public GameObject attack;
    public GameObject defend;

    private bool clickedAttack;
    private bool clickedDefend;

    private float noActiveAlphaValue = 0.33f;
    private float activeAlphaValue = 1f;

    private void Start()
    {
        gameObject.SetActive(false);
        clickedAttack = false;
        clickedDefend = false;
        ChangeAttackAlphaTo(noActiveAlphaValue);
        ChangeDefendAlphaTo(noActiveAlphaValue);
    }

    private void ChangeAttackAlphaTo(float value)
    {
        Image image = attack.GetComponent<Image>();
        Color actualColor = image.color;
        actualColor.a = value;
        image.color = actualColor;
    }

    private void ChangeDefendAlphaTo(float value)
    {
        Image image = defend.GetComponent<Image>();
        Color actualColor = image.color;
        actualColor.a = value;
        image.color = actualColor;
    }

    public void OnClickAttack()
    {
        clickedAttack = !clickedAttack;
        clickedDefend = false;

        if(clickedAttack)
        {
            ChangeAttackAlphaTo(activeAlphaValue);
        }
        else
        {
            ChangeAttackAlphaTo(noActiveAlphaValue);
        }
        ChangeDefendAlphaTo(noActiveAlphaValue);
    }

    public void OnClickDefend()
    {
        clickedDefend = !clickedDefend;
        clickedAttack = false;

        if (clickedDefend)
        {
            ChangeDefendAlphaTo(activeAlphaValue);
        }
        else
        {
            ChangeDefendAlphaTo(noActiveAlphaValue);
        }
        ChangeAttackAlphaTo(noActiveAlphaValue);
    }

    public string Clicked()
    {
        if(!clickedAttack && !clickedDefend)
        {
            return "none";
        }
        else
        {
            if(clickedAttack)
            {
                return "clickedAttack";
            }
            else
            {
                return "clickedDefend";
            }
        }
    }
}
