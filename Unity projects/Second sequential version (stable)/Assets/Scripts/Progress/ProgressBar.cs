using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour {

    private Transform bar;
    private bool imActive;

	void Start () {
        imActive = false;
        gameObject.SetActive(false);
        bar = transform.Find("Bar");
	}
	
	public void SetProgress(float progressPercentage)
    {
        if(!imActive) {
            imActive = true;
            gameObject.SetActive(true);
        }
        bar.localScale = new Vector3(progressPercentage, 1f);
    }
}
