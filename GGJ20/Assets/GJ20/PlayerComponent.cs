using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class PlayerComponent : MonoBehaviour {
  // Start is called before the first frame update
  void Start()
  {
    
  }

  // Update is called once per frame
  void Update()
  {
    
  }

  public void onMessage(JToken data) {
    // TODO: Add rigidbody and move that instead, that allows for easier collision handling.
    if (data["action"] != null) {
      transform.position = transform.position + new Vector3(1, 0, 0);
    }
  }

  public void onDisconnect() {

  }
}
