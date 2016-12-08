using UnityEngine;
using System.Collections;

public class Parameters : MonoBehaviour {

    public static float positionLeeway = 0.01f;
    
    //TODO: Refactor datarecorder to case on action?
    public enum Action 
    {
        Idle,
        NeutralJump,
        LeftJump,
        RightJump,
        LeftMove,
        RightMove,
        Attack
    }

    public enum InputDirection
    {
        W,
        NW,
        N,
        NE,
        E,
        SE,
        S,
        SW,
        None
    };

    //Do we need this?
    public enum PlayerStatus
    {
        Default, //Normal everyday state
        Invincible //No damageno knockback
    };


    public static InputDirection vectorToDirection(Vector2 inputVector)
    {
        if (inputVector == Vector2.zero)
            return Parameters.InputDirection.None;

        float angle = Mathf.Atan2(inputVector.y, inputVector.x) * Mathf.Rad2Deg;
        
        if (angle >= -22.5 && angle < 22.5)
        {
            return Parameters.InputDirection.E;
        }
        else if (angle >= 22.5 && angle < 67.5)
        {
            return Parameters.InputDirection.NE;
        }
        else if (angle >= 67.5 && angle < 112.5)
        {
            return Parameters.InputDirection.N;
        }
        else if (angle >= 112.5 && angle < 157.5)
        {
            return Parameters.InputDirection.NW;
        }
        else if (angle >= 157.5 || angle < -157.5)
        {
            return Parameters.InputDirection.W;
        }
        else if (angle >= -157.5 && angle < -112.5)
        {
            return Parameters.InputDirection.SW;
        }
        else if (angle >= -112.5 && angle < -67.5)
        {
            return Parameters.InputDirection.S;
        }
        else if (angle >= -67.5 && angle < -22.5)
        {
            return Parameters.InputDirection.SE;
        }
        return Parameters.InputDirection.None;
    }

    public static bool isOppositeDirection(InputDirection dir_1, InputDirection dir_2)
    {
        switch (dir_1)
        {
            case InputDirection.W: 
                return dir_2 == InputDirection.E;
            case InputDirection.E:
                return dir_2 == InputDirection.W;
            }
        return false;
    }

    public static InputDirection getOppositeDirection(InputDirection dir)
    {
        switch (dir)
        {
            case InputDirection.W:
                return InputDirection.E;
            case InputDirection.E:
                return InputDirection.W;
        }
        return InputDirection.W;
    }

    public static Vector2 VectorToDir(InputDirection dir)
    {
        switch (dir)
        {
            case Parameters.InputDirection.W:
                return new Vector2(-1, 0);
            case Parameters.InputDirection.E:
                return new Vector2(1, 0);
        }
        return Vector2.zero;
    }
}
