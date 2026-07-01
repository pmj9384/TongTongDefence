using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterInputHandler : MonoBehaviour
{
    public event Action<Vector2> OnDirectionChanged;

    private const float SwipeThreshold = 10f;

    private Vector2 touchStartPos;
    private bool isSwiping;

    private void Update()
    {
        if (Touchscreen.current != null)
            HandleTouch();
        else
            HandleMouse();
    }

    private void HandleTouch()
    {
        var touch = Touchscreen.current.primaryTouch;
        if (touch.press.wasPressedThisFrame)
        {
            touchStartPos = touch.position.ReadValue();
            isSwiping = true;
        }
        if (isSwiping && touch.press.isPressed)
        {
            Vector2 delta = touch.position.ReadValue() - touchStartPos;
            if (delta.magnitude > SwipeThreshold)
                FireDirection(delta);
        }
        if (touch.press.wasReleasedThisFrame)
            isSwiping = false;
    }

    private void HandleMouse()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchStartPos = Mouse.current.position.ReadValue();
            isSwiping = true;
        }
        if (isSwiping && Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.position.ReadValue() - touchStartPos;
            if (delta.magnitude > SwipeThreshold)
                FireDirection(delta);
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
            isSwiping = false;
    }

    private void FireDirection(Vector2 delta)
    {
        Vector2 dir = delta.normalized;
        if (dir.y < 0.1f)
            dir = new Vector2(dir.x, 0.1f).normalized;
        OnDirectionChanged?.Invoke(dir);
    }
}
