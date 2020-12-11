using Godot;
using System;

public class NewPlayerCamera : Spatial {
    [Export]
    public Vector2 mouseSensetivity = new Vector2(0.5f, 0.5f);
    [Export]
    public bool invertMouseX = true;
    [Export]
    public bool invertMouseY = true;
    [Export]
    public Vector2 maxTilt = new Vector2(-45f, 45f);

    private Area collisionArea;
    private Vector3 defaultCameraPos = new Vector3(1f, 0f, 2f);
    private Vector3 desiredCameraPos = new Vector3(1f, 0f, 2f);

    private Vector3 tiltLookUp = new Vector3(1f, 0.25f, 2f);
    private Vector3 tiltLookForward = new Vector3(1f, 0f, 2f);
    private Vector3 tiltLookDown = new Vector3(1f, 0.25f, 1.5f);

    private Vector3 desiredRotation = Vector3.Zero;

    private Vector2 mousePos = Vector2.Zero;

    private SkeletonIK skeletIK;
    private Position3D position3D;

    public override void _Ready() {
        collisionArea = GetNode<Area>("Camera/Area");
        collisionArea.Connect("body_entered", this, nameof(BodyEntered));
        collisionArea.Connect("body_exited", this, nameof(BodyExited));

        skeletIK = GetNode<SkeletonIK>("../PlayerModel/Armature/Skeleton/SkeletonIK");
        position3D = GetNode<Position3D>("../PlayerModel/Armature/Skeleton/SkeletonIK/Position3D");
    }

    public override void _Process(float delta) {
        ProcessMouse(delta);

        float tilt = Mathf.Rad2Deg(this.Rotation.x);
        float mAng = 25f;
        float amount = (Mathf.Abs(tilt) - mAng) / (45 - mAng);

        if (tilt < -mAng) {
            defaultCameraPos = tiltLookForward.LinearInterpolate(tiltLookDown, amount);
        } else if (tilt > mAng) {
            defaultCameraPos = tiltLookForward.LinearInterpolate(tiltLookUp, amount);
        } else {
            defaultCameraPos = tiltLookForward;
        }

        if (Input.IsActionPressed("mb_right")) {
            //new Vector3(1.5f, 0f, 1.5f)
            float lerpSpd = 0.25f;
            desiredCameraPos.x = Mathf.Lerp(desiredCameraPos.x, 1.5f, lerpSpd);
            desiredCameraPos.z = Mathf.Lerp(desiredCameraPos.z, 1.5f, lerpSpd);
            desiredCameraPos.y = Mathf.Lerp(desiredCameraPos.y, defaultCameraPos.y, lerpSpd);
        } else {
            float lerpSpd = 0.25f;
            desiredCameraPos = desiredCameraPos.LinearInterpolate(defaultCameraPos, lerpSpd);
        }        

        Vector3 _defCamPos = desiredCameraPos;
        Vector3 _camPos = _defCamPos;
        if (GetNode<RayCast>("RayCast").IsColliding()) {

            Vector3 _pos = GetNode<RayCast>("RayCast").GetCollisionPoint();
            Vector3 _ori = this.GlobalTransform.origin;
            Vector3 _desired = _defCamPos;
            float _len = _ori.DistanceTo(_pos);
            float _maxlen = Vector3.Zero.DistanceTo(_desired);
            _len = Mathf.Min(_len - 1f, _maxlen);
            _camPos = _desired * (_len / _maxlen);
        }
        Camera _cam = GetNode<Camera>("Camera");
        _cam.Translation = _cam.Translation.LinearInterpolate(_camPos, 0.5f);

    }
    
    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion eventMouseMotion) {
            //mousePos = eventMouseMotion.Position / viewScale;
            mousePos = eventMouseMotion.Relative;
        }
    }
    

    private void ProcessMouse(float delta) {
        float _cameraPan = mousePos.x;
        float _cameraTilt = mousePos.y;

        desiredRotation.x += _cameraTilt * delta * mouseSensetivity.y * (invertMouseY ? -1f : 1f);
        desiredRotation.y += _cameraPan * delta * mouseSensetivity.x * (invertMouseX ? -1f : 1f);

        desiredRotation.x = Mathf.Clamp(desiredRotation.x, Mathf.Deg2Rad(maxTilt.x), Mathf.Deg2Rad(maxTilt.y));

        desiredRotation.y = Mathf.Wrap(desiredRotation.y, 0, Mathf.Tau);
        // cameraHolder.GlobalRotate(Vector3.Up, _cameraPan * delta * mouseSensetivity.x);
        // cameraHolder.GlobalRotate(Vector3.Right, _cameraTilt * delta * mouseSensetivity.y);

        float lerpSpd = 0.1f;
        Vector3 _rot = this.Rotation;

        _rot.x = Mathf.LerpAngle(_rot.x, desiredRotation.x, lerpSpd);
        _rot.y = Mathf.LerpAngle(_rot.y, desiredRotation.y, lerpSpd);

        this.Rotation = _rot;

        //GD.Print(cameraHolder.RotationDegrees);

        mousePos = Vector2.Zero;
    }

    public void BodyEntered(Node _body) {
        if (_body is NewPlayer) {
            NewPlayer _pl = (NewPlayer) _body;
            _pl.Visible = false;
        }
    }

    public void BodyExited(Node _body) {
        if (_body is NewPlayer) {
            NewPlayer _pl = (NewPlayer) _body;
            _pl.Visible = true;
        }
    }
}
