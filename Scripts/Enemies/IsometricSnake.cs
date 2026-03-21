using Godot;
using System;

public partial class IsometricSnake : CharacterBody2D
{
    [Export] public float Speed = 80.0f;
    [Export] public float DetectRange = 300.0f;
    [Export] public float AttackRange = 50.0f;
    [Export] public int Damage = 15;
    [Export] public int Health = 100;

    private AnimatedSprite2D _animSprite;
    private Node2D _target;
    public bool IsDead = false;
    private bool _canAttack = true;
    private Timer _attackCooldown;

    // Health Bar UI
    [Export] public Vector2 HealthBarOffset = new Vector2(-20, -50);
    private Node2D _healthBarNode;
    private ColorRect _healthBarFill;

    private void PlayAnimSafe(string preferred, string fallback = "walk")
    {
        if (_animSprite?.SpriteFrames == null) return;

        if (_animSprite.SpriteFrames.HasAnimation(preferred))
        {
            _animSprite.Play(preferred);
            return;
        }

        if (!string.IsNullOrEmpty(fallback) && _animSprite.SpriteFrames.HasAnimation(fallback))
            _animSprite.Play(fallback);
    }

    public override void _Ready()
    {
        _animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Build snake animations from source sheets so scene placeholders don't make enemies invisible.
        var rebuiltFrames = SpriteHelper.CreateSnakeSpriteFrames();
        if (rebuiltFrames != null)
            _animSprite.SpriteFrames = rebuiltFrames;

        PlayAnimSafe("walk");

        _attackCooldown = new Timer();
        _attackCooldown.WaitTime = 1.5f;
        _attackCooldown.OneShot = true;
        _attackCooldown.Timeout += () => _canAttack = true;
        AddChild(_attackCooldown);

        AddToGroup("enemies");
        YSortEnabled = true;

        SetupHealthBar();
    }

    private void SetupHealthBar()
    {
        _healthBarNode = new Node2D();
        _healthBarNode.Position = HealthBarOffset;

        // Viền đen
        ColorRect border = new ColorRect();
        border.Color = new Color(0.1f, 0.1f, 0.1f);
        border.Size = new Vector2(40, 8);
        border.MouseFilter = Control.MouseFilterEnum.Ignore;

        // Nền tối
        ColorRect bg = new ColorRect();
        bg.Color = new Color(0.2f, 0.2f, 0.2f);
        bg.Size = new Vector2(38, 6);
        bg.Position = new Vector2(1, 1);
        bg.MouseFilter = Control.MouseFilterEnum.Ignore;

        // Thanh máu xanh
        _healthBarFill = new ColorRect();
        _healthBarFill.Color = new Color("4caf50");
        _healthBarFill.Size = new Vector2(38, 6);
        _healthBarFill.Position = new Vector2(1, 1);
        _healthBarFill.MouseFilter = Control.MouseFilterEnum.Ignore;

        border.AddChild(bg);
        _healthBarNode.AddChild(border);
        _healthBarNode.AddChild(_healthBarFill);
        AddChild(_healthBarNode);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead) return;

