using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody2D
{
    // Skill cooldowns
    private float _skill1Timer = 0f;
    private float _skill2Timer = 0f;
    private float _skill3Timer = 0f;

    private const float Skill1Cooldown = 2.0f;
    private const float Skill2Cooldown = 5.0f;
    private const float Skill3Cooldown = 15.0f;

    private bool _isSpinning = false;
    private static Texture2D _cachedAxeTexture = null;
    private static Texture2D _cachedPortraitTexture = null;

    private void PrepareAxeTexture()
    {
        if (_cachedAxeTexture != null) return;

        var fullTexture = GD.Load<Texture2D>("res://Assets/Sprites/Player/Riu_Skill.png");
        if (fullTexture == null) 
        {
            fullTexture = GD.Load<Texture2D>("res://Assets/Sprites/Player/divine_axe.png");
            if (fullTexture == null) return;
        }

        Image img = fullTexture.GetImage();
        if (img == null) return;
        img.Decompress();
        img.Convert(Image.Format.Rgba8);

        // THUẬT TOÁN TÁCH NỀN THÔNG MINH
        Color bgColor = img.GetPixel(0, 0); // Lấy màu ở góc trái trên làm màu nền
        for (int y = 0; y < img.GetHeight(); y++) {
            for (int x = 0; x < img.GetWidth(); x++) {
                Color p = img.GetPixel(x, y);
                // Tính khoảng cách màu
                float diff = Mathf.Sqrt(Mathf.Pow(p.R - bgColor.R, 2) + Mathf.Pow(p.G - bgColor.G, 2) + Mathf.Pow(p.B - bgColor.B, 2));
                if (diff < 0.35f) 
                    img.SetPixel(x, y, new Color(1, 1, 1, 0)); // Làm trong suốt
            }
        }

        _cachedAxeTexture = ImageTexture.CreateFromImage(img);
    }

    private void HandleSkills(float dt)
    {
        if (_skill1Timer > 0) _skill1Timer -= dt;
        if (_skill2Timer > 0) _skill2Timer -= dt;
        if (_skill3Timer > 0) _skill3Timer -= dt;

        if (_inCutscene || _isDead || _isHurt) return;

        // Use IsActionJustPressed for all skills. 
        // Fallback to direct key check but only on 'Just Pressed' logic via a timer or better, use GD.Print to warn
        if (Input.IsActionJustPressed("skill1") || (Input.IsKeyPressed(Key.Key1) && _skill1Timer <= 0 && !_isAttacking))
        {
            ExecuteAxeThrow();
        }
        else if (Input.IsActionJustPressed("skill2") || (Input.IsKeyPressed(Key.Key2) && _skill2Timer <= 0 && !_isAttacking))
        {
            ExecuteWhirlwind();
        }
        else if (Input.IsActionJustPressed("skill3") || (Input.IsKeyPressed(Key.Key3) && _skill3Timer <= 0 && !_isAttacking))
        {
            ExecuteEarthBreaker();
        }
    }

    // --- SKILL 1: AXE THROW ---
    private async void ExecuteAxeThrow()
    {
        _skill1Timer = Skill1Cooldown;
        _isAttacking = true; // Lock movement
        
        _sfxPlayer.Stream = SFX.GetAxeThrowSound();
        _sfxPlayer.Play();

        // Create axe projectile
        PrepareAxeTexture();
        var axe = new AxeProjectile();
        axe.Texture = _cachedAxeTexture;
        axe.GlobalPosition = GlobalPosition + new Vector2(_facingDirection * 20, -10);
        axe.Direction = new Vector2(_facingDirection, -0.1f).Normalized();
        axe.Damage = AttackDamage * 1.5f;
        
        // Find nearest enemy
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D nearest = null;
        float minDist = 500f;
        foreach (Node2D e in enemies.Cast<Node2D>())
        {
            float d = GlobalPosition.DistanceTo(e.GlobalPosition);
            if (d < minDist) { minDist = d; nearest = e; }
        }
        axe.Target = nearest;

        GetParent().AddChild(axe);
        _animatedSprite.Play("attack1");

        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        _isAttacking = false;
    }

    // --- SKILL 2: WHIRLWIND SPIN ---
    private async void ExecuteWhirlwind()
    {
        _skill2Timer = Skill2Cooldown;
        _isSpinning = true;
        // _isAttacking = false (để nhân vật có thể di chuyển/nhảy)
        
        float duration = 1.6f;
        float elapsed = 0f;
        
        _sfxPlayer.Stream = SFX.GetSpinSound();
        _sfxPlayer.Play();

        // 1. Lưu lại trạng thái gốc chính xác
        Vector2 baseScale = _animatedSprite.Scale;

        // 2. Hiệu ứng vòng xoáy (VFX chém vàng - ÉP DẸT CHIỀU NGANG)
        var spinVFX = new SpinVFX();
        spinVFX.Scale = new Vector2(1.3f, 0.35f); // Ép dẹt trục Y để thành vòng xoay ngang 3D
        spinVFX.Position = new Vector2(0, -25); // Nâng lên ngang tầm tay cầm rìu
        AddChild(spinVFX);

        while (elapsed < duration && !_isDead && !_isHurt)
        {
            float dt = 0.04f;
            elapsed += dt;
            
            float spinSpeed = 32.0f; // Xoay cực nhanh
            float angle = elapsed * spinSpeed;
            
            // --- NHÂN VẬT: Tự xoay (Flip) kết hợp Anim Tấn công ---
            _animatedSprite.Scale = baseScale; 
            _animatedSprite.FlipH = Mathf.Cos(angle) < 0;
            _animatedSprite.Play("attack1");

            // Cập nhật góc xoay cho hiệu ứng đồng bộ
            spinVFX.RotationAngle = angle;

            // Tạo bóng ma tốc độ để tăng cảm giác xoay tít mù
            if (Time.GetTicksMsec() % 100 < 50) CreateGhostEffect();

            // Gây sát thương diện rộng xung quanh
            var bodies = _attackArea.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body.IsInGroup("enemies") && body.HasMethod("TakeDamage"))
                    body.Call("TakeDamage", (int)(AttackDamage * 0.5f));
            }
            
            await ToSignal(GetTree().CreateTimer(dt), "timeout");
        }

        // Reset về đúng trạng thái ban đầu
        _animatedSprite.Scale = baseScale;
        _animatedSprite.Rotation = 0;
        _animatedSprite.FlipH = _facingDirection < 0;
        _animatedSprite.Play("idle");
        
        _isSpinning = false;
        if (spinVFX != null) spinVFX.QueueFree();
    }

    private void CreateGhostEffect()
    {
        var ghost = new Sprite2D();
        // Lấy texture hiện tại của animation
        ghost.Texture = _animatedSprite.SpriteFrames.GetFrameTexture(_animatedSprite.Animation, _animatedSprite.Frame);
        ghost.GlobalPosition = _animatedSprite.GlobalPosition;
        ghost.Scale = _animatedSprite.GlobalScale;
        ghost.FlipH = _animatedSprite.FlipH;
        ghost.Rotation = _animatedSprite.Rotation;
        ghost.Modulate = new Color(1, 1, 1, 0.4f);
        GetParent().AddChild(ghost);
        
        var tw = ghost.CreateTween();
        tw.TweenProperty(ghost, "modulate:a", 0f, 0.3f);
        tw.Finished += () => ghost.QueueFree();
    }

    private void PreparePortraitTexture()
    {
        if (_cachedPortraitTexture != null) return;

        var fullTexture = GD.Load<Texture2D>("res://Assets/Sprites/Player/Thach_sanh_hoat_hoa.png");
        if (fullTexture == null) 
        {
            fullTexture = GD.Load<Texture2D>("res://Assets/Sprites/Player/thach_sanh_portrait_shout.png");
            if (fullTexture == null) return;
        }

        Image img = fullTexture.GetImage();
        if (img == null) return;
        img.Decompress();
        img.Convert(Image.Format.Rgba8);

        // TÁCH NỀN CHO AVATAR (Dựa vào màu góc trái trên)
        Color bgColor = img.GetPixel(1, 1); 
        for (int y = 0; y < img.GetHeight(); y++) {
            for (int x = 0; x < img.GetWidth(); x++) {
                Color p = img.GetPixel(x, y);
                float diff = Mathf.Sqrt(Mathf.Pow(p.R - bgColor.R, 2) + Mathf.Pow(p.G - bgColor.G, 2) + Mathf.Pow(p.B - bgColor.B, 2));
                // Avatar thường có nền phức tạp hơn nên dùng ngưỡng thấp hơn chút
                if (diff < 0.4f) 
                    img.SetPixel(x, y, new Color(1, 1, 1, 0));
            }
        }
        
        _cachedPortraitTexture = ImageTexture.CreateFromImage(img);
    }

    // --- SKILL 3: EARTH BREAKER ULTIMATE ---
    private async void ExecuteEarthBreaker()
    {
        if (_isSpinning) return;
        _skill3Timer = Skill3Cooldown;
        _isAttacking = true;

        // 1. Phát âm thanh kích hoạt cực mạnh (Battle Cry & Energy Build-up)
        _sfxPlayer.Stream = SFX.GetUltimateSound();
        _sfxPlayer.VolumeDb = 10f; 
        _sfxPlayer.Play();

        // 2. Chuẩn bị Texture chân dung (Xóa nền)
        PreparePortraitTexture();

        // 3. Hiện Avatar nhân vật cực ngầu bên trái (Cinematic Intro)
        var portrait = new SkillPortraitUI(_cachedPortraitTexture);
        GetTree().Root.AddChild(portrait);

        // 4. Chuẩn bị Rìu Thần khổng lồ
        PrepareAxeTexture();
        var bigAxe = new Sprite2D();
        bigAxe.Texture = _cachedAxeTexture;
        // Nhỏ lại 35% so với kích thước cũ (0.3 * 0.35 = 0.105)
        bigAxe.Scale = new Vector2(0.105f, 0.105f); 
        // Để màu gốc của ảnh bạn thiết kế
        bigAxe.Modulate = new Color(1.1f, 1.1f, 1.1f); 
        bigAxe.ZIndex = 2;
        AddChild(bigAxe);
        
        // 5. NHẢY LÊN LẤY ĐÀ (High Leap)
        _animatedSprite.Play("jump");
        Vector2 startPos = GlobalPosition;
        
        var leapTween = CreateTween().SetParallel(true);
        leapTween.TweenProperty(this, "global_position:y", startPos.Y - 180, 0.4f).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
        
        // Thêm âm thanh nhún nhảy cực mạnh
        var jumpPlayer = new AudioStreamPlayer2D();
        jumpPlayer.Stream = SFX.GetJumpSound();
        jumpPlayer.VolumeDb = 5f;
        AddChild(jumpPlayer);
        jumpPlayer.Play();
        jumpPlayer.Finished += () => jumpPlayer.QueueFree();
        
        bigAxe.Position = new Vector2(-40 * _facingDirection, -80);
        bigAxe.Rotation = -Mathf.Pi / 1.5f * _facingDirection; 
        leapTween.TweenProperty(bigAxe, "position", new Vector2(-20 * _facingDirection, -110), 0.4f);
        leapTween.TweenProperty(bigAxe, "rotation", -Mathf.Pi / 4 * _facingDirection, 0.4f);
        
        await ToSignal(leapTween, "finished");
        
        // 6. FREEZE FRAME
        Engine.TimeScale = 0.15f; 
        await ToSignal(GetTree().CreateTimer(0.12f), "timeout");
        Engine.TimeScale = 1.0f;

        // 7. GIÁNG ĐÒN THIÊN THẠCH
        var slamTween = CreateTween().SetParallel(true);
        slamTween.TweenProperty(this, "global_position:y", startPos.Y, 0.15f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
        slamTween.TweenProperty(bigAxe, "rotation", Mathf.Pi * 1.25f * _facingDirection, 0.13f);
        // Bổ rìu ra phía trước nhân vật
        slamTween.TweenProperty(bigAxe, "position", new Vector2(70 * _facingDirection, 40), 0.13f); 
        
        await ToSignal(slamTween, "finished");

        // 8. VA CHẠM ĐỊA CHẤN (Impact)
        bigAxe.QueueFree();
        
        // Phát âm thanh nện đất ĐỊA CHẤN (Vụ nổ cực mạnh)
        _sfxPlayer.Stream = SFX.GetEarthImpactSound();
        _sfxPlayer.VolumeDb = 12f; // Tăng cực đại uy lực
        _sfxPlayer.Play();

        // Tạo thêm một AudioPlayer phụ để chồng âm thanh va chạm kim loại
        var metalHit = new AudioStreamPlayer2D();
        metalHit.Stream = SFX.GetAttackSound(3);
        metalHit.VolumeDb = 8f;
        AddChild(metalHit);
        metalHit.Play();
        metalHit.Finished += () => metalHit.QueueFree();

        // Rung màn hình mãnh liệt
        var cam = GetViewport().GetCamera2D() as FollowCamera;
        if (cam != null) cam.Shake(1.0f, 45f);
        
        var crack = new GroundCrackVFX();
        // Xuất hiện vệt nứt ở phía trước chân nhân vật 60 pixel
        Vector2 impactPos = GlobalPosition + new Vector2(60 * _facingDirection, 15);
        crack.GlobalPosition = impactPos;
        GetParent().AddChild(crack);

        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (Node2D e in enemies)
        {
            // Tính sát thương dựa trên tâm nổ phía trước nhân vật
            if (impactPos.DistanceTo(e.GlobalPosition) < 350f)
            {
                if (e.HasMethod("TakeDamage"))
                    e.Call("TakeDamage", AttackDamage * 6);
            }
        }

        _isAttacking = false;
        _animatedSprite.Play("idle");
    }
}

