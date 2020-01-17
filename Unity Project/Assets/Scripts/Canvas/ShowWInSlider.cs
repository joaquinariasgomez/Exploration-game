using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowWInSlider : MonoBehaviour {

    public GameObject textMeshObject;
    Text percentageText;
    public float value;

	void Start () {
        percentageText = GetComponent<Text>();
	}
	
	public void textUpdate(float value)
    {
        this.value = value;
        //percentageText.text = Mathf.RoundToInt(value * 100) + "%";
        textMeshObject.transform.GetComponent<TMPro.TextMeshProUGUI>().text = Mathf.RoundToInt(value * 100) + " %";
    }
}
