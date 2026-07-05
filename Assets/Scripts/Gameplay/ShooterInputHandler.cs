using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterInputHandler
{
    public event Action<Vector2> OnDirectionChanged;

    private readonly Transform origin;

    public ShooterInputHandler(Transform origin)
    {
        this.origin = origin;
    }

    public void Tick()
    {
        if (Touchscreen.current != null)
            HandleTouch();
        else
            HandleMouse();
    }

    private void HandleTouch()
    {
        var touch = Touchscreen.current.primaryTouch;
        if (touch.press.isPressed)
            UpdateDirection(touch.position.ReadValue());
    }

    private void HandleMouse()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed)
            UpdateDirection(Mouse.current.position.ReadValue());
    }

    private void UpdateDirection(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        float camZ = Mathf.Abs(cam.transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camZ));

        Vector2 dir = ((Vector2)worldPos - (Vector2)origin.position).normalized;
        if (dir.y < 0.1f)
            dir = new Vector2(dir.x, 0.1f).normalized;

        OnDirectionChanged?.Invoke(dir);
    }
}