// --- PROJECTILE CLASS ---
public partial class AxeProjectile : Area2D
{
    public Vector2 Direction;
    public float Speed = 650f;
    public float Damage;
    public Node2D Target;
    public Texture2D Texture;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 4;
        
        var sprite = new Sprite2D();
        sprite.Texture = Texture;
        // Nhỏ lại phân nửa (0.12 * 0.5 = 0.06)
        // Nhỏ lại phù hợp (nâng lên 0.08 để thấy chi tiết)
        sprite.Scale = new Vector2(0.08f, 0.08f); 
        // Để màu gốc của ảnh
        sprite.Modulate = new Color(1.0f, 1.0f, 1.0f); 
        AddChild(sprite);

        // --- HIỆU ỨNG LỬA THẦN (Cân đối lại siêu nhỏ) ---
        var fire = new CpuParticles2D();
        fire.Amount = 80; // Giảm bớt lượng hạt cho đỡ rối
        fire.Lifetime = 0.4f;
        fire.SpeedScale = 3.0f;
        fire.LocalCoords = false;
        fire.EmissionShape = CpuParticles2D.EmissionShapeEnum.Sphere;
        fire.EmissionSphereRadius = 6f; // Rất nhỏ để ôm sát rìu
        fire.Gravity = new Vector2(0, -50); 
        fire.InitialVelocityMin = 40f;
        fire.InitialVelocityMax = 80f;
        fire.ScaleAmountMin = 2f; 
        fire.ScaleAmountMax = 5f;
        fire.Color = new Color(1, 0.4f, 0, 0.8f);
        AddChild(fire);

