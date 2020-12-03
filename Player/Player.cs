using Godot;
using System;

public class Player : KinematicBody {
    public String shoulderCameraPath = "ModelHolder/Camera";

    public PlayerCamera camera;
    
    private float moveSpeed = 6f;
    private float velocity = 0f;

    public Vector3 mousePos = Vector3.Zero;
    public Spatial modelHolder;
    public Spatial cameraPoint;
    public AnimationNodeStateMachinePlayback stateMachine;
    public AnimationTree animTree;

    public NinePatchRect _selectHi;
    public Label _selectLabel;

    public override void _Ready() {
        camera = this.GetParent().GetNode<PlayerCamera>("CameraPivot");
        modelHolder = GetNode<Spatial>("ModelHolder");
        cameraPoint = GetNode<Spatial>("CameraPoint");
        animTree = GetNode<AnimationTree>("ModelHolder/gurl/AnimationTree");
        stateMachine = (AnimationNodeStateMachinePlayback) animTree.Get("parameters/playback");

        _selectHi = GetNode<NinePatchRect>("Control/Select");
        _selectLabel = GetNode<Label>("Control/Select/Label");
    }

    public override void _Process(float delta) {
        if (mousePos != Vector3.Zero) {

        }

        if (GetNode<RayCast>("ModelHolder/RayCast").GetCollider() is InteractiveObject) {

            InteractiveObject _a = (InteractiveObject) GetNode<RayCast>("ModelHolder/RayCast").GetCollider();
            Vector2 _obj_pos = camera.camera.UnprojectPosition(_a.GlobalTransform.origin);

            _selectHi.Visible = true;
            _selectHi.RectSize = _a.labelSize;
            _selectHi.RectPivotOffset = _a.labelSize / 2f;
            _selectHi.RectPosition = _obj_pos - _selectHi.RectPivotOffset;// / topCamera.viewScale;
            _selectLabel.RectPosition = new Vector2(_a.labelSize.x + 4f, 2);

            String _name = _a.objName;
            String _cur_text = _selectLabel.Text;
            int _max_text = _name.Length;
            int _text_pos = _cur_text.Length;
            String _new_text = _name.Substr(0, Mathf.Min(_text_pos + 1, _max_text));
            _selectLabel.Text = _new_text;

            if (Input.IsActionJustPressed("key_use")) {
                _a.UseObject();
            }
        } else {
            _selectHi.Visible = false;
            _selectLabel.Text = "";
        }
    }

    public override void _PhysicsProcess(float delta) {

        Vector3 direction = Vector3.Zero;
        Vector3 keyboard = Vector3.Zero;
        keyboard.x =  (Input.IsActionPressed("move_right") ? 1f : 0f) - (Input.IsActionPressed("move_left") ? 1f : 0f);
        keyboard.z =  (Input.IsActionPressed("move_down") ? 1f : 0f) - (Input.IsActionPressed("move_up") ? 1f : 0f);
        if (keyboard == Vector3.Zero) {
            direction.x = Input.GetJoyAxis(0, 0); // -l+r
            direction.z = Input.GetJoyAxis(0, 1); // -u+d
        } else {
            direction = keyboard;
        }
        if ((direction.x < 0.2 && direction.x >= 0f) || (direction.x > -0.2 && direction.x <= 0f)) {direction.x = 0f;}
        if ((direction.z < 0.2 && direction.z >= 0f) || (direction.z > -0.2 && direction.z <= 0f)) {direction.z = 0f;}
        
        if (direction == Vector3.Zero) {
            stateMachine.Travel("Idle-loop");
        } else {
            modelHolder.RotateY(modelHolder.Translation.DirectionTo(modelHolder.Translation + direction).y);
            if (direction.Length() < 0.5f) {
                stateMachine.Travel("WalkSlow-loop");
            } else if (direction.Length() < 0.8f) {
                stateMachine.Travel("Walk-loop");
            } else {
                stateMachine.Travel("Run-loop");
            }
        }

        Transform rootMotion = animTree.GetRootMotionTransform();
        Vector3 velocity = rootMotion.origin.Rotated(Vector3.Up, modelHolder.Rotation.y + Mathf.Pi) / delta;
        velocity.y = -1.5f;
        MoveAndSlide(velocity, Vector3.Up, true, 4, 0.785f, false);
    }

    private Vector3 GetMoveVector() {
        Vector3 _move = Vector3.Zero;
        if (Input.IsActionPressed("move_up")) {
            _move.z -= 1;
        }
        if (Input.IsActionPressed("move_down")) {
            _move.z += 1;
        }
        if (Input.IsActionPressed("move_right")) {
            _move.x += 1;
        }
        if (Input.IsActionPressed("move_left")) {
            _move.x -= 1;
        }
        return Vector3.Zero.DirectionTo(_move);
    }
} 
