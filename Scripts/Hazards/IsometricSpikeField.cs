using Godot;
using System;

/// <summary>
/// Bẫy gai Isometric: Gai trồi lên rồi rút xuống theo chu kỳ.
/// So với SpikeHazard cũ, hoạt động trong không gian top-down (không cần trọng lực).
/// </summary>
public partial class IsometricSpikeField : Node2D
{
    [Export] public int Damage = 20;
    [Export] public float DamageCooldown = 1.0f;
    [Export] public float UpDuration = 2.0f;
    [Export] public float DownDuration = 2.5f;
    [Export] public float StartDelay = 0.0f;
    [Export] public int SpikeCount = 5;
    [Export] public float SpikeSpacing = 20f;
    [Export] public float SpikeHeight = 24f;
    [Export] public float RiseSpeed = 200f;

    private bool _isUp = false;
    private float _timer = 0f;
    private float _currentHeight = 0f;
    private bool _wantUp = false;
    private bool _inWarnPhase = false;
    private float _warnTimer = 0f;
    private const float WarnDuration = 0.4f;

    private Area2D _hitArea;
    private CollisionShape2D _hitCollision;
    private bool _canDamage = true;
    private Timer _damageCooldownTimer;

    // Màu sắc gai pixel-art
    private static readonly Color ColBase = new Color(0.38f, 0.24f, 0.12f, 1.0f);
    private static readonly Color ColMid = new Color(0.56f, 0.38f, 0.20f, 1.0f);
    private static readonly Color ColTip = new Color(0.92f, 0.76f, 0.40f, 1.0f);
    private static readonly Color ColShadow = new Color(0.15f, 0.08f, 0.02f, 0.6f);
    private static readonly Color ColWarning = new Color(1.0f, 0.3f, 0.1f, 0.6f);

    public override void _Ready()
    {
        BuildCollision();
        BuildDamageTimer();
        if (StartDelay > 0) _timer = -StartDelay;
        _currentHeight = 0f;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _timer += dt;

        if (!_isUp)
        {
            if (_inWarnPhase)
            {
                _warnTimer += dt;
                // Rung cảnh báo: mặt đất chớp đỏ
                Modulate = (_warnTimer % 0.1f < 0.05f)
                    ? new Color(1f, 0.7f, 0.3f)
                    : Colors.White;

                if (_warnTimer >= WarnDuration)
                {
                    _inWarnPhase = false;
                    _isUp = true;
                    _wantUp = true;
                    if (_hitArea != null) _hitArea.Monitoring = true; // Bật giám sát ngay lập tức
                    Modulate = Colors.White;
                }
            }
            else if (_timer >= DownDuration)
            {
                _timer = 0f;
                _warnTimer = 0f;
                _inWarnPhase = true;
            }
        }
        else
        {
            if (_timer >= UpDuration)
            {
                _timer = 0f;
                _isUp = false;
                _wantUp = false;
                if (_hitArea != null) _hitArea.Monitoring = false; // Tắt giám sát
                Modulate = Colors.White;
            }
        }

        // Animate height
        float target = _wantUp ? SpikeHeight : 0f;
        _currentHeight = Mathf.MoveToward(_currentHeight, target, RiseSpeed * dt);

        // LIÊN TỤC KIỂM TRA SÁT THƯƠNG KHI GAI ĐANG LÊN (Fix lỗi đứng yên không mất máu)
        if (_isUp && _canDamage && _hitArea != null)
        {
            var bodies = _hitArea.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is IsometricPlayer player)
                {
                    player.TakeDamage(Damage);
                    _canDamage = false;
                    _damageCooldownTimer.Start();
                    break;
                }
            }
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        float total = (SpikeCount - 1) * SpikeSpacing;
        float startX = -total / 2f;

        // Vẽ nền bẫy (đĩa nâu hình oval isometric)
        DrawEllipse(Vector2.Zero, total / 2f + 12, 8f, new Color(0.25f, 0.15f, 0.08f, 0.7f));

        // Vẽ vùng cảnh báo khi sắp trồi
        if (_inWarnPhase)
        {
            DrawEllipse(Vector2.Zero, total / 2f + 8, 6f, ColWarning);
        }

        if (_currentHeight < 0.5f) return;

        float h = _currentHeight;
        float hw = 6f;

        for (int i = 0; i < SpikeCount; i++)
        {
            float cx = startX + i * SpikeSpacing;
            float baseY = 2f;
            float tipY = baseY - h;

            // Bóng đổ
            DrawColoredPolygon(new[] {
                new Vector2(cx - hw + 2, baseY + 1),
                new Vector2(cx + hw + 2, baseY + 1),
                new Vector2(cx + 2, tipY + 2)
            }, ColShadow);

            // Mặt phải tối
            DrawColoredPolygon(new[] {
                new Vector2(cx, baseY),
                new Vector2(cx + hw, baseY),
                new Vector2(cx, tipY)
            }, ColBase);

            // Mặt trái sáng hơn
            DrawColoredPolygon(new[] {
                new Vector2(cx - hw, baseY),
                new Vector2(cx, baseY),
                new Vector2(cx, tipY)
            }, ColMid);

            // Điểm sáng đầu nhọn
            if (h > SpikeHeight * 0.5f)
                DrawCircle(new Vector2(cx, tipY), 1.5f, ColTip);
        }
    }

    private void DrawEllipse(Vector2 center, float rx, float ry, Color color)
    {
        int segments = 24;
        Vector2[] points = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.Pi * 2;
            points[i] = center + new Vector2(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry);
        }
        DrawColoredPolygon(points, color);
    }

    private void BuildCollision()
    {
        _hitArea = new Area2D();
        _hitArea.CollisionLayer = 0;
        _hitArea.CollisionMask = 1;
        _hitArea.Monitoring = false; // Tắt giám sát mặc định

        _hitCollision = new CollisionShape2D();
        var shape = new RectangleShape2D();
        float totalW = (SpikeCount - 1) * SpikeSpacing + 16;
        // Tăng Size Y từ 24 lên 64 để bắt trúng nhân vật dễ hơn (dầy gấp đôi)
        shape.Size = new Vector2(totalW, 64f); 
        _hitCollision.Shape = shape;
        _hitCollision.Disabled = false; // Luôn bật collision, ta dùng Monitoring để kiểm soát

        _hitArea.AddChild(_hitCollision);
        _hitArea.BodyEntered += OnBodyEntered;
        AddChild(_hitArea);
    }

    private void BuildDamageTimer()
    {
        _damageCooldownTimer = new Timer();
        _damageCooldownTimer.WaitTime = DamageCooldown;
        _damageCooldownTimer.OneShot = true;
        _damageCooldownTimer.Timeout += () => _canDamage = true;
        AddChild(_damageCooldownTimer);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_isUp && _canDamage)
        {
            if (body is IsometricPlayer player)
            {
                player.TakeDamage(Damage);
                _canDamage = false;
                _damageCooldownTimer.Start();
            }
        }
    }
}
