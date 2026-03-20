using Godot;
using System.Collections.Generic;

/// <summary>
/// Gai nhọn trồi lên từ mặt đất – dùng _Draw() vẽ tam giác pixel-art.
/// Đặt node TẠI bề mặt đất (y = top of ground), gai mọc LÊN (y âm).
/// </summary>
public partial class SpikeHazard : Node2D
{
    [Export] public int   Damage         = 25;
    [Export] public float DamageCooldown = 0.8f;

    // ── Timing ──────────────────────────────────────────────
    [Export] public float UpDuration   = 2.0f;
    [Export] public float DownDuration = 1.5f;
    [Export] public float StartDelay   = 0.0f;

    // ── Visual ──────────────────────────────────────────────
    [Export] public int   SpikeCount   = 4;
    [Export] public float SpikeSpacing = 18f;    // px giữa tâm các gai
    [Export] public float SpikeHeight  = 32f;    // chiều cao tối đa mỗi gai
    [Export] public float SpikeWidth   = 13f;    // chiều rộng đáy mỗi gai
    [Export] public float RiseSpeed    = 220f;   // px/s khi trồi lên
    [Export] public float FallSpeed    = 360f;   // px/s khi rút xuống

    // Màu sắc pixel-art (đất nâu → mũi nhọn sáng)
    private static readonly Color ColShadow = new Color(0.18f, 0.10f, 0.04f, 0.70f);
    private static readonly Color ColDark   = new Color(0.38f, 0.24f, 0.12f, 1.0f);
    private static readonly Color ColMid    = new Color(0.56f, 0.38f, 0.20f, 1.0f);
    private static readonly Color ColLight  = new Color(0.76f, 0.58f, 0.34f, 1.0f);
    private static readonly Color ColTip    = new Color(0.92f, 0.80f, 0.55f, 1.0f);

    // ── State ───────────────────────────────────────────────
    private bool  _isUp          = false;
    private float _timer         = 0f;
    private float _currentHeight = 0f;   // 0 = ẩn hoàn toàn, SpikeHeight = trồi hết
    private bool  _wantUp        = false;

    // ── Collision ───────────────────────────────────────────
    private Area2D            _hitArea;
    private CollisionShape2D  _hitCollision;
    private Player            _playerOnSpike = null;
    private bool              _canDamage     = true;
    private Timer             _damageCooldownTimer;

    // ── Cảnh báo rung trước khi trồi ────────────────────────
    private float _warnTimer   = 0f;
    private bool  _inWarnPhase = false;
    private const float WarnDuration = 0.35f; // giây rung cảnh báo

    public override void _Ready()
    {
        BuildCollision();
        BuildDamageTimer();

        // Bắt đầu rút hoàn toàn
        if (StartDelay > 0)
            _timer = -StartDelay;

        _currentHeight = 0f;
        _wantUp        = false;
        _isUp          = false;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _timer += dt;

        if (!_isUp)
        {
            // ── Đang ở xuống ──
            if (_inWarnPhase)
            {
                _warnTimer += dt;
                // Rung nhẹ để cảnh báo (dùng modulate nhấp nháy)
                Modulate = (_warnTimer % 0.12f < 0.06f)
                    ? new Color(1f, 0.85f, 0.4f)
                    : Colors.White;

                if (_warnTimer >= WarnDuration)
                {
                    _inWarnPhase = false;
                    _isUp        = true;
                    _wantUp      = true;
                    _hitCollision?.SetDeferred("disabled", false);
                    Modulate = new Color(1.0f, 0.55f, 0.30f);
                }
            }
            else if (_timer >= DownDuration)
            {
                _timer       = 0f;
                _warnTimer   = 0f;
                _inWarnPhase = true;   // vào pha cảnh báo trước khi trồi
            }
        }
        else
        {
            // ── Đang ở trên ──
            if (_timer >= UpDuration)
            {
                _timer  = 0f;
                _isUp   = false;
                _wantUp = false;
                _hitCollision?.SetDeferred("disabled", true);
                Modulate = Colors.White;
            }

            if (_playerOnSpike != null && _canDamage)
                DamagePlayer(_playerOnSpike);
        }

        // ── Animate chiều cao ──
        float target = _wantUp ? SpikeHeight : 0f;
        float speed  = _wantUp ? RiseSpeed : FallSpeed;
        _currentHeight = Mathf.MoveToward(_currentHeight, target, speed * dt);

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_currentHeight < 0.5f) return;