        var trails = new CpuParticles2D();
        trails.Amount = 50;
        trails.Lifetime = 0.6f;
        trails.LocalCoords = false;
        trails.Gravity = Vector2.Zero;
        trails.ScaleAmountMin = 3f;
        trails.ScaleAmountMax = 6f;
        trails.Color = new Color(1, 0.8f, 0.2f, 0.5f);
        AddChild(trails);

        var shape = new CollisionShape2D();
        shape.Shape = new CircleShape2D { Radius = 35 };
        AddChild(shape);

        AreaEntered += (area) => {
            if (area.IsInGroup("enemies") || area.GetParent().IsInGroup("enemies")) 
                HandleHit(area);
        };
        BodyEntered += (body) => {
            if (body.IsInGroup("enemies")) 
                HandleHit(body);
        };
        
        GetTree().CreateTimer(3.0f).Timeout += () => QueueFree();
    }

    private void HandleHit(Node node)
    {
        if (node.HasMethod("TakeDamage"))
            node.Call("TakeDamage", (int)Damage);
        else if (node.GetParent().HasMethod("TakeDamage"))
            node.GetParent().Call("TakeDamage", (int)Damage);
            
        QueueFree();
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        
        if (IsInstanceValid(Target))
        {
            Vector2 toTarget = (Target.GlobalPosition - GlobalPosition).Normalized();
            Direction = Direction.Lerp(toTarget, dt * 6f).Normalized();
        }

        // Xoay tít mù tạo cảm giác rìu ném rất mạnh
        Rotation += dt * 35f; 
        Position += Direction * Speed * dt;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("enemies") && body.HasMethod("TakeDamage"))
        {
            body.Call("TakeDamage", (int)Damage);
            QueueFree();
        }
    }
}

