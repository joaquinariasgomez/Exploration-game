using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedSelector : MonoBehaviour {

    private void Awake()
    {
        int seed = DataBetweenScenes.getSeed();
        this.SetSeedText(seed.ToString());
    }

    private void SetSeedText(string seed)
    {
        gameObject.GetComponent<InputField>().text = seed;
    }

    public void SetSeed(string value)
    {
        DataBetweenScenes.setSeed(int.Parse(value));
    }
}
