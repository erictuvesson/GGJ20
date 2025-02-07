﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class PlayerComponent : MonoBehaviour {

  private Vector3 lastInputVelocity = Vector3.zero;
  private string nickname;

  public Renderer renderComponent;

  public int DeviceID;
  public Color Color;
  public float Speed = 2.0f;
  public int Points = 0;
  public bool ready = false;

  void Start() {
    renderComponent = GetComponentInChildren<Renderer>();
    renderComponent.material.SetColor("_Color", this.Color);
  }

  void Update() {
    var min = GameLogic.BBMin;
    var max = GameLogic.BBMax;
    
    transform.position = transform.position + (this.lastInputVelocity * 0.025f) * this.Speed;

    // NEVER DO THIS! :D
    if (!(transform.position.x > min.x && transform.position.x < max.x)) {
      var newX = Mathf.Clamp(transform.position.x, min.x, max.x);
      transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
    if (!(transform.position.z > min.z && transform.position.z < max.z)) {
      var newZ = Mathf.Clamp(transform.position.z, min.z, max.z);
      transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
    }
  }

  public string Nickname() {
    if (string.IsNullOrWhiteSpace(this.nickname)) {
      this.nickname = AirConsole.instance.GetNickname(this.DeviceID) ?? "No nickname";
      return this.nickname;
    }
    return this.nickname;
  }

  public void Score() {
    this.Points++;

    var message = new {
      action = "score",
      points = this.Points
    };

    AirConsole.instance.Message(this.DeviceID, message);
  }

  public void onMessage(JToken data) {
    var action = data["action"].ToObject<string>();

    switch (action) {
      case "move":
        var posX = data["x"].ToObject<int>();
        var posY = data["y"].ToObject<int>();
        this.lastInputVelocity = new Vector3(posX, 0, posY);
        break;

      case "ready":
        this.ready = data["ready"].ToObject<bool>();
        break;
    }
  }

  public void onDisconnect() {
    Destroy(gameObject);
  }
}
