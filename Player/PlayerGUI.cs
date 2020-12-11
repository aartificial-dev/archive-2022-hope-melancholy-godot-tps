using Godot;
using System;

public class PlayerGUI : Control {
    private int currentFadeState = 0; // 0 - still, 1 - fade black, 2 - black, 3 - fade white
    private ColorRect fadeRect;

    private NinePatchRect selectRect;
    private Label selectLabel;
    private float selectLabelLength = 0f;
    private float selectLabelTextSpeed = 12f; // char per second
    private String selectLabelText = "";
    private bool selectShow = false;

    public override void _Ready() {
        fadeRect = GetNode<ColorRect>("ColorRect");
        selectRect = GetNode<NinePatchRect>("Select");
        selectLabel = GetNode<Label>("Select/Label");
    }

    public override void _Process(float delta) {
        ProcessFadeRect(delta);
        ProcessSelect(delta);
    }

    private void ProcessSelect(float delta) {
        if (selectRect.Visible == true) {
            if (selectShow == true) {
                selectLabelLength += (selectLabelTextSpeed * delta);
            } else {
                selectLabelLength -= (selectLabelTextSpeed * delta * 4f);
                if (selectLabelLength <= 0) {
                    selectRect.Visible = false;
                    selectLabel.Text = "";
                }
            }
            selectLabelLength = Mathf.Clamp(selectLabelLength, 0, selectLabelText.Length);
            String _new_text = selectLabelText.Substr(0, (int) selectLabelLength);
            selectLabel.Text = _new_text;
        }
    }

    public void SelectShow(Vector2 pos, Vector2 size, String text) {
        if (!text.StartsWith(selectLabelText)) {selectLabel.Text = "";}
        selectShow = true;
        selectRect.Visible = true;
        selectRect.RectSize = size.Floor();
        selectRect.RectPivotOffset = size / 2f;
        selectRect.RectPosition = pos.Floor() - selectRect.RectPivotOffset.Floor();// / topCamera.viewScale;

        selectLabel.RectPosition = new Vector2(size.x + 4f, 2);

        selectLabelText = text;
    }

    public void SelectHide() {
        selectShow = false;
    }

    private void ProcessFadeRect(float delta) {
        Color _col = fadeRect.Color;
        if (currentFadeState == 0) {
            _col.a = 0f;
        }
        if (currentFadeState == 1) {
            _col.a = Mathf.Lerp(_col.a, 1f, delta * 4f);
            if (_col.a > 0.95f) {
                _col.a = 1f;
                currentFadeState = 2;
            }
        }
        if (currentFadeState == 2) {
            GetParent<Player>().camera.isLocked = false;
            _col.a = 1f;
            currentFadeState = 3;
        }
        if (currentFadeState == 3) {
            _col.a = Mathf.Lerp(_col.a, 0f, delta * 4f);
            if (_col.a < 0.05f) {
                _col.a = 0f;
                currentFadeState = 0;
            }
        }
        fadeRect.Color = _col;
    }

    public void FadeIn() { // to alpha
        //currentFadeState = 3;
    }

    public void FadeOut() { // to black
        currentFadeState = 1;
        GetParent<Player>().camera.isLocked = true;
    }
}
