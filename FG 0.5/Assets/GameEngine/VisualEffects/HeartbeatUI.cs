using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartbeatUI : MonoBehaviour {
    public Player player;

    public Text hearbeatDisplay;
    public Image heart;

    private float currentHeartbeat;

    // Update is called once per frame
    void Update()
    {
        hearbeatDisplay.text = currentHeartbeat.ToString();
    }

    public void setHeartbeat(float heartbeat)
    {
        currentHeartbeat = heartbeat;
    }
}
