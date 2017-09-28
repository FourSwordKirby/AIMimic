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
    Overhead,
    LowAttack,
    AirAttack,
    StandBlock,
    CrouchBlock,
    DashLeft,
    DashRight,
    AirdashLeft,
    AirdashRight,
    DP,
    TechNeutral,
    TechLeft,
    TechRight
}

//The player status merely indicates what kind of state they are in
//Maybe make player status capture the last action that was taken by the player

//These all need evaluation tbh
public enum PlayerStatus
{
    Stand,
    Crouch,
    Air,
    Highblock,
    Lowblock,
    Hit,
    KnockdownHit,
    Tech,
    Attacking, //Left in for consistency for now
    Other, //Left in for consistency for now
    //Include various kinds of attacking
    Moving,
    Dashing,
    AirDashing,
    StandAttack, //When the opponent has the stand hitbox out
    LowAttack, //When the opponent has the low hitbox out
    OverheadAttack, //When the opponent has the overhead hitbox out
    AirAttack, //When the opponent has the AirAttack hitbox out
    DP, //When the opponent has the Dp hitbox out
    Recovery //The recovery period after an attack
}


//These enums are all extrapolated from the data by the AI
public enum Side
{
    Left,
    Right
}

public enum xMovement
{
    Left,
    Neutral,
    Right
}

public enum yMovement
{
    Up,
    Neutral,
    Down
}

public enum xDistance
{
    Adjacent,
    Near,
    Far
}

public enum yDistance
{
    Below,
    Level,
    Above,
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
