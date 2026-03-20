using Godot;

/// <summary>
/// Cánh cửa đá cổ kính ở cuối Ải 1.
/// Khi Player đến chạm vào → FadeOut → chuyển sang Ải 2.
/// </summary>
public partial class CaveDoor : Area2D
{
    [Export] public string NextScenePath = "res://Scenes/Levels/Level2.tscn";
    [Export] public float TransitionDelay = 0.0f; // Immediate transition for better feel

    private bool _isTriggered = false;
    private bool _isActive = false; // Mặc định bị khóa
    private Label _hintLabel;
    private AnimatedSprite2D _doorGlow;

    public override void _Ready()
    {
        AddToGroup("LevelExit"); // Cho phép Rương báu tìm thấy để mở khóa
        CollisionLayer = 0;
        CollisionMask = 1; // Detect Player

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        // Tạo label gợi ý
        _hintLabel = new Label();
        _hintLabel.Text = "✦ Cửa Hang ✦\nLối vào...";
        _hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _hintLabel.VerticalAlignment = VerticalAlignment.Center;
        _hintLabel.Position = new Vector2(-80, -95);
        _hintLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.4f)); // Vàng ánh
        _hintLabel.AddThemeFontSizeOverride("font_size", 15);
        _hintLabel.Visible = false;
        AddChild(_hintLabel);

        // Ban đầu bị khóa (Mờ tối)
        Modulate = new Color(0.4f, 0.4f, 0.4f);
    }

    public void Activate()
    {
        _isActive = true;
        Modulate = Colors.White;
        
        // Hiệu ứng glow rực rỡ khi được mở khóa
        StartDoorGlowEffect();
        
        _hintLabel.Text = "✧ Cửa Hang Đã Mở ✧\nTiến vào...";
        _hintLabel.Visible = true;
        
        var tw = GetTree().CreateTimer(3.0f);
        tw.Timeout += () => { if (IsInstanceValid(_hintLabel)) _hintLabel.Visible = false; };
    }

    public override void _Process(double delta)
    {
        // Animate label nhấp nháy nhẹ
        if (_hintLabel != null && _hintLabel.Visible)
        {
            float alpha = 0.7f + 0.3f * Mathf.Sin((float)Time.GetTicksMsec() * 0.003f);
            _hintLabel.Modulate = new Color(1, 1, 1, alpha);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_isTriggered) return;
        if (body == null || !body.IsInGroup("player")) return;

        if (!_isActive)
        {
            // Dự phòng: Nếu chưa mở rương mà đòi vào
            _hintLabel.Text = "Cửa hang đã bị khóa!\nHãy mở Rương Báu trước.";
            _hintLabel.Visible = true;
            _hintLabel.AddThemeColorOverride("font_color", Colors.Orange);
            return;
        }

        _isTriggered = true;
        _hintLabel.Visible = false;

        // Ép Player chuyển sang Cutscene Mode: Khóa phím, tiếp tục chạy bộ vào cửa phía bên phải
        if (body.HasMethod("WalkIntoCave"))
            body.Call("WalkIntoCave", 1f);

        // Chuyển màn ngay lập tức khi chạm cửa
        DoTransition();
    }

    private void OnBodyExited(Node2D body)
    {
        if (body != null && body.IsInGroup("player") && !_isTriggered)
        {
            _hintLabel.Visible = false;
        }
    }

    private void DoTransition()
    {
        // Nhờ GameManager kích hoạt Fade Global Màn Hình và load Scene kế!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CallDeferred("NextLevel");
        }
    }

    private void StartDoorGlowEffect()
    {
        // Tạo hiệu ứng ánh sáng xung quanh cửa
        // Dùng Tween để pulse màu sắc node cha
        var tween = CreateTween();
        tween.SetLoops(); // Lặp vô tận
        tween.TweenProperty(this, "modulate", new Color(1.2f, 1.1f, 0.8f), 1.2f);
        tween.TweenProperty(this, "modulate", Colors.White, 1.2f);
    }
}
