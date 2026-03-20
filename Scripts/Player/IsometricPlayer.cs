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

        // 1. Horizontal Movement (8 hướng)
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (inputDir != Vector2.Zero && !_isAttacking)
        {
            Velocity = Velocity.MoveToward(inputDir * Speed, Acceleration * dt);
            _facingDirection = inputDir;
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * dt);
        }

        MoveAndSlide();

        // 2. Fake Z-Axis (Jump)
        HandleJump(dt);

        // 3. Attack
        HandleAttack();

        if (_comboActive)
        {
            _comboTimer += dt;
            if (_comboTimer >= ComboResetTime)
            {
                _comboActive = false;
                _comboIndex = 0;
                _comboTimer = 0f;
            }
        }

        // 4. Update Visuals
        UpdateAnimation(inputDir);
        UpdateVisualOffset();
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
        var slashNode = new Node2D();
        float faceX = _facingDirection.X;
        float faceY = _facingDirection.Y;

        // Hướng tấn công dựa trên facingDirection
        float angle = Mathf.Atan2(faceY, faceX);
        slashNode.Position = new Vector2(faceX * 30, faceY * 30 - _z);
        slashNode.Rotation = angle;
        AddChild(slashNode);

        // Tạo 1 vệt chém duy nhất (không dùng 3 đường móng vuốt)
        var slash = new ColorRect();
        slash.Size = new Vector2(10, 52);
        slash.PivotOffset = new Vector2(5f, 26f);
        slash.Position = new Vector2(0, -26);
        slash.Rotation = -0.15f;
        slash.Color = new Color(1f, 0.9f, 0.3f, 0.95f);
        slashNode.AddChild(slash);

        var tw = CreateTween();
        tw.SetParallel(true);
        slash.Scale = new Vector2(0.15f, 0.15f);
        tw.TweenProperty(slash, "scale", new Vector2(1.35f, 1.5f), 0.12f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        tw.TweenProperty(slash, "rotation", 0.2f, 0.16f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tw.TweenProperty(slash, "modulate:a", 0f, 0.22f).SetDelay(0.08f);

        // Spark particles
        var sparks = new CpuParticles2D();
        sparks.Emitting = true;
        sparks.OneShot = true;
        sparks.Amount = 12;
        sparks.Lifetime = 0.3f;
        sparks.Explosiveness = 0.95f;
        sparks.Direction = new Vector2(faceX, faceY);
        sparks.Spread = 35f;
        sparks.Gravity = Vector2.Zero;
        sparks.InitialVelocityMin = 100f;
        sparks.InitialVelocityMax = 200f;
        sparks.ScaleAmountMin = 2f;
        sparks.ScaleAmountMax = 4f;
        sparks.Color = new Color(1f, 0.8f, 0.2f, 0.8f);
        slashNode.AddChild(sparks);

        // Cleanup
        var cleanup = GetTree().CreateTimer(0.5f);
        cleanup.Timeout += () => { if (IsInstanceValid(slashNode)) slashNode.QueueFree(); };
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
    public void RefreshSkillUI()
    {
        // IsometricPlayer currently has no dedicated skill panel UI.
    }
}
