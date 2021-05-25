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

    public override void _Ready() {
        collisionArea = GetNode<Area>("Camera/Area");
        collisionArea.Connect("body_entered", this, nameof(BodyEntered));
        collisionArea.Connect("body_exited", this, nameof(BodyExited));
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

        float zoomLerpSpd = GameHelper.GetDeltaValue(0.1f, delta); // 0.25
        if (Input.IsActionPressed("mb_right")) {
            //new Vector3(1.5f, 0f, 1.5f)
            desiredCameraPos.x = Mathf.Lerp(desiredCameraPos.x, 1.35f, zoomLerpSpd);
            desiredCameraPos.z = Mathf.Lerp(desiredCameraPos.z, 1.35f, zoomLerpSpd);
            desiredCameraPos.y = Mathf.Lerp(desiredCameraPos.y, defaultCameraPos.y, zoomLerpSpd);
        } else {
            desiredCameraPos = desiredCameraPos.LinearInterpolate(defaultCameraPos, zoomLerpSpd);
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
        float camLerpSpd = GameHelper.GetDeltaValue(0.1f, delta); // 0.5f
        _cam.Translation = _cam.Translation.LinearInterpolate(_camPos, camLerpSpd);

    }
    
    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion eventMouseMotion) {
            //mousePos = eventMouseMotion.Position / viewScale;
            mousePos = eventMouseMotion.Relative;
        }
    }

    public void InterpolateX(float x, float amount) {
        desiredRotation.x = Mathf.LerpAngle(desiredRotation.x, x, amount);
    }

    public void InterpolateFOV(float fov, float amount) {
        Camera _cam = GetNode<Camera>("Camera");
        _cam.Fov = Mathf.Lerp(_cam.Fov, fov, amount);
    }
    

    private void ProcessMouse(float delta) {
        float _cameraPan = mousePos.x;
        float _cameraTilt = mousePos.y;

        Vector3 wantedRotation = desiredRotation;

        wantedRotation.x += _cameraTilt * delta * mouseSensetivity.y * (invertMouseY ? -1f : 1f);
        wantedRotation.y += _cameraPan * delta * mouseSensetivity.x * (invertMouseX ? -1f : 1f);

        wantedRotation.x = Mathf.Clamp(wantedRotation.x, Mathf.Deg2Rad(maxTilt.x), Mathf.Deg2Rad(maxTilt.y));

        wantedRotation.y = Mathf.Wrap(wantedRotation.y, 0, Mathf.Tau);

        if (Mathf.Abs(Mathf.Rad2Deg(AngleDiff(this.Rotation.y, wantedRotation.y))) <= 90f) {
            desiredRotation.y = wantedRotation.y;
        }
        desiredRotation.x = wantedRotation.x;
        
        Vector3 _rot = this.Rotation;
        float turnLerpSpd = GameHelper.GetDeltaValue(0.05f, delta); // 0.1f // 0.05f
        _rot.x = Mathf.LerpAngle(_rot.x, desiredRotation.x, turnLerpSpd);
        _rot.y = Mathf.LerpAngle(_rot.y, desiredRotation.y, turnLerpSpd);

        _rot.y = Mathf.Wrap(_rot.y, 0, Mathf.Tau);
        this.Rotation = _rot;

        mousePos = Vector2.Zero;
    }

    private float AngleDiff(float from, float to) {
        float ans = Mathf.PosMod(to - from, Mathf.Tau);
        if (ans > Mathf.Pi) ans -= Mathf.Tau;
        return ans;
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
