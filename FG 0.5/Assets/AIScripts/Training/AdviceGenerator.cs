using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdviceGenerator : MonoBehaviour {

    public Player advicePlayer;

    public Action lastAction;
    public HashSet<Advice> adviceSet = new HashSet<Advice>();

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            print(adviceSet.Count);
            print("mystery");
        }
    }

    public void Hit(Hitbox hitbox)
    {
        if (hitbox.owner.isPlayer1 != advicePlayer.isPlayer1)
            adviceSet.Add(new Advice(lastAction, Result.Hit));
        else
            adviceSet.Add(new Advice(lastAction, Result.Landed));
        p1Attacked = false;
        p2Attacked = false;
    }

    public void Block(Hitbox hitbox)
    {
        if (hitbox.owner.isPlayer1 != advicePlayer.isPlayer1)
            adviceSet.Add(new Advice(lastAction, Result.Block));
        else
            adviceSet.Add(new Advice(lastAction, Result.Blocked));
        p1Attacked = false;
        p2Attacked = false;
    }


    bool p1Attacked = false;
    bool p2Attacked = false;
    bool p1JustAttacked = false;
    bool p2JustAttacked = false;
    public void ActionPerformed(KeyValuePair<Action, bool> pair)
    {
        Action action = pair.Key;
        bool isPlayer1 = pair.Value;

        if (isPlayer1)
        {
            if (action == Action.AirAttack || action == Action.LowAttack || action == Action.Attack)
            {
                p1Attacked = true;
                p1JustAttacked = true;
            }
            else
                p1JustAttacked = false;
        }
        else
        {
            if (action == Action.AirAttack || action == Action.LowAttack || action == Action.Attack)
            {
                p2Attacked = true;
                p2JustAttacked = true;
            }
            else
                p2JustAttacked = false;
        }

        if (p1Attacked && !p1JustAttacked)
        {
            if (advicePlayer.isPlayer1)
                adviceSet.Add(new Advice(lastAction, Result.Whiffed));
            else
                adviceSet.Add(new Advice(lastAction, Result.Dodged));
            p1Attacked = false;
        }
        if (p2Attacked && !p2JustAttacked)
        {
            if (advicePlayer.isPlayer1)
                adviceSet.Add(new Advice(lastAction, Result.Dodged));
            else
                adviceSet.Add(new Advice(lastAction, Result.Whiffed));
            p2Attacked = false;
        }

        lastAction = action;
    }
}