// --- SPIN VFX CLASS ---
public partial class SpinVFX : Node2D
{
    public float RotationAngle = 0f;

    public override void _Draw()
    {
        // Màu sắc THÉP SẮT BÉN (Trắng - Xám - Bạc)
        Color coreColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);     // Trắng tinh khiết (Lõi lưỡi rìu)
        Color bladeColor = new Color(0.8f, 0.85f, 0.9f, 0.7f);  // Xanh bạc (Ánh thép)
        Color trailColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);  // Xám mờ (Gió/Bụi)
        
        // Vẽ 4 tầng vệt chém chồng lên nhau
        for (int i = 0; i < 4; i++)
        {
            float radius = 40f + i * 11f;
            float arcLen = 1.8f; 
            
            for (int side = 0; side < 2; side++)
            {
                float offset = side * Mathf.Pi;
                float currentAngle = RotationAngle + offset;
                
                // 1. Vẽ luồng gió (Wind trail) - Xám mờ ảo
                DrawArc(Vector2.Zero, radius, currentAngle, currentAngle + arcLen, 16, trailColor, 14f);
                
                // 2. Vẽ ánh thép (Steel flash) - Bạc
                DrawArc(Vector2.Zero, radius, currentAngle, currentAngle + arcLen * 0.7f, 24, bladeColor, 4f);
                
                // 3. Vẽ LÕI SẮC LẸM (Razor Sharp Core) - Trắng sáng siêu mỏng
                DrawArc(Vector2.Zero, radius, currentAngle + arcLen * 0.5f, currentAngle + arcLen * 0.65f, 32, coreColor, 1.5f);
                
                // 4. Các tia chớp thép nhỏ xẹt ra
                if (i % 2 == 0) {
                    float extraAngle = currentAngle + 0.4f;
                    DrawArc(Vector2.Zero, radius + 4, extraAngle, extraAngle + 0.25f, 8, coreColor, 1.0f);
                }
            }
        }
    }
    public override void _Process(double delta) { QueueRedraw(); }
}

