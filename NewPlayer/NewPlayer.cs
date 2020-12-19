using Godot;
using System;

public class NewPlayer : KinematicBody {

    [Export]
    public NodePath modelHolderPath = new NodePath("ModelHolder");
    [Export]
    public NodePath cameraHolderPath = new NodePath("CameraHolder");
    [Export]
    public float gravity = 9.8f;
    [Export]
    public float maxSpeed = 3f;

    private Spatial modelHolder;
    private NewPlayerCamera cameraHolder;
    public AnimationNodeStateMachinePlayback stateMachine;
    public AnimationTree animTree;

    private float camRecoverTime = 1f;

    public override void _Ready() {
        modelHolder = GetNode<Spatial>(modelHolderPath);
        cameraHolder = GetNode<NewPlayerCamera>(cameraHolderPath);

        animTree = GetNode<AnimationTree>("ModelHolder/PlayerModel/AnimationTree");
        stateMachine = (AnimationNodeStateMachinePlayback) animTree.Get("parameters/playback");

        Input.SetMouseMode(Input.MouseMode.Captured);

        AnimationPlayer animPlayer = GetNode<AnimationPlayer>("ModelHolder/PlayerModel/AnimationPlayer");
        AudioStreamPlayer3D aud = GetNode<AudioStreamPlayer3D>("ModelHolder/PlayerModel/AudioStreamPlayer");

        int _track = animPlayer.GetAnimation("Walk-loop").AddTrack(Animation.TrackType.Audio);
        animPlayer.GetAnimation("Walk-loop").TrackSetPath(_track, "AudioStreamPlayer:playing");
        animPlayer.GetAnimation("Walk-loop").AudioTrackInsertKey(_track, 0f, aud.Stream);
        animPlayer.GetAnimation("Walk-loop").AudioTrackInsertKey(_track, 1.3f / 2f, aud.Stream);
        
        _track = animPlayer.GetAnimation("Run-loop").AddTrack(Animation.TrackType.Audio);
        animPlayer.GetAnimation("Run-loop").TrackSetPath(_track, "AudioStreamPlayer:playing");
        animPlayer.GetAnimation("Run-loop").AudioTrackInsertKey(_track, 0f, aud.Stream);
        animPlayer.GetAnimation("Run-loop").AudioTrackInsertKey(_track, 1f / 2f, aud.Stream);
        
        _track = animPlayer.GetAnimation("WalkSlow-loop").AddTrack(Animation.TrackType.Audio);
        animPlayer.GetAnimation("WalkSlow-loop").TrackSetPath(_track, "AudioStreamPlayer:playing");
        animPlayer.GetAnimation("WalkSlow-loop").AudioTrackInsertKey(_track, 0f, aud.Stream);
        animPlayer.GetAnimation("WalkSlow-loop").AudioTrackInsertKey(_track, 2.6f / 2f, aud.Stream);
    }

    public override void _Process(float delta) {

        if (GetNode<RayCast>("ModelHolder/RayCast").GetCollider() is InteractiveObject) {

            InteractiveObject _a = (InteractiveObject) GetNode<RayCast>("ModelHolder/RayCast").GetCollider();
            // Vector2 _obj_pos = camera.camera.UnprojectPosition(_a.GlobalTransform.origin);

            // playerGUI.SelectShow(_obj_pos, _a.labelSize, _a.objName);

            if (Input.IsActionJustPressed("key_use")) {
                _a.UseObject();
            }
        } else {
            // playerGUI.SelectHide();
        }
    }

    public override void _PhysicsProcess(float delta) {
        ProcessInput(delta); 
    }
    
    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion eventMouseMotion) {
            //mousePos = eventMouseMotion.Position / viewScale;
            //mousePos = eventMouseMotion.Relative;
            GetNode<Timer>("Timer").Start(camRecoverTime);
        }
    }

    private void ProcessInput(float delta) {
        Vector3 _movement = Vector3.Zero;

        if (Input.IsActionPressed("move_right")) {
            _movement.x += 1f;
        }
        if (Input.IsActionPressed("move_left")) {
            _movement.x -= 1f;
        }
        if (Input.IsActionPressed("move_up")) {
            _movement.z -= 1f;
        }
        if (Input.IsActionPressed("move_down")) {
            _movement.z += 1f;
        }

        _movement = _movement.Normalized();

        if (_movement != Vector3.Zero || Input.IsActionPressed("mb_right")) {
            Vector3 _rot = modelHolder.Rotation;
             float _ang = Mathf.Pi - Mathf.Atan2(-_movement.x, _movement.z);
            _rot.y = Mathf.LerpAngle(_rot.y, cameraHolder.Rotation.y/* + _ang*/, delta * 10f);
            //_rot.y = Mathf.Wrap(_rot.y, 0, Mathf.Tau);
            modelHolder.Rotation = _rot;
            //GD.Print(Mathf.Rad2Deg(Mathf.Pi - Mathf.Atan2(-_movement.x, _movement.z)));
        } 
        if (_movement != Vector3.Zero) {
            float fovLerpSpd = GameHelper.GetDeltaValue(0.01f, delta);
            if (Input.IsActionPressed("move_sprint")) {
                stateMachine.Travel("Run-loop");
                camRecoverTime = 0.4f;
                cameraHolder.InterpolateFOV(75f, fovLerpSpd);
            } else 
            if (Input.IsActionPressed("move_slow")){
                stateMachine.Travel("WalkSlow-loop");
                camRecoverTime = 2f;
                cameraHolder.InterpolateFOV(65f, fovLerpSpd);
            } else {
                stateMachine.Travel("Walk-loop");
                camRecoverTime = 1f;
                cameraHolder.InterpolateFOV(70f, fovLerpSpd);
            }
            if (GetNode<Timer>("Timer").TimeLeft == 0f) {
                float normalLerpSpd = GameHelper.GetDeltaValue(0.05f / camRecoverTime, delta);
                cameraHolder.InterpolateX(0f, normalLerpSpd);
            }
        } else {
            stateMachine.Travel("Idle-loop");
        }

        Transform rootMotion = animTree.GetRootMotionTransform();

        Vector3 _rott = modelHolder.Rotation;
        float _angg = 0f;
        if (_movement != Vector3.Zero) { // temporary. remove after animations redo
            _angg = Mathf.Pi - Mathf.Atan2(-_movement.x, _movement.z);
        }

        Vector3 _velocity = rootMotion.origin.Rotated(Vector3.Up, cameraHolder.Rotation.y + _angg + Mathf.Pi) / delta;

        if (!this.IsOnFloor()) {
            _velocity.y = -gravity;
        }


        this.MoveAndSlide(_velocity, Vector3.Up, true, 4, 0.785f, false);
    }

    // public Vector3 GetMouseInWorld() {
    //     Plane dropPlane = new Plane(0f, 1f, 0f, pivotOffset);

    //     Vector3 from = camera.ProjectRayOrigin(mousePos);
    //     Vector3 to = from + camera.ProjectRayNormal(mousePos) * 1000f;
    //     Vector3? _mouse = dropPlane.IntersectRay(from, to);
    //     if (_mouse is null) {
    //         //GD.Print("Null");
    //         return Vector3.Zero;
    //     } else {
    //         return (Vector3) _mouse;
    //     }
    // }


}
