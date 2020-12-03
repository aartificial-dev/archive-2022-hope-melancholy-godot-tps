using Godot;
using System;

public class PlayerCamera : Spatial {

    [Export]
    public string playerPath = "";
    [Export]
    public Vector2 screenSize = Vector2.Zero;
    [Export]
    public Vector2 viewSize = Vector2.Zero;
    [Export]
    public Vector3 borderTopRightOffset = new Vector3(-15, 0, 9);
    [Export]
    public Vector3 borderBottomLeftOffset = new Vector3(13, 0, -12);

    private Player player;
    
    private Vector2 mousePos;
    public Camera camera;
    public Vector2 viewScale = Vector2.Zero;

    private float pivotOffset = 5f;

    public bool isLocked = false;

    private Vector3 borderTopRight = new Vector3(1000, 0, -1000);
    private Vector3 borderBottomLeft = new Vector3(-1000, 0, 1000);
    
    public override void _Ready() {
        player = this.GetParent().GetNodeOrNull<Player>("Player");
        if (!(player is null)) {
            //player.camera = this;
        }
        //player.camera = this;
        mousePos = Vector2.Zero;
        camera = GetNode<Camera>("CameraHolder/Camera");
        viewScale = screenSize / viewSize;
    }

    public override void _Process(float delta) {
        if (!(player is null)) {
            MoveCamera(delta);

            player.mousePos = GetMouseInWorld();
        }
    }

    private void MoveCamera(float delta) {
        if (!isLocked) {
            this.Translation = this.Translation.LinearInterpolate(player.cameraPoint.GlobalTransform.origin, delta * 2.5f);

            Transform _tr = this.GlobalTransform;

            _tr.origin.x = Mathf.Clamp(_tr.origin.x, borderBottomLeft.x, borderTopRight.x);
            _tr.origin.z = Mathf.Clamp(_tr.origin.z, borderTopRight.z, borderBottomLeft.z);
            this.GlobalTransform = _tr;
            /// left max (x negative) = corner + 13
            /// right max (x positive) = corner - 15
            /// top max (z negative) = corner + 9
            /// bottom max (z positive) = corner - 12
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion eventMouseMotion) {
            mousePos = eventMouseMotion.Position / viewScale;
        }
    }

    public Vector3 GetMouseInWorld() {
        Plane dropPlane = new Plane(0f, 1f, 0f, pivotOffset);

        Vector3 from = camera.ProjectRayOrigin(mousePos);
        Vector3 to = from + camera.ProjectRayNormal(mousePos) * 1000f;
        Vector3? _mouse = dropPlane.IntersectRay(from, to);
        if (_mouse is null) {
            //GD.Print("Null");
            return Vector3.Zero;
        } else {
            return (Vector3) _mouse;
        }
    }

    public void SetBorder(Vector3 topRight, Vector3 bottomLeft) {
        borderTopRight = topRight + borderTopRightOffset;
        borderBottomLeft = bottomLeft + borderBottomLeftOffset;

        if (borderBottomLeft.x > borderTopRight.x) {
            borderTopRight.x = Mathf.Lerp(borderTopRight.x, borderBottomLeft.x, 0.5f);
            borderBottomLeft.x = borderTopRight.x;
        }
        if (borderTopRight.z > borderBottomLeft.z) {
            borderTopRight.z = Mathf.Lerp(borderBottomLeft.z, borderTopRight.z, 0.5f);
            borderBottomLeft.z = borderTopRight.z;
        }

        //GD.Print("Switching room");
    }
}
