using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneComponent : MonoBehaviour {

    public Vector3 target;
    public float Speed = 1.0f;

    public float distanceToTarget() {
        return Vector3.Distance(this.transform.position, this.target);
    }

    void Update() {
        this.transform.LookAt(target);   
        this.transform.position = this.transform.position + (this.transform.forward * 0.025f) * this.Speed;
    }
}
