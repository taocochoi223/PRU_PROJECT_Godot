using Godot;
using System;

public partial class IsometricPlayer : CharacterBody2D
{
    [Export] public float Speed = 250.0f;
    [Export] public float Acceleration = 1500.0f;
    [Export] public float Friction = 1200.0f;

    // Fake Z-axis (Jump Logic)
    [Export] public float JumpForce = 350.0f;
    [Export] public float GravityForce = 1200.0f;

    // Combat
    [Export] public int MaxHealth = 150;
    [Export] public int AttackDamage = 30;
    [Export] public float AttackCooldown = 0.3f;
    [Export] public float ComboResetTime = 0.6f;

    public float Z => _z;
    private float _z = 0f;
    private float _vz = 0f;
    private bool _isJumping = false;
    private bool _hasDoubleJumped = false;
    private bool _isFalling = false;
    private bool _isDead = false;
    private int _health;
    private bool _canAttack = true;
    private bool _isAttacking = false;
    private bool _isInvulnerable = false;

    private AnimatedSprite2D _animatedSprite;
    private Node2D _shadow;
    private Vector2 _facingDirection = Vector2.Down;
    private Timer _attackCooldownTimer;
    private Timer _invulnTimer;
    private AudioStreamPlayer2D _attackSfxPlayer;
    private Vector2 _baseSpriteScale = Vector2.One;
    private int _comboIndex = 0;
    private float _comboTimer = 0f;
    private bool _comboActive = false;
    private string _currentAttackAnimation = "attack";

    // Skills
    private float _skill1Timer = 0f;
    private float _skill2Timer = 0f;
    private float _skill3Timer = 0f;
    private const float Skill1Cooldown = 0f;  
    private const float Skill2Cooldown = 0f; 
    private const float Skill3Cooldown = 0f; 

    private CanvasLayer _skillPanelLayer;
    private TextureRect[] _skillIcons = new TextureRect[3];
    private TextureRect[] _cooldownOverlays = new TextureRect[3];
    private Label[] _cooldownLabels = new Label[3];
    private static Rect2I _unifiedSkillBounds = new Rect2I();
    private static bool _skillBoundsCalculated = false;
    private bool _isSpinning = false;
    private static Texture2D _cachedAxeTexture = null;
    private static Texture2D _cachedPortraitTexture = null;

    private Node2D _lastFadedObject = null;
    private bool _isAutoWalking = false;
    private Vector2 _autoWalkTarget;
    private bool _isEnteringCave = false;

    [Signal] public delegate void HealthChangedEventHandler(int newHealth, int maxHealth);
    [Signal] public delegate void PlayerDiedEventHandler();

    public override void _Ready()
    {
        _health = MaxHealth;
        SyncHealthToGameManager();

        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _shadow = GetNodeOrNull<Node2D>("Shadow");
        _baseSpriteScale = _animatedSprite.Scale;

        // Rebuild canonical frames from project assets to avoid empty/placeholder scene frames.
        var rebuiltFrames = SpriteHelper.CreatePlayerSpriteFrames();
        if (rebuiltFrames != null)
        {
            _animatedSprite.SpriteFrames = rebuiltFrames;
            if (_animatedSprite.SpriteFrames.HasAnimation("idle"))
                _animatedSprite.Play("idle");
        }

        YSortEnabled = true;

        if (_shadow == null && HasNode("Shadow"))
            _shadow = GetNode<Node2D>("Shadow");

        AddToGroup("player");

        var cam = GetNodeOrNull<Camera2D>("Camera2D");
        if (cam != null)
        {
            cam.MakeCurrent();
            cam.AddToGroup("MainCamera");
        }

        // Dedicated player for slash SFX so attack audio works even without scene audio nodes.
        _attackSfxPlayer = new AudioStreamPlayer2D();
        _attackSfxPlayer.Name = "AttackSFX";
        _attackSfxPlayer.Bus = "Master";
        _attackSfxPlayer.VolumeDb = -4f;
        AddChild(_attackSfxPlayer);

        // Attack cooldown
        _attackCooldownTimer = new Timer();
        _attackCooldownTimer.WaitTime = AttackCooldown;
        _attackCooldownTimer.OneShot = true;
        _attackCooldownTimer.Timeout += () => { _canAttack = true; _isAttacking = false; };
        AddChild(_attackCooldownTimer);

        // Invulnerability timer
        _invulnTimer = new Timer();
        _invulnTimer.WaitTime = 1.0f;
        _invulnTimer.OneShot = true;
        _invulnTimer.Timeout += () => { _isInvulnerable = false; Modulate = Colors.White; };
        AddChild(_invulnTimer);

        EmitSignal(SignalName.HealthChanged, _health, MaxHealth);
        
        // TIỀN XỬ LÝ TEXTURE (Tránh bị đơ/lag khi dùng chiêu lần đầu)
        PrepareAxeTexture();
        string[] iconPaths = { "res://Assets/Sprites/Player/Skill_1.png", "res://Assets/Sprites/Player/Skill_2.png", "res://Assets/Sprites/Player/Skill_3.png" };
        for (int i = 0; i < 3; i++) GetCleanSkillIcon(iconPaths, i);
    }

