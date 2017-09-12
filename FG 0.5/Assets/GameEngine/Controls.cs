using UnityEngine;
using System.Collections;
using System;

public class Controls {

    /*These constants refer to specific thresholds for reading in inputs
     * For example, the constant FALL_THROUGH_THRESHOLD denotes the threshold 
     * between crouching on a platform and falling through the platform
     */
    public const float FALL_THROUGH_THRESHOLD = 0.5f;

    public static Vector2 getDirection(Player player)
    {
        float xAxis = 0;
        float yAxis = 0;

        if (player == GameManager.instance.p1)
        {
            if (Mathf.Abs(Input.GetAxis("P1 Horizontal")) > Mathf.Abs(Input.GetAxis("P1 Keyboard Horizontal")))
                xAxis = Input.GetAxis("P1 Horizontal");
            else
                xAxis = Input.GetAxis("P1 Keyboard Horizontal");
            if (Mathf.Abs(Input.GetAxis("P1 Vertical")) > Mathf.Abs(Input.GetAxis("P1 Keyboard Vertical")))
                yAxis = Input.GetAxis("P1 Vertical");
            else
                yAxis = Input.GetAxis("P1 Keyboard Vertical");
        }
        else if (player == GameManager.instance.p2)
        {
            if (Mathf.Abs(Input.GetAxis("P2 Horizontal")) > Mathf.Abs(Input.GetAxis("P2 Keyboard Horizontal")))
                xAxis = Input.GetAxis("P2 Horizontal");
            else
                xAxis = Input.GetAxis("P2 Keyboard Horizontal");
            if (Mathf.Abs(Input.GetAxis("P2 Vertical")) > Mathf.Abs(Input.GetAxis("P2 Keyboard Vertical")))
                yAxis = Input.GetAxis("P2 Vertical");
            else
                yAxis = Input.GetAxis("P2 Keyboard Vertical");
        }
        return new Vector2(xAxis, yAxis);
    }

    public static Parameters.InputDirection getInputDirection(Player player)
    {
        return Parameters.vectorToDirection(getDirection(player));
    }

    public static bool jumpInputDown(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButtonDown("P1 Jump");
        else if (player = GameManager.instance.p2)
            return Input.GetButtonDown("P2 Jump");
        else
            return false;
    }

    public static bool attackInputDown(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButtonDown("P1 Attack");
        else if (player == GameManager.instance.p2)
            return Input.GetButtonDown("P2 Attack");
        else
            return false;
    }

    public static bool specialInputDown(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButtonDown("P1 Special");
        else if (player == GameManager.instance.p2)
            return Input.GetButtonDown("P2 Special");
        else
            return false;
    }

    public static bool shieldInputDown(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButtonDown("P1 Shield");
        else if (player == GameManager.instance.p2)
            return Input.GetButtonDown("P2 Shield");
        else
            return false;
    }

    public static bool enhanceInputDown(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButtonDown("P1 Enhance");
        else if (player == GameManager.instance.p2)
            return Input.GetButtonDown("P2 Enhance");
        else
            return false;
    }

    public static bool superInputDown(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButtonDown("P1 Super");
        else if (player == GameManager.instance.p2)
            return Input.GetButtonDown("P2 Super");
        else
            return false;
    }

    public static bool pauseInputDown(Player player)
    {
        return Input.GetButtonDown("Pause");
    }


    public static bool jumpInputHeld(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButton("P1 Jump");
        else if (player == GameManager.instance.p2)
            return Input.GetButton("P2 Jump");
        else
            return false;
    }

    public static bool attackInputHeld(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButton("P1 Attack");
        else if (player == GameManager.instance.p2)
            return Input.GetButton("P2 Attack");
        else
            return false;
    }

    public static bool specialInputHeld(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButton("P1 Special");
        else if (player == GameManager.instance.p2)
            return Input.GetButton("P2 Special");
        else
            return false;
    }

    public static bool shieldInputHeld(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButton("P1 Shield");
        else if (player == GameManager.instance.p2)
            return Input.GetButton("P2 Shield");
        else
            return false;
    }

    public static bool enhanceInputHeld(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButton("P1 Enhance");
        else if (player == GameManager.instance.p2)
            return Input.GetButton("P2 Enhance");
        else
            return false;
    }

    public static bool superInputHeld(Player player)
    {
        if (player == GameManager.instance.p1)
            return Input.GetButton("P1 Super");
        else if (player == GameManager.instance.p2)
            return Input.GetButton("P2 Super");
        else
            return false;
    }

    public static bool pauseInputHeld(Player player)
    {
        return Input.GetButton("Pause");
    }
}
