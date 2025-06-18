using System;
using UnityEngine;
using UnityEngine.InputSystem;

// ----- BASED ON STARTER ASSET StarterAssetsInputs.cs -----
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    private Vector2 movement;
    public Vector2 Movement => movement;

    private Vector2 look;
    public Vector2 Look => look;

    private bool jump;
    public bool Jump
    {
        get { return jump; } 
        set { jump = value; }
    }

    private bool sprint;
    // crouching prevents sprinting
    public bool Sprint => sprint && !crouch;

    private bool crouch;
    public bool Crouch => crouch;

    private bool dash;
    public bool Dash => dash;

    public event Action CrouchEvent;
    public event Action UncrouchEvent;
    public event Action DashEvent;


    public void HandleMove(InputAction.CallbackContext ctx)
    {
        movement = ctx.ReadValue<Vector2>();
    }

    public void HandleLook(InputAction.CallbackContext ctx)
    {
        look = ctx.ReadValue<Vector2>();
    }

    public void HandleJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started) jump = true;
        else if (ctx.canceled) jump = false;
    }

    public void HandleDash(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            dash = true;
            DashEvent?.Invoke();
        }
        else if (ctx.canceled)
        {
            dash = false;
        }
    }

    public void HandleCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            crouch = true;
            CrouchEvent?.Invoke();
        }
        else if (ctx.canceled)
        {
            crouch = false;
            UncrouchEvent?.Invoke();
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}