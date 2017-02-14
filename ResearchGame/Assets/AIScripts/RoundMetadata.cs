using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundMetadata : MonoBehaviour {
    public int p1Wins;
    public int p2Wins;

    public RoundMetadata(int p1Wins, int p2Wins)
    {
        this.p1Wins = p1Wins;
        this.p2Wins = p2Wins;
    }
}
