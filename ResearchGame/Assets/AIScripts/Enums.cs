using UnityEngine;
using System.Collections;

public enum Action {
    Stand,
    Crouch,
    WalkLeft,
    WalkRight,
    JumpNeutral,
    JumpLeft,
    JumpRight,
    Attack,
    AirAttack,
    Block,
    Idle
}

public enum PlayerStatus
{
    Stand,
    Low,
    Air,
    Highblock,
    Lowblock,
    Hit,
    KnockedDown
}

public enum xDistance
{
    Far,
    Near,
    Adjacent
}

public enum yDistance
{
    Far,
    Near,
    Level
}

public enum Health
{
    High,
    Med,
    Low
}

public enum Cornered
{
    yes,
    no
}
