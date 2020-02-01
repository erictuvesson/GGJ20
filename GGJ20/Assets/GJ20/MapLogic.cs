using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLogic : MonoBehaviour {
    
    public float Speed = -0.05f;

    void Update() {
        transform.Rotate(0.0f, this.Speed, 0.0f, Space.Self);
    }
}
