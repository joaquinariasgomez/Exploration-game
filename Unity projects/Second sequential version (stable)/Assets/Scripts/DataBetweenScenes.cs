using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataBetweenScenes {

    private static float musicVolume;
    private static float soundsVolume;
    private static int size;
    private static int seed;

    public static void setMusicVolume(float data)
    {
        musicVolume = data;
    }

    public static void setSoundsVolume(float data)
    {
        soundsVolume = data;
    }

    public static void setSize(int data)
    {
        size = data;
    }

    public static void setSeed(int data)
    {
        seed = data;
    }

    public static float getMusicVolume()
    {
        return musicVolume;
    }

    public static float getSoundsVolume()
    {
        return soundsVolume;
    }

    public static int getSize()
    {
        return size;
    }

    public static int getSeed()
    {
        return seed;
    }
}
