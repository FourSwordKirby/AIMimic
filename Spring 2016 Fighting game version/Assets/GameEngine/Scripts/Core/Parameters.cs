using UnityEngine;
using System.Collections;

public class Parameters : MonoBehaviour {

    public enum InputDirection
    {
        Left,
        Right
    };

    //Do we need this?
    public enum PlayerStatus
    {
        Default, //Normal everyday state
        Immovable, //Not affected by forces
        Invulnerable, //Doesn't take damage, can be moved around (reduced knockback?)
        Invincible, //No damageno knockback
        Counter //Can initiate a counter attack
    };

    public enum HurtboxStatus
    {
        Superarmor,
        invuln
    }

    public enum Effect
    {
        None,
        Poison        
    }

    public static InputDirection vectorToDirection(Vector2 inputVector)
    {
        if (inputVector.x > 0)
            return InputDirection.Right;
        else
            return InputDirection.Left;

        /*
        if (inputVector == Vector2.zero)
            return Parameters.InputDirection.Stop; ;

        float angle = Mathf.Atan2(inputVector.y, inputVector.x) * Mathf.Rad2Deg;

        if (angle >= -22.5 && angle < 22.5)
        {
            return Parameters.InputDirection.East;
        }
        else if (angle >= 22.5 && angle < 67.5)
        {
            return Parameters.InputDirection.NorthEast;
        }
        else if (angle >= 67.5 && angle < 112.5)
        {
            return Parameters.InputDirection.North;
        }
        else if (angle >= 112.5 && angle < 157.5)
        {
            return Parameters.InputDirection.NorthWest;
        }
        else if (angle >= 157.5 || angle < -157.5)
        {
            return Parameters.InputDirection.West;
        }
        else if (angle >= -157.5 && angle < -112.5)
        {
            return Parameters.InputDirection.SouthWest;
        }
        else if (angle >= -112.5 && angle < -67.5)
        {
            return Parameters.InputDirection.South;
        }
        else if (angle >= -67.5 && angle < -22.5)
        {
            return Parameters.InputDirection.SouthEast;
        }

        return Parameters.InputDirection.Stop
            */
    }

    public static bool isOppositeDirection(InputDirection dir_1, InputDirection dir_2)
    {
        switch (dir_1)
        {
            case InputDirection.Left: 
                return dir_2 == InputDirection.Right;
            case InputDirection.Right:
                return dir_2 == InputDirection.Left;
            }
        return false;
    }

    public static InputDirection getOppositeDirection(InputDirection dir)
    {
        switch (dir)
        {
            case InputDirection.Left:
                return InputDirection.Right;
            case InputDirection.Right:
                return InputDirection.Left;
        }
        return InputDirection.Left;
    }

    public static Vector2 VectorToDir(InputDirection dir)
    {
        switch (dir)
        {
            case Parameters.InputDirection.Left:
                return new Vector2(-1, 0);
            case Parameters.InputDirection.Right:
                return new Vector2(1, 0);
        }
        return Vector2.zero;
    }

    public static InputDirection getTargetDirection(Mobile player, Mobile target)
    {
        Vector2 playerPos = player.transform.position; 
        Vector2 targetPos = target.transform.position;

        if (player.transform.position.x - target.transform.position.x > 0)
        {
            return InputDirection.Right;
        }
        else
            return InputDirection.Left;
    }
}
