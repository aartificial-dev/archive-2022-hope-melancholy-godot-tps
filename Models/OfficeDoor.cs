using Godot;
using System;

public class OfficeDoor : InteractiveObject {
    private OfficeDoorSlide doorSlide;

    public override void _Ready() {
        doorSlide = (OfficeDoorSlide) GetParent().GetParent();
    }

    public override void UseObject() {
        if (doorSlide.isUnlocked && !doorSlide.isOpened) {
            doorSlide.Slide();
        } else {
            GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D").Play();
        }
    }
}
