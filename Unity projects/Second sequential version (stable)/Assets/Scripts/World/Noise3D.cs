using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise3D
{

    public static float Perlin3D(float x, float y, float z)
    {
        float XY = Mathf.PerlinNoise(x, y);
        float YZ = Mathf.PerlinNoise(y, z);
        float XZ = Mathf.PerlinNoise(x, z);

        float YX = Mathf.PerlinNoise(y, x);
        float ZY = Mathf.PerlinNoise(z, y);
        float ZX = Mathf.PerlinNoise(z, x);

        float xyz = (XY + YZ + XZ + YX + ZY + ZX) / 6;
        return xyz;
    }
}
