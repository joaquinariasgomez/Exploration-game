using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fpsMeasure : MonoBehaviour {
    Rect fpsRect;
    GUIStyle style;

    float fps;

	// Use this for initialization
	void Start () {
        fpsRect = new Rect(100, 100, 400, 100);
        style = new GUIStyle();
        style.fontSize = 30;

        StartCoroutine(RecalculateFPS());
	}
	
    private IEnumerator RecalculateFPS()
    {
        while(true)
        {
            fps = 1 / Time.deltaTime;
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void OnGUI()
    {
        GUI.Label(fpsRect, "FPS: " + string.Format("{0:0.0}", fps), style);
    }
}