// --- GROUND CRACK VFX ---
public partial class GroundCrackVFX : Node2D
{
    private float _life = 1.0f;
    private List<Vector2> _points = new List<Vector2>();

    public override void _Ready()
    {
        // Tạo các điểm nứt ngẫu nhiên lan tỏa sang 2 bên
        for(int i = 0; i < 20; i++) {
            _points.Add(new Vector2(GD.Randf() * 500 - 250, GD.Randf() * 30));
        }
        
        var tw = CreateTween();
        tw.TweenProperty(this, "_life", 0f, 1.4f).SetTrans(Tween.TransitionType.Sine);
        tw.Finished += () => QueueFree();
    }
    public override void _Draw()
    {
        Color gold = new Color(1.0f, 0.9f, 0.2f, _life);
        Color lava = new Color(0.9f, 0.3f, 0.0f, _life * 0.6f);
        
        foreach(var p in _points) {
            float t = 1.2f - _life;
            // Vẽ vệt nứt lan tỏa
            DrawLine(new Vector2(0, -10), p * t, gold, 8f * _life);
            // Vẽ các đốm lửa bùng lên
            DrawCircle(p * t, 15f * _life, lava);
        }
    }
    public override void _Process(double delta) { QueueRedraw(); }
}

// --- SKILL PORTRAIT UI ---
public partial class SkillPortraitUI : CanvasLayer
{
    private Texture2D _portraitTex;

    public SkillPortraitUI(Texture2D tex)
    {
        _portraitTex = tex;
    }

    public override void _Ready()
    {
        Layer = 1;

        var control = new Control();
        control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(control);

        // HIỆN ẢNH THẠCH SANH HOẠT HỌA (Nguyên bản, không khung)
        if (_portraitTex != null)
        {
            var portrait = new TextureRect();
            portrait.Texture = _portraitTex;
            portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            
            // Kích thước hiển thị vừa vặn hơn
            portrait.Size = new Vector2(320, 420); 
            portrait.Position = new Vector2(-400, 10); // Đưa sát lên mép trên (Y=10)
            portrait.Modulate = new Color(1.1f, 1.1f, 1.1f, 0f); 
            control.AddChild(portrait);

            // Tên chiêu thức
            var label = new Label { 
                Text = "SƠN HÀ THIÊN TỔ!", 
                HorizontalAlignment = HorizontalAlignment.Center
            };
            label.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            label.Modulate = new Color(2.5f, 2.0f, 0.5f, 0); 
            label.Scale = new Vector2(2.5f, 2.5f);
            label.Position = new Vector2(100, 150); // Hạ thấp nhãn xuống theo Avatar (Gần sát mép trên hơn)
            control.AddChild(label);

            // ANIMATION SLIDE-IN
            var tw = CreateTween().SetParallel(true);
            tw.TweenProperty(portrait, "position:x", 30f, 0.6f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tw.TweenProperty(portrait, "modulate:a", 1.0f, 0.4f);
            
            tw.TweenProperty(label, "modulate:a", 1.0f, 0.5f);
            tw.TweenProperty(label, "position:y", 210f, 0.6f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

            // Tự hủy sau màn intro
            var timer = GetTree().CreateTimer(1.8f);
            timer.Timeout += () => {
                var fade = CreateTween();
                fade.TweenProperty(control, "modulate:a", 0f, 0.5f);
                fade.Finished += () => QueueFree();
            };
        }
        else { QueueFree(); }
    }
}
