using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volume : MonoBehaviour {

	public void SetMusicVolume(float value)
    {
        DataBetweenScenes.setMusicVolume(value);
    }

    public void SetSoundsVolume(float value)
    {
        DataBetweenScenes.setSoundsVolume(value);
    }
}
