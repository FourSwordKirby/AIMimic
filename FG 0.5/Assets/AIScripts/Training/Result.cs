using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// This describes a results that can occur. This is a fairly broad spectrum
/// </summary>
public enum Result
{
    Landed,    //Your attack hit
    Whiffed,   //Your attack whiffed
    Blocked,   //Your attack was blocked
    LockedDown, //You put the opponent in the corner
    Hit,       //You were hit by an attack 
    Block,     //You blocked an attack
    Dodged,    //You dodged the attack
    Cornered,  //You were put into the corner
    Win,       //You won the round
    Lose       //You lost the round
}
