using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataBetweenScenes {

    private static float musicVolume = 0.5f;
    private static float soundsVolume = 0.75f;

    public static void setMusicVolume(float data)
    {
        musicVolume = data;
    }

    public static void setSoundsVolume(float data)
    {
        soundsVolume = data;
    }

    public static float getMusicVolume()
    {
        return musicVolume;
    }

    public static float getSoundsVolume()
    {
        return soundsVolume;
    }
}
