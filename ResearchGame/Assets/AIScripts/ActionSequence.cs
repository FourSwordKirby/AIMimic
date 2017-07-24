using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This will be used to denote a string of actions that occurs commonly.For example executing a combo etc.
/// We hope to be able to derive action sequences from the data we've collected
/// </summary>
public class ActionSequence
{
    //A tuple of the action and the frame that action was executed.
    private List<KeyValuePair<Action, int>> actionSequence;
    private int actionIndex;

    public ActionSequence()
    {
        actionSequence = new List<KeyValuePair<Action, int>>() { new KeyValuePair<Action, int>(Action.Stand, 0),
                                                                    new KeyValuePair<Action, int>(Action.JumpNeutral, 30),};
    }

    public Action GetAction(int currentFrame)
    {
        if(actionIndex+1 >= actionSequence.Count)
        {
            Debug.Log("Sequence Completed");
            return Action.Stand;
        }

        if(currentFrame >= actionSequence[actionIndex+1].Value)
            actionIndex++;
        return actionSequence[actionIndex].Key;
    }
}
