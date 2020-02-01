using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class GameLogic : MonoBehaviour {

  const int MaxConnections = 4;

  public GameObject PlayerPrefab;

  // this is the code that is required to connect to the game.
  public string Code;

  private Dictionary<int, PlayerComponent> players = new Dictionary<int, PlayerComponent>();

  void Awake() {
    AirConsole.instance.onReady += onReady;
    AirConsole.instance.onConnect += onConnect;
    AirConsole.instance.onDisconnect += onDisconnect;
    AirConsole.instance.onMessage += onMessage;
  }

  void onReady(string code) {
    this.Code = code;
  }

  void onConnect(int deviceID) {
    Debug.Log("Connected " + deviceID);
    if (players.Count <= MaxConnections && !this.players.ContainsKey(deviceID)) {
      var prefab = Instantiate(this.PlayerPrefab);
      this.players[deviceID] = prefab.GetComponent<PlayerComponent>();

      var message = new {
        action = "connect",
        status = "success",
        color = "#FF0000",
        colorName = "Red"
      };

      AirConsole.instance.Message(deviceID, message);
    Debug.Log("Connected; Success " + deviceID);
    } else {
      var message = new {
        action = "connect",
        status = "failed"
      };

      AirConsole.instance.Message(deviceID, message);
    Debug.Log("Connected; Failed " + deviceID);
    }
  }

  void onDisconnect(int deviceID) {
    if (this.players.TryGetValue(deviceID, out var player)) {
      player.onDisconnect();
      this.players.Remove(deviceID);
    }
  }

  void onMessage(int deviceID, JToken data) {
    Debug.Log("message from " + deviceID + ", data: " + data);
    if (this.players.TryGetValue(deviceID, out var player)) {
      player.onMessage(data);
    }
  }

  void OnDestroy() {
    if (AirConsole.instance != null) {
      AirConsole.instance.onReady -= onReady;
      AirConsole.instance.onConnect -= onConnect;
      AirConsole.instance.onDisconnect -= onDisconnect;
      AirConsole.instance.onMessage -= onMessage;
    }
  }
}
