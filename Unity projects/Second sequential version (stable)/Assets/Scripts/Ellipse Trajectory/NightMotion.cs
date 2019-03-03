using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightMotion : MonoBehaviour {

    public CubeSphere attractor;
    public Transform orbitingObject;
    private Ellipse orbitPath;

    [Range(0f, 1f)]
    private float orbitProgress = 0.5f;
    private float orbitPeriod;
    private bool orbitActive = true;

    //Use this for initialization
    void Start()
    {
        float axisSize = attractor.gridSize + 100;
        orbitPath = new Ellipse(axisSize, axisSize);
        switch (attractor.gridSize)
        {
            case 100: orbitPeriod = 100; break;
            case 200: orbitPeriod = 130; break;
            case 400: orbitPeriod = 210; break;
        }
        if (orbitingObject == null)
        {
            orbitActive = false;
            return;
        }
        SetOrbitingObjectPosition();
        StartCoroutine(AnimateOrbit());
    }

    void SetOrbitingObjectPosition()
    {
        Vector2 orbitPos = orbitPath.Evaluate(orbitProgress);
        orbitingObject.localPosition = new Vector3(orbitPos.x, orbitPos.y);
    }

    IEnumerator AnimateOrbit()
    {
        if (orbitPeriod < 0.1f)
        {
            orbitPeriod = 0.1f;
        }
        float orbitSpeed = 1f / orbitPeriod;
        while (orbitActive)
        {
            orbitProgress += Time.deltaTime * orbitSpeed;
            orbitProgress %= 1f;
            SetOrbitingObjectPosition();
            yield return null;
        }
    }
}
