using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneComponent : MonoBehaviour {

    public Vector3 target;
    public float Speed = 1.0f;

    void Update() {
        this.transform.LookAt(target);   
        this.transform.position = this.transform.position + (this.transform.forward * 0.025f) * this.Speed;

        // Killing it based on distance
        var distance = Vector3.Distance(this.transform.position, this.target);
        if (distance < 3) {
            Destroy(gameObject);
        }
    }
}
