using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class GameInput
{
    public static bool GetKeyDown(KeyCode k)
    {
#if ENABLE_INPUT_SYSTEM
        var mapped = MapKey(k);
        if (mapped == null || Keyboard.current == null) return false;
        return Keyboard.current[mapped.Value].wasPressedThisFrame;
#else
        return Input.GetKeyDown(k);
#endif
    }

    public static bool GetKey(KeyCode k)
    {
#if ENABLE_INPUT_SYSTEM
        var mapped = MapKey(k);
        if (mapped == null || Keyboard.current == null) return false;
        return Keyboard.current[mapped.Value].isPressed;
#else
        return Input.GetKey(k);
#endif
    }

    public static float GetAxis(string axisName)
    {
#if ENABLE_INPUT_SYSTEM
        return axisName switch
        {
            "Horizontal" => AxisFromKeys(Key.A, Key.D, Key.LeftArrow, Key.RightArrow),
            "Vertical"   => AxisFromKeys(Key.S, Key.W, Key.DownArrow, Key.UpArrow),
            "Mouse X"    => Mouse.current != null ? Mouse.current.delta.ReadValue().x * 0.05f : 0f,
            "Mouse Y"    => Mouse.current != null ? Mouse.current.delta.ReadValue().y * 0.05f : 0f,
            _            => 0f
        };
#else
        return Input.GetAxis(axisName);
#endif
    }

    public static Vector2 MouseScrollDelta()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.scroll.ReadValue() * 0.01f : Vector2.zero;
#else
        return Input.mouseScrollDelta;
#endif
    }

    public static Vector2 MousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static float AxisFromKeys(Key neg, Key pos, Key negAlt, Key posAlt)
    {
        if (Keyboard.current == null) return 0f;
        float v = 0f;
        if (Keyboard.current[neg].isPressed    || Keyboard.current[negAlt].isPressed) v -= 1f;
        if (Keyboard.current[pos].isPressed    || Keyboard.current[posAlt].isPressed) v += 1f;
        return Mathf.Clamp(v, -1f, 1f);
    }

    private static Key? MapKey(KeyCode k) => k switch
    {
        KeyCode.Space      => Key.Space,
        KeyCode.Tab        => Key.Tab,
        KeyCode.Return     => Key.Enter,
        KeyCode.Escape     => Key.Escape,
        KeyCode.Alpha1     => Key.Digit1,
        KeyCode.Alpha2     => Key.Digit2,
        KeyCode.Alpha3     => Key.Digit3,
        KeyCode.P          => Key.P,
        KeyCode.Q          => Key.Q,
        KeyCode.E          => Key.E,
        KeyCode.F          => Key.F,
        KeyCode.R          => Key.R,
        KeyCode.W          => Key.W,
        KeyCode.A          => Key.A,
        KeyCode.S          => Key.S,
        KeyCode.D          => Key.D,
        KeyCode.UpArrow    => Key.UpArrow,
        KeyCode.DownArrow  => Key.DownArrow,
        KeyCode.LeftArrow  => Key.LeftArrow,
        KeyCode.RightArrow => Key.RightArrow,
        _                  => null
    };
#endif
}
