using Godot;
using System;

public partial class LevelChest : Area2D
{
    private bool _isOpened = false;
    private Sprite2D _visual;
    private Label _hintLabel;
    
    [Export] public NodePath TargetExitPath;
    private LevelExit _targetExit;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1; // Player
        BodyEntered += OnBodyEntered;

        // Visual
        _visual = new Sprite2D();
        _visual.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Environment/treasure_chest_closed.png");
        _visual.Scale = new Vector2(1.0f, 1.0f); // Tăng tỉ lệ cho rương to rõ ràng
        AddChild(_visual);

        // Label
        _hintLabel = new Label();
        _hintLabel.Text = "[ E ] MỞ RƯƠNG";
        _hintLabel.Position = new Vector2(-100, -120);
        _hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _hintLabel.Visible = false;
        AddChild(_hintLabel);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!_isOpened && (body.IsInGroup("player") || body is Player || body is IsometricPlayer))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        _isOpened = true;
        
        // Hiệu ứng mở rương (Shake and Scale)
        var tween = CreateTween();
        tween.TweenProperty(_visual, "scale", new Vector2(1.2f, 1.2f), 0.1f);
        tween.TweenProperty(_visual, "scale", new Vector2(1.0f, 1.0f), 0.1f);
        
        // Chuyển sang sprite rương đã mở
        _visual.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Environment/treasure_chest_open.png");
        
        // Hiện thông báo lấy chìa khóa
        _hintLabel.Text = "★ BẠN ĐÃ NHẬN ĐƯỢC CHÌA KHÓA! ★\nCửa hang đã được mở khóa!";
        _hintLabel.Visible = true;
        _hintLabel.AddThemeColorOverride("font_color", Colors.Yellow);

        // Kích hoạt lối thoát
        if (TargetExitPath != null)
        {
            _targetExit = GetNode<LevelExit>(TargetExitPath);
        }
        else
        {
            // Fallback: Tìm trong group
            var exits = GetTree().GetNodesInGroup("LevelExit");
            if (exits.Count > 0) _targetExit = exits[0] as LevelExit;
        }

        if (_targetExit != null)
        {
            _targetExit.Activate();
        }

        GD.Print("Chest opened! Key collected.");
    }
}
