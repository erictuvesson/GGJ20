using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class GameLogic : MonoBehaviour {

  const int MaxConnections = 4;
  private Dictionary<int, PlayerComponent> players = new Dictionary<int, PlayerComponent>();

  public GameObject PlayerPrefab;
  public List<GameObject> SpawnPoints = new List<GameObject>();
  public UnityEngine.UI.Text Text;
  
  public List<GameObject> PlaneStartPoints = new List<GameObject>();
  public List<GameObject> PlaneEndPoints = new List<GameObject>();

  void Awake() {
    AirConsole.instance.onReady += onReady;
    AirConsole.instance.onConnect += onConnect;
    AirConsole.instance.onDisconnect += onDisconnect;
    AirConsole.instance.onMessage += onMessage;
  }

  void Update() {
    if (Input.GetKeyDown("space")) {
      // var devices = AirConsole.instance.getControllerDeviceIds();
    }

  }

  void OnDrawGizmos() {
    foreach (var start in PlaneStartPoints) {
      foreach (var end in PlaneEndPoints) {
        Gizmos.DrawLine(start.transform.position, end.transform.position);
      }
    }
  }

  void onReady(string code) {
    this.Text.text = "Connect using code " + code;
  }

  void onConnect(int deviceID) {
    Debug.Log("Connected " + deviceID);
    if (players.Count <= MaxConnections && !this.players.ContainsKey(deviceID)) {
      var index = this.players.Count;

      var prefab = Instantiate(this.PlayerPrefab);
      prefab.transform.position = SpawnPoints[index].transform.position;

      this.players[deviceID] = prefab.GetComponent<PlayerComponent>();

      // Get Color
      var (color, colorName) = PlayerComponent.GetColor(index + 1);
      this.players[deviceID].Color = color;

      var message = new {
        action = "connect",
        status = "success",
        color = ColorUtility.ToHtmlStringRGB(color),
        colorName = colorName
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
