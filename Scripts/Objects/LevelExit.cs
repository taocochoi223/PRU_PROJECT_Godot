using Godot;

public partial class LevelExit : Area2D
{
    private bool _isActive = false;
    private Label _hintLabel;
    private Sprite2D _visual;

    public override void _Ready()
    {
        AddToGroup("LevelExit");
        CollisionLayer = 0;
        CollisionMask = 1; // Player
        BodyEntered += OnBodyEntered;

        // Visual (Cổng ra sáng rực)
        _visual = new Sprite2D();
        _visual.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Backgrounds/Cave_door.png");
        _visual.Scale = new Vector2(0.5f, 0.5f);
        _visual.Modulate = new Color(0.5f, 1.0f, 0.5f, 0.5f); // Màu xanh mờ ban đầu
        AddChild(_visual);

        // Label gợi ý
        _hintLabel = new Label();
        _hintLabel.Text = "✦ LỐI THOÁT ✦";
        _hintLabel.Position = new Vector2(-60, -110);
        _hintLabel.AddThemeFontSizeOverride("font_size", 12);
        _hintLabel.Visible = false;
        AddChild(_hintLabel);

        // Ban đầu bị khóa (Mờ tối)
        Visible = true;
        _isActive = false;
        _visual.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    }

    public void Activate()
    {
        _isActive = true;
        Visible = true;
        _visual.Modulate = Colors.White; // Hiện rõ cổng
        
        // Hiệu ứng phát sáng
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_visual, "modulate", new Color(1.5f, 1.5f, 1.5f), 0.8f);
        tween.TweenProperty(_visual, "modulate", Colors.White, 0.8f);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player") || body is Player || body is IsometricPlayer)
        {
            if (!_isActive)
            {
                // Hiện thông báo khóa
                _hintLabel.Text = "✧ HỐI TIẾC! Cửa hang đã bị khóa ✧\n[ Hãy tìm Rương Báu để lấy Chìa Khóa ]";
                _hintLabel.Visible = true;
                _hintLabel.AddThemeColorOverride("font_color", Colors.Orange);
                _hintLabel.AddThemeFontSizeOverride("font_size", 11);
                _hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
                _hintLabel.Position = new Vector2(-150, -130);
                
                var tw = GetTree().CreateTimer(3.0f);
                tw.Timeout += () => { if (IsInstanceValid(_hintLabel)) _hintLabel.Visible = false; };
                return;
            }
            
            GD.Print("Chúc mừng! Bạn đã hoàn thành level!");
            GameManager.Instance.NextLevel();
        }
    }
}
