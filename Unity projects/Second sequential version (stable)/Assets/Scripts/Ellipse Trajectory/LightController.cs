using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {
    Vector3 gravityUp;
    Vector3 bodyUp;

    // Update is called once per frame
    void Update () {
        gravityUp = (transform.position - Vector3.zero).normalized;
        bodyUp = transform.up;

        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 50 * Time.deltaTime);
    }
}
