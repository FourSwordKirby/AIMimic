using UnityEngine;
using System.Collections;

public class PlayerSnapshot{
    public bool isBlocking;
    public bool isCrouching;
    public string currentState;

    public PlayerSnapshot(Player player)
    {
        isBlocking = player.isBlocking;
        isCrouching = player.isCrouching;
        currentState = player.ActionFsm.CurrentState.ToString();
    }
}
