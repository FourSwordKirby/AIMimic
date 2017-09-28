using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class will take care of handling taking in events like "Round has ended"
/// and "The attack has been blocked" and then relaying these events to things like
/// the data recorder and the various AI's
/// </summary>
public class EventManager : MonoBehaviour {

    public EventRecorder eventRecorder;

    public static EventManager instance;
    void Awake()
    {
        instance = this;
    }

    //This is called when someone gets hid
    public void RecordHit(Player aggressor, Player victim, Hitbox hitbox, int comboCount = 1)
    {
        //When you get hit, the data recorder should note that you were not successful in completing the last attempted action
        if (eventRecorder != null && eventRecorder.gameObject.activeSelf)
            eventRecorder.InterruptAction(victim.isPlayer1);

        BroadcastMessage("Hit", hitbox, SendMessageOptions.DontRequireReceiver);
    }

    public void RecordBlock(Player aggressor, Player victim, Hitbox hitbox)
    {
        BroadcastMessage("Block", hitbox, SendMessageOptions.DontRequireReceiver);
    }

    public void RecordRecovery(Player victim)
    {
        //Upon exiting hitstun, the recorder should start recording your actions once more (it notes what your wakeup option was)
        if (eventRecorder != null && eventRecorder.gameObject.activeSelf)
            eventRecorder.ResumeRecording(victim.isPlayer1, victim.isCrouching, victim.isBlocking);
    }

    public void RecordRoundWin(Player winner, Player loser, bool timedOut = false)
    {
        print(winner.isPlayer1 + " Win");
    }

    public void RecordTie(bool timedOut = false)
    {
        print("Tie");
    }

    public void RecordGameWin(Player winner, Player loser)
    {
        print(winner.isPlayer1 + " Game Win");
    }

    public void RecordEndAction(Action action, Player player)
    {
        BroadcastMessage("ActionEnded", new KeyValuePair<Action, bool>(action, player.isPlayer1), SendMessageOptions.DontRequireReceiver);
    }

    //This will take in an action and then send out the 
    //appropriate notification for that action
    public void RecordActionPerformed(Action action, Player player)
    {
        if (eventRecorder != null && eventRecorder.gameObject.activeSelf)
            eventRecorder.RecordAction(action, player.isPlayer1);
        BroadcastMessage("ActionPerformed", new KeyValuePair<Action, bool>(action, player.isPlayer1), SendMessageOptions.DontRequireReceiver);
    }
}