        // Simple AI: Find player
        if (_target == null)
        {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0) _target = players[0] as Node2D;
        }

        if (_target != null)
        {
            float dist = GlobalPosition.DistanceTo(_target.GlobalPosition);

            if (dist < DetectRange && dist > AttackRange)
            {
                // Chase
                Vector2 dir = ((_target.GlobalPosition - GlobalPosition).Normalized());
                Velocity = dir * Speed;
                PlayAnimSafe("walk");
                _animSprite.FlipH = dir.X < 0;
            }
            else if (dist <= AttackRange)
            {
                // Attack
                if (dist < 35.0f) // Nếu quá gần (dính người), lùi lại một chút
                {
                    Vector2 repulsionDir = (GlobalPosition - _target.GlobalPosition).Normalized();
                    Velocity = repulsionDir * Speed * 0.6f;
                    PlayAnimSafe("walk");
                }
                else
                {
                    Velocity = Vector2.Zero;
                    if (_canAttack) 
                    {
                        CreateAttackVFX();
                        Attack();
                    }
                }
            }
            else
            {
                // Idle / Patrol
                Velocity = Vector2.Zero;
                PlayAnimSafe("idle", "walk");
            }
        }

        MoveAndSlide();
    }

    private void Attack()
    {
        _canAttack = false;
        PlayAnimSafe("attack", "walk");
        _attackCooldown.Start();

        // Damage target if it's player
        if (_target is IsometricPlayer player)
        {
            player.Call("TakeDamage", Damage);
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        
        // Cập nhật thanh máu
        if (_healthBarFill != null)
        {
            float percent = Math.Max(0f, (float)Health / 100f);
            _healthBarFill.Size = new Vector2(38f * percent, 6f);
        }

        if (Health <= 0) Die();
        else
        {
            PlayAnimSafe("hurt", "walk");
            // Flash red
            Modulate = new Color(1, 0.5f, 0.5f);
            var tw = CreateTween();
            tw.TweenProperty(this, "modulate", Colors.White, 0.3f);
        }
    }

    private void Die()
    {
        // Xóa thanh máu khi chết
        if (_healthBarNode != null)
        {
            _healthBarNode.QueueFree();
            _healthBarNode = null;
        }

        IsDead = true;
        PlayAnimSafe("die", "hurt");
        CollisionLayer = 0;
        CollisionMask = 0;
        GameManager.Instance.OnEnemyDefeated();

        // Hồi máu cho player khi giết quái (10% Max HP)
        var player = GetTree().GetFirstNodeInGroup("player") as Node;
        if (player != null && !player.IsQueuedForDeletion() && player.HasMethod("Heal"))
        {
            int healAmount = (int)(GameManager.Instance.MaxPlayerHealth * 0.10f);
            player.Call("Heal", healAmount);
        }

        var tw = CreateTween();
        tw.TweenProperty(this, "modulate:a", 0f, 1.0f);
        tw.Chain().TweenCallback(Callable.From(() => QueueFree()));
    }

    private void CreateAttackVFX()
    {
        // Container cho hiệu ứng Cắn (Bite) của Rắn Isometric
        var biteNode = new Node2D();
        float faceSign = _animSprite.FlipH ? -1f : 1f;
        
        // Miệng rắn thường nằm ở X=25, Y=-15 (tùy sprite)
        biteNode.Position = new Vector2(faceSign * 30, -15);
        AddChild(biteNode);

        // --- TẠO 4 RĂNG NANH (FANGS) ---
        for (int i = 0; i < 4; i++)
        {
            var fang = new ColorRect();
            fang.Size = new Vector2(6, 12);
            fang.Color = new Color(1, 1, 1); // Màu trắng răng
            
            bool isTop = i < 2;
            float xOff = (i % 2 == 0) ? -8 : 8;
            fang.Position = new Vector2(xOff, isTop ? -20 : 20);
            biteNode.AddChild(fang);

            var tw = CreateTween();
            tw.SetParallel(true);
            float targetY = isTop ? -2 : 2;
            tw.TweenProperty(fang, "position:y", targetY, 0.12f).SetTrans(Tween.TransitionType.Quart);
            tw.Chain().TweenProperty(fang, "color", new Color(0.7f, 0.3f, 1.0f), 0.05f); // Nháy tím độc
            tw.TweenProperty(fang, "modulate:a", 0f, 0.2f).SetDelay(0.1f);
        }

        // --- HIỆU ỨNG NHỎ GIỌT ĐỘC TỐ (POISON DRIP) ---
        var drip = new CpuParticles2D();
        drip.Amount = 10;
        drip.Lifetime = 0.6f;
        drip.Explosiveness = 0.8f;
        drip.Direction = new Vector2(0, 1);
        drip.Spread = 30f;
        drip.Gravity = new Vector2(0, 400);
        drip.InitialVelocityMin = 40f;
        drip.InitialVelocityMax = 80f;
        drip.ScaleAmountMin = 2f;
        drip.ScaleAmountMax = 5f;
        drip.Color = new Color(0.2f, 0.9f, 0.1f, 0.8f); // Xanh lá độc
        biteNode.AddChild(drip);
        drip.Emitting = true;

        // Cleanup
        var cleanup = GetTree().CreateTimer(0.8f);
        cleanup.Timeout += () => { if (IsInstanceValid(biteNode)) biteNode.QueueFree(); };
    }
}
