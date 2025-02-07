﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class GameLogic : MonoBehaviour {

  public static Vector3 BBMin;
  public static Vector3 BBMax;

  const int MaxConnections = 20;

  private bool connected = false;

  private Dictionary<int, PlayerComponent> players = new Dictionary<int, PlayerComponent>();
  private List<PlaneComponent> planes = new List<PlaneComponent>();
  private float spawnTimer = 0;
  private bool playing = false;
  private float gameTimer = 0;
  private int nextStatsUpdate = 1;

  private float counter = 5;
  private bool counterRun = false;

  public GameObject PlayerPrefab;
  public GameObject PlanePrefab;
  public List<GameObject> SpawnPoints = new List<GameObject>();
  public UnityEngine.UI.Text ConnectText;
  public GameObject ConnectPanel;

  public GameObject ScoreboardPanel;
  public UnityEngine.UI.Text ScoreboardText;

  public GameObject BoundingBoxMin;
  public GameObject BoundingBoxMax;
  
  public List<GameObject> PlaneStartPoints = new List<GameObject>();
  public List<GameObject> PlaneEndPoints = new List<GameObject>();

  void Awake() {
    AirConsole.instance.onReady += onReady;
    AirConsole.instance.onConnect += onConnect;
    AirConsole.instance.onDisconnect += onDisconnect;
    AirConsole.instance.onMessage += onMessage;
  }

  void Start() {
    GameLogic.BBMin = BoundingBoxMin.transform.position;
    GameLogic.BBMax =  BoundingBoxMax.transform.position;
    this.ScoreboardPanel.SetActive(false);
  }

  void StartGame() {
    this.playing = true;
    this.spawnTimer = 0;
    this.gameTimer = 60;
    this.ConnectText.gameObject.SetActive(false);
    this.ConnectPanel.SetActive(false);

    foreach (var player in this.players) {
      player.Value.Points = 0;
    }

    UpdateStats();
  }

  void EndGame() {
    this.playing = false;
    this.ConnectText.gameObject.SetActive(true);
    this.ConnectPanel.SetActive(true);

    foreach (var player in this.players) {
      player.Value.ready = false;
    }
    
    foreach (var plane in this.planes) {
      plane.Saved();
    }
    
    var message = new {
      action = "reset"
    };

    AirConsole.instance.Broadcast(message);

    // Update scoreboard
    this.ScoreboardPanel.SetActive(true);

    var players = this.players.OrderBy(e => e.Value.Points).ToArray();
    var content = "";
    for (var i = 0; i < players.Length; i++) {
      var player = players[i].Value;
      content += "#" + (i + 1) + ". " + player.Nickname() + " (" + player.Points + ")\n";
    }
    this.ScoreboardText.text = content;
  }

  void Update() {
    // Only spawn when playing
    if (this.playing) {
      gameTimer -= Time.deltaTime;

      spawnTimer += Time.deltaTime;
      if (spawnTimer > 3) {
        spawnTimer -= 3;

        var planeStartIndex = Random.Range(0, PlaneStartPoints.Count);
        var planeEndIndex   = Random.Range(0, PlaneEndPoints.Count);

        var planeStart = PlaneStartPoints[planeStartIndex].transform.position;
        var planeEnd   = PlaneEndPoints[planeEndIndex].transform.position;

        var prefab = Instantiate(this.PlanePrefab);
        var plane  = prefab.GetComponent<PlaneComponent>();
        prefab.transform.position = planeStart;
        plane.target = planeEnd;

        planes.Add(plane);
      }

      if (this.gameTimer <= 0) {
        EndGame();
      }
    } else {
      if (counterRun) {
        counter -= Time.deltaTime;
        if (counter <= 0) {
          this.counterRun = false;
          StartGame();
        }
        this.UpdateStats();
      }
    }

    for (var i = 0; i < this.planes.Count;) {
      var plane = this.planes[i];
      if (plane.distanceToTarget() < 3) {
        this.planes.RemoveAt(i);
        Destroy(plane.gameObject);
      } else {

        foreach (var player in this.players) {
          var playerPosition = player.Value.transform.position;

          var distance = Vector3.Distance(playerPosition, plane.transform.position);
          if (distance < 3) {
            player.Value.Score();
            this.planes.RemoveAt(i);
            plane.Saved();
            this.UpdateStats();
            break;
          }
        }

        i++;
      }
    }

    if (connected && Time.time >= nextStatsUpdate) {
      nextStatsUpdate = Mathf.FloorToInt(Time.time) + 1;
      this.UpdateStats();
    }
  }

  void UpdateStats() {
   var total = this.players.Sum(pair => pair.Value.Points);

    var message = new {
      action = "stats",
      state = this.playing ? "playing" : "waiting",
      counter = this.playing ?
        (this.gameTimer <= 10 ? System.Math.Floor(gameTimer) : -1) :
        (this.counterRun ? System.Math.Floor(counter) : -1),
      player_count = this.players.Count,
      total_points = total,
      player_points = this.players.OrderBy(e => e.Value.Points).Select(pair => (name: pair.Value.Nickname(), value: pair.Value.Points)),
      player_ready = this.players.Select(pair => (name: pair.Value.Nickname(), value: pair.Value.ready ? "Yes" : "No"))
    };

    AirConsole.instance.Broadcast(message);
  }

  // Check if all players are ready 
  void ReadyCheck() {
    var allReady = this.players.All(pair => pair.Value.ready);
    if (allReady) {
      this.ScoreboardPanel.SetActive(false);
      this.counter = 3;
      this.counterRun = true;
    }
  }

  void OnDrawGizmos() {
    Gizmos.color = Color.white;
    foreach (var start in PlaneStartPoints) {
      foreach (var end in PlaneEndPoints) {
        Gizmos.DrawLine(start.transform.position, end.transform.position);
      }
    }
    
    Gizmos.color = Color.red;
    var minPos = BoundingBoxMin.transform.position;
    var maxPos = BoundingBoxMax.transform.position;

    Gizmos.DrawLine(new Vector3(minPos.x, 0, minPos.z), new Vector3(maxPos.x, 0, minPos.z));
    Gizmos.DrawLine(new Vector3(minPos.x, 0, minPos.z), new Vector3(minPos.x, 0, maxPos.z));
    Gizmos.DrawLine(new Vector3(maxPos.x, 0, maxPos.z), new Vector3(minPos.x, 0, maxPos.z));
    Gizmos.DrawLine(new Vector3(maxPos.x, 0, maxPos.z), new Vector3(maxPos.x, 0, minPos.z));
  }

  void onReady(string code) {
    this.connected = true;
    this.ConnectText.text = "Connect using code " + code;
  }

  void onConnect(int deviceID) {
    if (players.Count <= MaxConnections && !this.players.ContainsKey(deviceID)) {
      var index = this.players.Count;

      var prefab = Instantiate(this.PlayerPrefab);
      prefab.transform.position = SpawnPoints[index].transform.position;

      var (color, colorName) = GetColor(index);

      this.players[deviceID] = prefab.GetComponent<PlayerComponent>();
      this.players[deviceID].DeviceID = deviceID;
      this.players[deviceID].Color = color;

      var message = new {
        action = "connect",
        status = "success",
        color = ColorUtility.ToHtmlStringRGB(color),
        colorName = colorName,
        nickname = this.players[deviceID].Nickname()
      };

      AirConsole.instance.Message(deviceID, message);
    } else {
      var message = new {
        action = "connect",
        status = "failed"
      };

      AirConsole.instance.Message(deviceID, message);
    }

    UpdateStats();
  }

  void onDisconnect(int deviceID) {
    if (this.players.TryGetValue(deviceID, out var player)) {
      player.onDisconnect();
      this.players.Remove(deviceID);
    }
  }

  void onMessage(int deviceID, JToken data) {
    if (this.players.TryGetValue(deviceID, out var player)) {
      player.onMessage(data);
    }

    var action = data["action"].ToObject<string>();
    if (action == "ready") {
      this.UpdateStats();
      this.ReadyCheck();
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

  (Color, string) GetColor(int index) {
    switch (index) {
    default:
    case 0: return CreateColor("#e6194B", "Red");
    case 1: return CreateColor("#f58231", "Orange");
    case 2: return CreateColor("#ffe119", "Yellow");
    case 3: return CreateColor("#bfef45", "Lime");
    case 4: return CreateColor("#3cb44b", "Green");
    case 5: return CreateColor("#42d4f4", "Cyan");
    case 6: return CreateColor("#4363d8", "Blue");
    case 7: return CreateColor("#911eb4", "Purple");
    case 8: return CreateColor("#f032e6", "Magenta");
    case 9: return CreateColor("#a9a9a9", "Grey");
    case 10: return CreateColor("#000000", "Black");
    case 11: return CreateColor("#e6beff", "Lavender");
    case 12: return CreateColor("#000075", "Navy");
    case 13: return CreateColor("#469990", "Teal");
    case 14: return CreateColor("#aaffc3", "Mint");
    case 15: return CreateColor("#fffac8", "Beige");
    case 16: return CreateColor("#ffd8b1", "Apricot");
    case 17: return CreateColor("#fabebe", "Pink");
    case 18: return CreateColor("#808000", "Olive");
    case 19: return CreateColor("#9A6324", "Brown");
    case 20: return CreateColor("#800000", "Maroon");
    }
  }
  
  private (Color, string) CreateColor(string hex, string name) {
    ColorUtility.TryParseHtmlString(hex, out var color);
    return (color, name);
  }
}
