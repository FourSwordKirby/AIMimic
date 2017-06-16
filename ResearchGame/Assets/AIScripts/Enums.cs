using UnityEngine;
using System.Collections;

//Actions are things that a player voluntarily does
public enum Action {
    Stand,
    Crouch,
    WalkLeft,
    WalkRight,
    JumpNeutral,
    JumpLeft,
    JumpRight,
    Attack,
    LowAttack,
    AirAttack,
    StandBlock,
    CrouchBlock
}

//The player status merely indicates what kind of state they are in
public enum PlayerStatus
{
    Stand,
    Crouch,
    Air,
    Highblock,
    Lowblock,
    Hit,
    Tech,
    Attacking,
    Other
}


//These enums are all extrapolated from the data by the AI
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