        float total  = (SpikeCount - 1) * SpikeSpacing;
        float startX = -total / 2f;
        float h      = _currentHeight;
        float hw     = SpikeWidth / 2f;

        for (int i = 0; i < SpikeCount; i++)
        {
            float cx = startX + i * SpikeSpacing;

            // Đáy gai HỞ từ mặt đất (baseY = 2 = nằm dưới mặt đất 2px)
            float baseY = 2f;
            float tipY  = baseY - h;

            Vector2 bL  = new Vector2(cx - hw,       baseY);
            Vector2 bR  = new Vector2(cx + hw,       baseY);
            Vector2 tip = new Vector2(cx,             tipY);
            Vector2 mid = new Vector2(cx,             baseY); // trung điểm đáy

            // 1. Bóng đổ (phải & dưới mũi)
            DrawColoredPolygon(new[] {
                bL + new Vector2(2f, 1f),
                bR + new Vector2(2f, 1f),
                tip + new Vector2(2f, 2f)
            }, ColShadow);

            // 2. Mặt phải (tối)
            DrawColoredPolygon(new[] { mid, bR, tip }, ColDark);

            // 3. Mặt trái (sáng hơn)
            DrawColoredPolygon(new[] { bL, mid, tip }, ColMid);

            // 4. Viền mép trái (highlight)
            DrawLine(bL, tip, ColLight, 1.5f);

            // 5. Điểm sáng mũi nhọn
            if (h > SpikeHeight * 0.6f)
                DrawCircle(tip, 1.3f, ColTip);

            // 6. Đường viền nền (khớp với mặt đất)
            DrawLine(bL, bR, ColDark, 2f);
        }
    }

    // ───────────────────────────────────────────────────────────
    private void BuildCollision()
    {
        _hitArea = new Area2D();
        _hitArea.CollisionLayer = 0;
        _hitArea.CollisionMask  = 1;

        _hitCollision = new CollisionShape2D();
        var shape     = new RectangleShape2D();
        float totalW  = (SpikeCount - 1) * SpikeSpacing + SpikeWidth;
        shape.Size    = new Vector2(totalW, SpikeHeight);
        _hitCollision.Shape    = shape;
        _hitCollision.Position = new Vector2(0, -SpikeHeight / 2f);
        _hitCollision.Disabled = true;

        _hitArea.AddChild(_hitCollision);
        _hitArea.BodyEntered += OnBodyEntered;
        _hitArea.BodyExited  += OnBodyExited;
        AddChild(_hitArea);
    }

    private void BuildDamageTimer()
    {
        _damageCooldownTimer          = new Timer();
        _damageCooldownTimer.WaitTime = DamageCooldown;
        _damageCooldownTimer.OneShot  = true;
        _damageCooldownTimer.Timeout  += () => { _canDamage = true; };
        AddChild(_damageCooldownTimer);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player") || body is Player || body is IsometricPlayer)
        {
            if (body is Player p) _playerOnSpike = p;
            
            if (_canDamage && _isUp)
            {
                body.Call("TakeDamage", Damage);
                _canDamage = false;
                _damageCooldownTimer.Start();
            }
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("player") || body is Player || body is IsometricPlayer) _playerOnSpike = null;
    }

    private void DamagePlayer(Player player)
    {
        player.TakeDamage(Damage);
        _canDamage = false;
        _damageCooldownTimer.Start();
    }
}
