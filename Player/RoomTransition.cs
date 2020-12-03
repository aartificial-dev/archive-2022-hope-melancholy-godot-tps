using Godot;
using System;

public class RoomTransition : Control {
    private float currentAlpha = 0f;
    private int currentState = 0; // 0 - still, 1 - fade black, 2 - black, 3 - fade white
    private ColorRect _rect;

    public override void _Ready() {
        _rect = GetNode<ColorRect>("ColorRect");
    }

    public override void _Process(float delta) {
        Color _col = _rect.Color;
        if (currentState == 0) {
            _col.a = 0f;
        }
        if (currentState == 1) {
            _col.a = Mathf.Lerp(_col.a, 1f, delta * 4f);
            if (_col.a > 0.95f) {
                _col.a = 1f;
                currentState = 2;
            }
        }
        if (currentState == 2) {
            GetParent<Player>().camera.isLocked = false;
            _col.a = 1f;
            currentState = 3;
        }
        if (currentState == 3) {
            _col.a = Mathf.Lerp(_col.a, 0f, delta * 4f);
            if (_col.a < 0.05f) {
                _col.a = 0f;
                currentState = 0;
            }
        }
        _rect.Color = _col;
    }

    public void FadeIn() { // to alpha
        //currentState = 3;
    }

    public void FadeOut() { // to black
        currentState = 1;
        GetParent<Player>().camera.isLocked = true;
    }
}
