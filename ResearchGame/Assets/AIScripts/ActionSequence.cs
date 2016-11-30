using UnityEngine;
using System.Collections.Generic;

//This will be used to denote a string of actions that occurs commonly. For example executing a combo etc.
//We hope to be able to derive action sequences from the data we've collected
public class ActionSequence
{
    //A tuple of the action and the time to wait before attempting the next action.
    public List<KeyValuePair<Action, float>> actionSequence;
}
