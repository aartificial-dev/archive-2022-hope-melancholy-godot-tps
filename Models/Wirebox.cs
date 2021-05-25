using Godot;
using System;

public class Wirebox : InteractiveObject {

    public override void UseObject() {
        GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D").Play();
        GD.PrintErr("Use not set.");
    }
}
