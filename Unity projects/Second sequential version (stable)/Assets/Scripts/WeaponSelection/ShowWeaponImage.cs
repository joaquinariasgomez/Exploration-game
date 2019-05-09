using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowWeaponImage : MonoBehaviour {

    public Sprite Sword;
    public Sprite Shield;

    void Start () {
        gameObject.SetActive(false);
	}
	
	public void SetImage(string from)
    {
        switch(from)
        {
            case "sword": gameObject.GetComponent<Image>().sprite = Sword; break;
            case "shield": gameObject.GetComponent<Image>().sprite = Shield; break;
        }
        gameObject.SetActive(true);
    }
}
