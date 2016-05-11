using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stage : MonoBehaviour {

    public string name;

    public StageBoundary stageBoundary;
    public BoxCollider2D cameraBoundary;

    public List<GameObject> spawnPoints;
    public List<MeterOrb> meterOrbs;

}
