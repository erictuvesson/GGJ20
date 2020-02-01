using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneComponent : MonoBehaviour {

    public ParticleSystem particleSystem;
    public Vector3 target;
    public float Speed = 1.0f;

    void Update() {
        this.transform.LookAt(target);   
        this.transform.position = this.transform.position + (this.transform.forward * 0.025f) * this.Speed;
    }

    public void Saved() {
        this.Speed = 3.0f;
        particleSystem.Stop();
    }

    public float distanceToTarget() {
        return Vector3.Distance(this.transform.position, this.target);
    }
}
