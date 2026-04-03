using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class GameInput
{
    public static bool GetKeyDown(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        Key? mappedKey = MapKey(keyCode);
        if (!mappedKey.HasValue || Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current[mappedKey.Value].wasPressedThisFrame;
#else
        return Input.GetKeyDown(keyCode);
#endif
    }

    public static bool GetKey(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        Key? mappedKey = MapKey(keyCode);
        if (!mappedKey.HasValue || Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current[mappedKey.Value].isPressed;
#else
        return Input.GetKey(keyCode);
#endif
    }

    public static float GetAxis(string axisName)
    {
#if ENABLE_INPUT_SYSTEM
        switch (axisName)
        {
            case "Horizontal":
                return AxisFromKeys(Key.A, Key.D, Key.LeftArrow, Key.RightArrow);
            case "Vertical":
                return AxisFromKeys(Key.S, Key.W, Key.DownArrow, Key.UpArrow);
            case "Mouse X":
                return Mouse.current != null ? Mouse.current.delta.ReadValue().x * 0.05f : 0f;
            case "Mouse Y":
                return Mouse.current != null ? Mouse.current.delta.ReadValue().y * 0.05f : 0f;
            default:
                return 0f;
        }
#else
        return Input.GetAxis(axisName);
#endif
    }

    public static Vector2 MouseScrollDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null)
        {
            return Vector2.zero;
        }

        return Mouse.current.scroll.ReadValue() * 0.01f;
#else
        return Input.mouseScrollDelta;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static float AxisFromKeys(Key negativePrimary, Key positivePrimary, Key negativeAlt, Key positiveAlt)
    {
        if (Keyboard.current == null)
        {
            return 0f;
        }

        float value = 0f;
        if (Keyboard.current[negativePrimary].isPressed || Keyboard.current[negativeAlt].isPressed) value -= 1f;
        if (Keyboard.current[positivePrimary].isPressed || Keyboard.current[positiveAlt].isPressed) value += 1f;
        return Mathf.Clamp(value, -1f, 1f);
    }

    private static Key? MapKey(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.Space => Key.Space,
            KeyCode.Tab => Key.Tab,
            KeyCode.Alpha1 => Key.Digit1,
            KeyCode.Alpha2 => Key.Digit2,
            KeyCode.Alpha3 => Key.Digit3,
            KeyCode.P => Key.P,
            KeyCode.Q => Key.Q,
            KeyCode.E => Key.E,
            KeyCode.W => Key.W,
            KeyCode.A => Key.A,
            KeyCode.S => Key.S,
            KeyCode.D => Key.D,
            KeyCode.UpArrow => Key.UpArrow,
            KeyCode.DownArrow => Key.DownArrow,
            KeyCode.LeftArrow => Key.LeftArrow,
            KeyCode.RightArrow => Key.RightArrow,
            _ => null
        };
    }
#endif
}
