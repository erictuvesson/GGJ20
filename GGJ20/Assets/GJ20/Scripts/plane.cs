using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plane : MonoBehaviour
{
    public float speed = 10.0f;
    private Rigidbody rb;
    public List<Transform> targetList;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        Vector3 direction = targetList[Random.Range(0, 3)].position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }

    // Update is called once per frame
    void Update()
    {
        rb.position += transform.forward * Time.deltaTime * speed;
    }
}
