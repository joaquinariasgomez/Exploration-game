using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowWInSlider : MonoBehaviour {

    Text percentageText;
    public float value;

	void Start () {
        percentageText = GetComponent<Text>();
	}
	
	public void textUpdate(float value)
    {
        this.value = value;
        percentageText.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
