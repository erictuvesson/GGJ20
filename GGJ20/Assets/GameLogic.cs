using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class GameLogic : MonoBehaviour
{
    void Awake() {
        AirConsole.instance.onMessage += onMessage;
    }

    void onMessage(int fromDeviceID, JToken data) {
      Debug.Log("message from " + fromDeviceID + ", data: " + data);
        
    }

    void OnDestroy() {
      if (AirConsole.instance != null) {
        AirConsole.instance.onMessage -= onMessage;
      }
    }
}