    private void SyncHealthToGameManager()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.MaxPlayerHealth = MaxHealth;
        GameManager.Instance.PlayerHealth = Mathf.Clamp(_health, 0, MaxHealth);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDead || _isFalling) return;

        float dt = (float)delta;

        // 1. Skill UI Initialization
        if (GameManager.Instance.UnlockedSkillsCount > 0 && _skillPanelLayer == null)
        {
            SetupSkillUI();
        }

        // 2. Physics & Movement
        HandleGravity(dt);

        if (_isAutoWalking)
        {
            HandleAutoWalkMovement(dt);
            UpdateAnimation(Velocity.Normalized());
            UpdateVisualOffset();
            MoveAndSlide();
            return;
        }

        HandleMovement(dt);
        HandleJump(dt);
        MoveAndSlide();

        // 3. Attack & Skills 
        HandleAttack();
        HandleSkills(dt);

        // 4. Update Visuals
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        UpdateAnimation(inputDir);
        UpdateVisualOffset();
        UpdateSkillUICooldowns();
    }

    private void HandleGravity(float dt)
    {
        // Trọng lực giả cho trục Z được xử lý trong HandleJump
    }

    private void HandleMovement(float dt)
    {
        if (_isAttacking || _isSpinning) return;

        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (inputDir != Vector2.Zero)
        {
            Velocity = Velocity.MoveToward(inputDir * Speed, Acceleration * dt);
            _facingDirection = inputDir;
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * dt);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  COMBAT — Tấn công chém bằng rìu
    // ═══════════════════════════════════════════════════════════
    private void HandleAttack()
    {
        if (Input.IsActionJustPressed("attack") && _canAttack && !_isJumping)
        {
            _isAttacking = true;
            _canAttack = false;
            _attackCooldownTimer.Start();

            int comboNumber = _comboIndex + 1;
            string comboAnim = $"attack{comboNumber}";
            _currentAttackAnimation = comboAnim;
            if (_animatedSprite?.SpriteFrames != null)
            {
                if (_animatedSprite.SpriteFrames.HasAnimation(comboAnim))
                    _animatedSprite.Play(comboAnim);
                else if (_animatedSprite.SpriteFrames.HasAnimation("attack"))
                {
                    _currentAttackAnimation = "attack";
                    _animatedSprite.Play("attack");
                }
            }

            if (_attackSfxPlayer != null)
            {
                _attackSfxPlayer.Stream = SFX.GetAttackSound(comboNumber);
                _attackSfxPlayer.VolumeDb = comboNumber == 3 ? 2f : -4f;
                _attackSfxPlayer.PitchScale = (float)GD.RandRange(0.95, 1.08);
                _attackSfxPlayer.Play();
            }

            CreateAttackVFX();
            DealDamageToNearbyEnemies();

            _comboIndex = (_comboIndex + 1) % 3;
            _comboActive = true;
            _comboTimer = 0f;
        }
    }

    private void DealDamageToNearbyEnemies()
    {
        float attackRange = 50f;
        var enemies = GetTree().GetNodesInGroup("enemies");

        foreach (var enemy in enemies)
        {
            if (enemy is Node2D enemyNode && IsInstanceValid(enemyNode))
            {
                float dist = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
                if (dist <= attackRange)
                {
                    // Kiểm tra hướng tấn công
                    Vector2 toEnemy = (enemyNode.GlobalPosition - GlobalPosition).Normalized();
                    float dot = _facingDirection.Dot(toEnemy);

                    if (dot > -0.3f) // Trong vùng tấn công phía trước
                    {
                        if (enemyNode.HasMethod("TakeDamage"))
                            enemyNode.Call("TakeDamage", AttackDamage);
                    }
                }
            }
        }
    }

    private void CreateAttackVFX()
    {
        // Hiệu ứng đánh thường của nhân vật đã được loại bỏ theo yêu cầu.
        // Bạn có thể thêm lại logic ở đây nếu muốn.
    }

    // ═══════════════════════════════════════════════════════════
    //  HEALTH — Nhận sát thương & Chết
    // ═══════════════════════════════════════════════════════════
    public void TakeDamage(int damage)
    {
        if (_isDead || _isFalling || _isInvulnerable) return;

        _health -= damage;
        SyncHealthToGameManager();
        _isInvulnerable = true;
        _invulnTimer.Start();

        EmitSignal(SignalName.HealthChanged, _health, MaxHealth);

        // Nhấp nháy đỏ
        Modulate = new Color(1, 0.3f, 0.3f);
        var tw = CreateTween();
        tw.TweenProperty(this, "modulate", new Color(1, 1, 1, 0.7f), 0.15f);
        tw.TweenProperty(this, "modulate", Colors.White, 0.15f);

        // Đẩy lùi
        Vector2 knockback = -_facingDirection.Normalized() * 80;
        Velocity = knockback;

        GD.Print($"Player took {damage} damage! HP: {_health}/{MaxHealth}");

        if (_health <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        _health = Math.Min(_health + amount, MaxHealth);
        SyncHealthToGameManager();
        EmitSignal(SignalName.HealthChanged, _health, MaxHealth);
    }

    private void Die()
    {
        _isDead = true;
        Velocity = Vector2.Zero;

        // Death animation
        var tw = CreateTween();
        tw.SetParallel(true);
        tw.TweenProperty(_animatedSprite, "modulate", new Color(0.5f, 0, 0, 0.5f), 0.8f);
        tw.TweenProperty(_animatedSprite, "rotation", Mathf.Pi * 0.5f, 0.6f);

        tw.Chain().TweenCallback(Callable.From(() =>
        {
            EmitSignal(SignalName.PlayerDied);
        }));
    }

    public void StartInvulnerability(float duration)
    {
        _isInvulnerable = true;
        _invulnTimer.WaitTime = duration;
        _invulnTimer.Start();
    }

    // ═══════════════════════════════════════════════════════════
    //  PIT — Rơi xuống hố
    // ═══════════════════════════════════════════════════════════
    public void FallIntoPit()
    {
        if (_z > 5.0f || _isFalling) return;

        _isFalling = true;
        Velocity = Vector2.Zero;

        var tw = CreateTween();
        tw.SetParallel(true);
        tw.TweenProperty(_animatedSprite, "scale", Vector2.Zero, 0.5f);
        tw.TweenProperty(_animatedSprite, "rotation", Mathf.Pi * 2, 0.5f);
        tw.TweenProperty(_animatedSprite, "modulate:a", 0f, 0.5f);

        tw.Chain().TweenCallback(Callable.From(() =>
        {
            GlobalPosition = new Vector2(200, 600);
            ResetState();
            TakeDamage(25); // Mất 25 HP khi rơi hố
        }));
    }

    private void ResetState()
    {
        _isFalling = false;
        _animatedSprite.Scale = _baseSpriteScale;
        _animatedSprite.Rotation = 0;
        _animatedSprite.Modulate = Colors.White;
        Modulate = Colors.White;
        _z = 0;
        _vz = 0;
        _isJumping = false;
    }

    public void FastReset()
    {
        _isDead = false;
        _health = MaxHealth;
        SyncHealthToGameManager();
        ResetState();
        EmitSignal(SignalName.HealthChanged, _health, MaxHealth);
    }

    // ═══════════════════════════════════════════════════════════
    //  JUMP — Nhảy giả (Fake Z)
    // ═══════════════════════════════════════════════════════════
    private void HandleJump(float dt)
    {
        if (Input.IsActionJustPressed("jump"))
        {
            if (!_isJumping)
            {
                _vz = JumpForce;
                _isJumping = true;
                _hasDoubleJumped = false;
            }
            else if (!_hasDoubleJumped)
            {
                _vz = JumpForce * 1.25f; // Nhảy đúp cao hơn
                _hasDoubleJumped = true;
            }
        }

        if (_isJumping)
        {
            _z += _vz * dt;
            _vz -= GravityForce * dt;

            if (_z <= 0)
            {
                _z = 0;
                _vz = 0;
                _isJumping = false;
                _hasDoubleJumped = false;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  VISUALS — Animation & Offset
    // ═══════════════════════════════════════════════════════════
    private void UpdateVisualOffset()
    {
        if (_animatedSprite != null)
            _animatedSprite.Offset = new Vector2(0, -_z - 16);

        if (_shadow != null)
        {
            float shadowScale = Mathf.Clamp(1.0f - (_z / 200.0f), 0.4f, 1.0f);
            _shadow.Scale = new Vector2(shadowScale, shadowScale);
            _shadow.Modulate = new Color(1, 1, 1, shadowScale * 0.5f);
        }
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (_isAutoWalking)
        {
            if (_animatedSprite != null && _animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation("run"))
            {
                _animatedSprite.Play("run");
                return;
            }
        }

        if (_isAttacking)
        {
            if (_animatedSprite?.SpriteFrames != null)
            {
                if (_animatedSprite.SpriteFrames.HasAnimation(_currentAttackAnimation))
                {
                    if (_animatedSprite.Animation != _currentAttackAnimation)
                        _animatedSprite.Play(_currentAttackAnimation);
                }
                else if (_animatedSprite.SpriteFrames.HasAnimation("attack") && _animatedSprite.Animation != "attack")
                {
                    _animatedSprite.Play("attack");
                }
            }
            return;
        }

        bool isMoving = direction.Length() > 0.1f;
        string anim = isMoving ? "run" : "idle";

        if (_isJumping) anim = "jump";

        if (_animatedSprite != null && _animatedSprite.SpriteFrames.HasAnimation(anim))
            _animatedSprite.Play(anim);

        if (Mathf.Abs(direction.X) > 0.01f && _animatedSprite != null)
        {
            _animatedSprite.FlipH = direction.X < 0;
        }
    }

    // Compatibility hook used by reward systems that also support the side-scroller Player class.
    // ═══════════════════════════════════════════════════════════
    //  SKILLS — Các chiêu thức võ công
    // ═══════════════════════════════════════════════════════════
    private void HandleSkills(float dt)
    {
        if (_skillPanelLayer == null) SetupSkillUI();

        if (_skill1Timer > 0) _skill1Timer -= dt;
        if (_skill2Timer > 0) _skill2Timer -= dt;
        if (_skill3Timer > 0) _skill3Timer -= dt;

        UpdateSkillUICooldowns();

        if (_isDead || _isFalling || _isAttacking) return;

        bool press1 = Input.IsActionJustPressed("skill1") || Input.IsKeyPressed(Key.Key1);
        bool press2 = Input.IsActionJustPressed("skill2") || Input.IsKeyPressed(Key.Key2);
        bool press3 = Input.IsActionJustPressed("skill3") || Input.IsKeyPressed(Key.Key3);
        int unlocked = GameManager.Instance.UnlockedSkillsCount;

        if (press1 && _skill1Timer <= 0 && unlocked >= 1) ExecuteAxeThrow();
        else if (press2 && _skill2Timer <= 0 && unlocked >= 2) ExecuteWhirlwind();
        else if (press3 && _skill3Timer <= 0 && unlocked >= 3) ExecuteEarthBreaker();
    }

    private void SetupSkillUI()
    {
        _skillPanelLayer = new CanvasLayer();
        _skillPanelLayer.Layer = 5;
        AddChild(_skillPanelLayer);

        var margin = new MarginContainer();
        margin.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        margin.AddThemeConstantOverride("margin_right", 100);
        margin.AddThemeConstantOverride("margin_top", -40);
        _skillPanelLayer.AddChild(margin);

        var hbox = new HBoxContainer();
        hbox.Alignment = BoxContainer.AlignmentMode.End;
        hbox.AddThemeConstantOverride("separation", 15);
        margin.AddChild(hbox);

        string[] paths = { "res://Assets/Sprites/Player/Skill_1.png", "res://Assets/Sprites/Player/Skill_2.png", "res://Assets/Sprites/Player/Skill_3.png" };
        for (int i = 0; i < 3; i++)
        {
            var btnHolder = new CenterContainer();
            btnHolder.CustomMinimumSize = new Vector2(160, 160);
            hbox.AddChild(btnHolder);

            // Xử lý tách nền xanh rác cho từng Icon
            var processedTex = GetCleanSkillIcon(paths, i);

            _skillIcons[i] = new TextureRect { Texture = processedTex, CustomMinimumSize = new Vector2(160, 160), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered };
            btnHolder.AddChild(_skillIcons[i]);

            _cooldownOverlays[i] = new TextureRect { Texture = processedTex, CustomMinimumSize = new Vector2(160, 160), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, Modulate = new Color(0, 0, 0, 0.75f), Visible = false };
            btnHolder.AddChild(_cooldownOverlays[i]);

            _cooldownLabels[i] = new Label { CustomMinimumSize = new Vector2(160, 160), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Visible = false };
            _cooldownLabels[i].AddThemeFontSizeOverride("font_size", 56);
            btnHolder.AddChild(_cooldownLabels[i]);

            btnHolder.Visible = (i < GameManager.Instance.UnlockedSkillsCount);
        }
    }

    public void RefreshSkillUI()
    {
        if (_skillPanelLayer == null) return;
        for (int i = 0; i < 3; i++)
        {
            if (_skillIcons[i] != null && _skillIcons[i].GetParent() is Control holder)
                holder.Visible = (i < GameManager.Instance.UnlockedSkillsCount);
        }
    }

    private void UpdateSkillUICooldowns()
    {
        if (_skillPanelLayer == null) return;
        float[] timers = { _skill1Timer, _skill2Timer, _skill3Timer };
        for (int i = 0; i < 3; i++)
        {
            if (timers[i] > 0)
            {
                _cooldownOverlays[i].Visible = true;
                _cooldownLabels[i].Visible = true;
                _cooldownLabels[i].Text = Mathf.CeilToInt(timers[i]).ToString();
            }
            else
            {
                _cooldownOverlays[i].Visible = false;
                _cooldownLabels[i].Visible = false;
            }
        }
    }

    private async void ExecuteAxeThrow()
    {
        _skill1Timer = Skill1Cooldown;
        _isAttacking = true;
        
        PrepareAxeTexture();
        var axe = new AxeProjectile();
        axe.Texture = _cachedAxeTexture;
        axe.GlobalPosition = GlobalPosition + new Vector2(_facingDirection.X * 40, _facingDirection.Y * 40 - _z - 20);
        axe.Direction = _facingDirection.Normalized();
        axe.Damage = AttackDamage * 1.8f;

        // TỰ ĐỘNG TÌM MỤC TIÊU GẦN NHẤT
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D nearest = null;
        float minDist = 650f; // Tầm bắn tự tìm 650px
        foreach (var e in enemies)
        {
            if (e is Node2D enemyNode && IsInstanceValid(enemyNode))
            {
                float d = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
                if (d < minDist) { minDist = d; nearest = enemyNode; }
            }
        }
        axe.Target = nearest;

        GetParent().AddChild(axe);
        _animatedSprite.Play("attack1");
        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        _isAttacking = false;
    }

    private async void ExecuteWhirlwind()
    {
        _skill2Timer = Skill2Cooldown;
        _isSpinning = true;
        float duration = 3.0f;
        float elapsed = 0f;
        
        var spinVFX = new SpinVFX();
        spinVFX.Position = new Vector2(0, -30);
        AddChild(spinVFX);

        while (elapsed < duration && IsInstanceValid(this) && !_isDead)
        {
            float dt = 0.04f;
            elapsed += dt;
            spinVFX.RotationAngle = elapsed * 25.0f;
            _animatedSprite.FlipH = Mathf.Cos(elapsed * 25.0f) < 0;
            _animatedSprite.Play("attack1");
            
            // Deal damage
            var enemies = GetTree().GetNodesInGroup("enemies");
            foreach (Node2D e in enemies)
            {
                if (GlobalPosition.DistanceTo(e.GlobalPosition) < 130f)
                    if (e.HasMethod("TakeDamage")) e.Call("TakeDamage", (int)(AttackDamage * 0.4f));
            }
            await ToSignal(GetTree().CreateTimer(dt), "timeout");
        }
        _isSpinning = false;
        if (IsInstanceValid(spinVFX)) spinVFX.QueueFree();
    }

    private async void ExecuteEarthBreaker()
    {
        _skill3Timer = Skill3Cooldown;
        _isAttacking = true;
        _animatedSprite.Play("jump");
        
        Vector2 startPos = GlobalPosition;
        var tw = CreateTween();
        tw.TweenProperty(this, "global_position:y", startPos.Y - 150, 0.4f);
        await ToSignal(tw, "finished");
        
        var slam = CreateTween();
        slam.TweenProperty(this, "global_position:y", startPos.Y, 0.15f);
        await ToSignal(slam, "finished");

        // Impact
        var crack = new GroundCrackVFX();
        crack.GlobalPosition = GlobalPosition;
        GetParent().AddChild(crack);

        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (Node2D e in enemies)
        {
            if (GlobalPosition.DistanceTo(e.GlobalPosition) < 300f)
                if (e.HasMethod("TakeDamage")) e.Call("TakeDamage", AttackDamage * 5);
        }
        _isAttacking = false;
    }

    public async void AutoWalkToCave(Vector2 targetPos)
    {
        _isAutoWalking = true;
        _autoWalkTarget = targetPos;
        _isEnteringCave = true;
        SetCollisionMaskValue(2, false); // Mở va chạm môi trường để không bị kẹt đá
        GD.Print("Thạch Sanh: Tự động đi vào hang...");
    }

    private void HandleAutoWalkMovement(float dt)
    {
        Vector2 dir = (_autoWalkTarget - GlobalPosition).Normalized();
        Velocity = Velocity.Lerp(dir * (Speed * 0.7f), dt * 5f);
        _facingDirection = dir;

        float dist = GlobalPosition.DistanceTo(_autoWalkTarget);
        if (dist < 200f && _isEnteringCave)
        {
            // Bắt đầu mờ dần muộn hơn, chỉ khi đã vào hẳn trong cửa hang (Yêu cầu dist < 200)
            float alpha = Mathf.Clamp((dist - 50f) / 150f, 0, 1);
            Modulate = new Color(1, 1, 1, alpha);
        }

        if (dist < 50f)
        {
            _isAutoWalking = false;
            Velocity = Vector2.Zero;
            GD.Print("Thạch Sanh: Đã vào hang sâu, chuyển màn!");
            // Gọi chuyển màn
            var parent = GetParent();
            if (parent != null && parent.HasMethod("ChangeLevel"))
            {
                parent.Call("ChangeLevel");
            }
        }
    }

    private void PrepareAxeTexture()
    {
        if (_cachedAxeTexture != null) return;
        var fullTexture = GD.Load<Texture2D>("res://Assets/Sprites/Player/Riu_Skill.png");
        if (fullTexture == null)
        {
            fullTexture = GD.Load<Texture2D>("res://icon.svg");
            _cachedAxeTexture = fullTexture;
            return;
        }

        Image img = fullTexture.GetImage();
        if (img == null) { _cachedAxeTexture = fullTexture; return; }
        img.Decompress();
        img.Convert(Image.Format.Rgba8);

        // THUẬT TOÁN TÁCH NỀN THÔNG MINH CHO VŨ KHÍ
        Color bgColor = img.GetPixel(0, 0); 
        for (int y = 0; y < img.GetHeight(); y++)
        {
            for (int x = 0; x < img.GetWidth(); x++)
            {
                Color p = img.GetPixel(x, y);
                // Lọc màu xanh lá (Chroma Key) và màu nền góc
                bool isGreen = p.G > 0.4f && p.G > p.R * 1.1f && p.G > p.B * 1.1f;
                float diff = Mathf.Sqrt(Mathf.Pow(p.R - bgColor.R, 2) + Mathf.Pow(p.G - bgColor.G, 2) + Mathf.Pow(p.B - bgColor.B, 2));
                
                if (isGreen || diff < 0.35f)
                    img.SetPixel(x, y, new Color(1, 1, 1, 0));
            }
        }
        _cachedAxeTexture = ImageTexture.CreateFromImage(img);
    }

    private Texture2D GetCleanSkillIcon(string[] paths, int index)
    {
        var tex = GD.Load<Texture2D>(paths[index]);
        if (tex == null) return GD.Load<Texture2D>("res://icon.svg");

        Image img = tex.GetImage();
        if (img == null) return tex;
        img.Decompress();
        img.Convert(Image.Format.Rgba8);

        Color bgColor = img.GetPixel(0, 0); // Lấy màu góc làm nền
        for (int y = 0; y < img.GetHeight(); y++)
        {
            for (int x = 0; x < img.GetWidth(); x++)
            {
                Color p = img.GetPixel(x, y);
                // Thuật toán tách nền xanh (Chroma Key) và màu nền tĩnh
                bool isGreen = p.G > 0.4f && p.G > p.R * 1.1f && p.G > p.B * 1.1f;
                float diffToBg = Mathf.Sqrt(Mathf.Pow(p.R - bgColor.R, 2) + Mathf.Pow(p.G - bgColor.G, 2) + Mathf.Pow(p.B - bgColor.B, 2));

                if (isGreen || diffToBg < 0.15f)
                {
                    img.SetPixel(x, y, new Color(1, 1, 1, 0)); // Làm trong suốt hoàn toàn
                }
            }
        }
        return ImageTexture.CreateFromImage(img);
    }
}
