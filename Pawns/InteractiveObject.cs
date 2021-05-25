using Godot;
using System;

public class InteractiveObject : Area {
    [Export]
    public String objName = "Object";
    [Export]
    public Vector2 labelSize = new Vector2(24f, 24f);

    public virtual void UseObject() {
        GD.PrintErr("Use not set.");
    }
}
