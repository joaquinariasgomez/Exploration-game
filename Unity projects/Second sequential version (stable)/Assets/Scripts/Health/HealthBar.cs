using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {

    private Transform bar;
	
	public void Initialize(float value)
    {
        bar = transform.Find("Bar");
        float lifePercentage = value / 100f;
        bar.localScale = new Vector3(lifePercentage, 1f);
    }
}
