using Godot;
using System;

public class RoomCamera : Area {

    [Export]
    public String bottomLeftName = "PosBL";
    [Export]
    public String topRightName = "PosTR";
    [Export]
    public String collisionShape = "CollisionShape";
    [Export]
    public String sdoorHighlights = "DoorHighlights";
    [Export]
    public String sblackForeground = "BlackForeground";

    private Position3D bottomLeft;
    private Position3D topRight;

    private Vector3 posTopRight;
    private Vector3 posBottomLeft;

    private Spatial doorHighlights;
    private CSGMesh blackForeground;

    public override void _Ready() {
        doorHighlights = GetNodeOrNull<Spatial>(sdoorHighlights);
        blackForeground = GetNodeOrNull<CSGMesh>(sblackForeground);
        if (!(doorHighlights is null)) {
            doorHighlights.Visible = false;
        }
        if (!(blackForeground is null)) {
            blackForeground.Visible = true;
        }

        bottomLeft = GetNodeOrNull<Position3D>(bottomLeftName);
        topRight = GetNodeOrNull<Position3D>(topRightName);
        if ((bottomLeft is null) || (topRight is null)) {
            bottomLeft = new Position3D();
            topRight = bottomLeft;
            this.AddChild(bottomLeft);
            bottomLeft.Translation = Vector3.Zero;
        }
        Vector3 _pos = Vector3.Zero;
        Vector3 _size = Vector3.Zero;
        float _xleft = bottomLeft.GlobalTransform.origin.x;
        float _xright = topRight.GlobalTransform.origin.x;
        float _ztop = topRight.GlobalTransform.origin.z;
        float _zbottom = bottomLeft.GlobalTransform.origin.z;
        /// bottom max (z positive) = corner - 12
        /// top max (z negative) = corner + 9
        /// left max (x negative) = corner + 13
        /// right max (x positive) = corner - 15
        _pos.x = Mathf.Lerp(_xleft, _xright, 0.5f);
        _pos.z = Mathf.Lerp(_zbottom, _ztop, 0.5f);
        _size.x = Mathf.Abs(_xright - _xleft) * 0.5f;
        _size.z = Mathf.Abs(_ztop - _zbottom) * 0.5f;
        _size.y = 100f;

        posTopRight = new Vector3(_xright, 0f, _ztop);
        posBottomLeft = new Vector3(_xleft, 0f, _zbottom);

        this.Connect("body_entered", this, nameof(EnterRoom));
        this.Connect("body_exited", this, nameof(ExitRoom));
    }

    public void EnterRoom(Node body) {
        if (body.GetParent() is Player) {
            Player _pl = (Player) body.GetParent();
            _pl.camera.SetBorder(posTopRight, posBottomLeft);
            _pl.GetNode<RoomTransition>("Control").FadeIn();
            if (!(doorHighlights is null)) {
                doorHighlights.Visible = true;
            }
            if (!(blackForeground is null)) {
                blackForeground.Visible = false;
            }
        }
    }

    public void ExitRoom(Node body) {
        if (body.GetParent() is Player) {
            Player _pl = (Player) body.GetParent();
            _pl.GetNode<RoomTransition>("Control").FadeOut();
            if (!(doorHighlights is null)) {
                doorHighlights.Visible = false;
            }
            if (!(blackForeground is null)) {
                blackForeground.Visible = true;
            }
        }
    }
}
