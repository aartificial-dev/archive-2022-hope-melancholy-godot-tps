using Godot;
using System;

public class OfficeDoorSlide : Spatial {
    [Export]
    public bool isUnlocked = false;
    [Export]
    public bool isOpened = false;
    [Export]
    public NodePath doorMarker = new NodePath();

    private float slideAmount = 3.9f;
    private float slideSpeed = 5f;
    private bool isFinished = true;
    private StaticBody doorModel;
    private MeshInstance doorMesh;

    private SpatialMaterial materialNormal = (SpatialMaterial) ResourceLoader.Load("res://Models/Materials/mat_office.tres");
    private SpatialMaterial materialRed = (SpatialMaterial) ResourceLoader.Load("res://Models/Materials/mat_office_red.tres");

    private SpatialMaterial materialPureNormal = (SpatialMaterial) ResourceLoader.Load("res://Materials/MaterialPureWhite.tres");
    private SpatialMaterial materialPureRed = (SpatialMaterial) ResourceLoader.Load("res://Materials/MaterialPureGrayRed.tres");

    private bool canClose = true;

    public override void _Ready() {
        GetNode<Timer>("Timer").Connect("timeout", this, nameof(CloseDoor));
        GetNode<Area>("Area").Connect("body_entered", this, nameof( canCloseFalse ));
        GetNode<Area>("Area").Connect("body_exited", this, nameof( canCloseTrue ));
        doorModel = GetNode<StaticBody>("DoorCollider");
        doorMesh = GetNode<MeshInstance>("DoorCollider/door");
        if (isUnlocked) {
            doorMesh.MaterialOverride = materialNormal;
        } else {
            doorMesh.MaterialOverride = materialRed;
        }
        if (!doorMarker.IsEmpty()) {
            if (isUnlocked) {
                GetNode<CSGMesh>(doorMarker).MaterialOverride = materialPureNormal;
            } else {
                GetNode<CSGMesh>(doorMarker).MaterialOverride = materialPureRed;
            }
        }
    }

    public override void _Process(float delta) {
        if (!isFinished) {
            SlideDoor(delta);
        } 
    }

    private void SlideDoor(float delta) {
        Vector3 _t = doorModel.Translation;
        if (isOpened) {
            _t.x = Mathf.Lerp(doorModel.Translation.x, slideAmount, slideSpeed * delta);
            if (_t.x > slideAmount - 0.05f) {
                _t.x = slideAmount;
                isFinished = true;
            }
        } else {
            _t.x = Mathf.Lerp(doorModel.Translation.x, 0f, slideSpeed * delta);
            if (_t.x < 0.05f) {
                _t.x = 0f;
                isFinished = true;
            }
        }
        doorModel.Translation = _t;
    }

    public void Slide() {
        isOpened = !isOpened;
        isFinished = false;
        if (isOpened) {
            GetNode<Timer>("Timer").Start(5f);
        } else {
            GetNode<Timer>("Timer").Stop();
        }
        GetNode<AudioStreamPlayer3D>("AudioStreamPlayer").Play();
    }

    public void CloseDoor() {
        if (canClose) {
            Slide();
        } else {
            GetNode<Timer>("Timer").Start(1f);
        }
    }

    public void canCloseTrue(Node _body) {
        if (_body is NewPlayer) {
            canClose = true;
        }
    }
    public void canCloseFalse(Node _body) {
        if (_body is NewPlayer) {
            canClose = false;
        }
    }

    public void ChangeMaterial() {
        if (isUnlocked) {
            doorMesh.MaterialOverride = materialNormal;
        } else {
            doorMesh.MaterialOverride = materialRed;
        }
        if (!doorMarker.IsEmpty()) {
            if (isUnlocked) {
                GetNode<CSGMesh>(doorMarker).MaterialOverride = materialPureNormal;
            } else {
                GetNode<CSGMesh>(doorMarker).MaterialOverride = materialPureRed;
            }
        }
    }
}
