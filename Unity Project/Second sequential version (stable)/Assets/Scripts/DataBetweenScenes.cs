using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataBetweenScenes {

    private static float musicVolume;
    private static float soundsVolume;
    private static int size;
    private static int seed;
    private static float c1Astronaut;
    private static float c2Astronaut;
    private static float c1Alien;
    private static float c2Alien;
    private static float maxSpeedAstronaut;
    private static float maxSpeedAlien;
    private static float maxDistanceToShoot;

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

    public static void setC1Astronaut(float data)
    {
        c1Astronaut = data;
    }

    public static void setC2Astronaut(float data)
    {
        c2Astronaut = data;
    }

    public static void setC1Alien(float data)
    {
        c1Alien = data;
    }

    public static void setC2Alien(float data)
    {
        c2Alien = data;
    }

    public static void setMaxSpeedAstronaut(float data)
    {
        maxSpeedAstronaut = data;
    }

    public static void setMaxSpeedAlien(float data)
    {
        maxSpeedAlien = data;
    }

    public static void setMaxDistanceToShoot(float data)
    {
        maxDistanceToShoot = data;
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

    public static float getC1Astronaut()
    {
        return c1Astronaut;
    }

    public static float getC2Astronaut()
    {
        return c2Astronaut;
    }

    public static float getC1Alien()
    {
        return c1Alien;
    }

    public static float getC2Alien()
    {
        return c2Alien;
    }

    public static float getMaxSpeedAstronaut()
    {
        return maxSpeedAstronaut;
    }

    public static float getMaxSpeedAlien()
    {
        return maxSpeedAlien;
    }

    public static float getMaxDistanceToShoot()
    {
        return maxDistanceToShoot;
    }
}
