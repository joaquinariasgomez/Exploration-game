using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Volume : MonoBehaviour {

    public Slider musicSlider;
    public Slider soundsSlider;

    private void Start()
    {
        musicSlider.value = DataBetweenScenes.getMusicVolume();
        soundsSlider.value = DataBetweenScenes.getSoundsVolume();
    }

    public void SetMusicVolume(float value)
    {
        DataBetweenScenes.setMusicVolume(value);
    }

    public void SetSoundsVolume(float value)
    {
        DataBetweenScenes.setSoundsVolume(value);
    }
}
