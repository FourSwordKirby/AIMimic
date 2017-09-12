using UnityEngine;
using System.Collections;

public class EffectsEngine : MonoBehaviour {
    public float p1HeartRate;
    public float p2HeartRate;
    public FloorOrganizer dynamicBackground;

    public Player player1;
    public Player player2;

    public HeartbeatUI p1UI;
    public HeartbeatUI p2UI;

    public AudioSource BGM;
    public AudioLowPassFilter bgmLowPass;
    public AudioReverbFilter bgmReverb;

    private float age;

    void Start()
    {
        
    }



    void Update()
    {
        p1UI.setHeartbeat(p1HeartRate);
        p2UI.setHeartbeat(p2HeartRate);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(dynamicBackground.DropletPattern((int)player2.effectivePosition.x, (int)player2.effectivePosition.y));
            //StartCoroutine(dynamicBackground.DropletPattern(1, 0));
        }
    }
}
