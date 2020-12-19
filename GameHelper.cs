using Godot;
using System;

public static class GameHelper {
    public static float delta = 0.01666667f;

    public static float GetDeltaValue(float value, float delta) {
        return (value / GameHelper.delta) * delta;
    }
}
