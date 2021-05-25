using Godot;
using System;

public class Keylock : InteractiveObject {

    [Export]
    public String doorName = "";
    private OfficeDoorSlide doorSlide;

    public override void _Ready() {
        doorSlide = this.GetParent().GetNodeOrNull<OfficeDoorSlide>(doorName);
    }

    public override void UseObject() {
        if (doorSlide is null) {
            GD.PrintErr("Door not set.");
        } else {
            doorSlide.isUnlocked = !doorSlide.isUnlocked;
            if (!doorSlide.isUnlocked && doorSlide.isOpened) {
                doorSlide.Slide();
            }
            doorSlide.ChangeMaterial();
            if (doorSlide.isUnlocked) {
                GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D2").Play();
            } else {
                GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D").Play();
            }
        }
    }
}
