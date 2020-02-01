using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class PlayerComponent : MonoBehaviour {

  public static (Color, string) GetColor(int index) {
    switch (index) {
      default:
      case 1: return (Color.red, "Red");
      case 2: return (Color.green, "Green");
      case 3: return (Color.blue, "Blue");
      case 4: return (Color.yellow, "Yellow");
    }
  }

  private Vector3 lastInputVelocity = Vector3.zero;

  public Color Color;

  void Start() {
    
  }

  void Update() {
    // TODO: Add rigidbody and move that instead, that allows for easier collision handling.
    transform.position = transform.position + this.lastInputVelocity * 0.1f;
  }

  public void onMessage(JToken data) {
    var action = data["action"].ToObject<string>();

    switch (action) {
      case "move":
        var posX = data["x"].ToObject<int>();
        var posY = data["y"].ToObject<int>();
        this.lastInputVelocity = new Vector3(posX, 0, posY);
        break;
    }
  }

  public void onDisconnect() {
    // TODO: Destroy
  }
}
