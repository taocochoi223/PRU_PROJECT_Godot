using Godot;

public partial class BossEnemy : BaseEnemy
{
    [Export] public PackedScene KeyScene;

    private Timer _contactDamageTimer;
    private bool _canDealContactDamage = true;

    public override void _Ready()
    {
        // Stats của Đại Boss
        MaxHealth = 300;
        AttackDamage = 25;
        MoveSpeed = 0; // Đứng yên tuyệt đối
        ScoreValue = 2000;
        DetectRange = 500.0f;
        AttackRange = 180.0f; // Tốt nhất cho Boss to
        PatrolDistance = 0;
        PatrolDirection = -1;

        HealthBarOffset = new Vector2(-20, -450);

        base._Ready();
        
        Scale = new Vector2(1.5f, 1.5f);

        if (_healthBarNode != null)
        {
            _healthBarNode.Scale = new Vector2(2.0f, 2.0f);
            (_healthBarNode as Node2D).ZIndex = 4005;
        }

        SetFacingDirection(true);

        ZIndex = 4000;
        if (AnimSprite != null) {
            AnimSprite.ZIndex = 4001;
            AnimSprite.Visible = true;
            AnimSprite.FlipH = false; // Phải để false vì SpriteHelper đã chuẩn hóa về hướng Trái
        }

        // Timer cho sát thương va chạm (2s)
        _contactDamageTimer = new Timer();
        _contactDamageTimer.WaitTime = 2.0f;
        _contactDamageTimer.OneShot = false;
        _contactDamageTimer.Timeout += () => _canDealContactDamage = true;
        AddChild(_contactDamageTimer);
        _contactDamageTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead) return;

        // 1. Kháng đẩy: Luôn ép Velocity.X = 0 và không cho di chuyển
        Vector2 velocity = Velocity;
        velocity.X = 0;
        
        if (!IsOnFloor())
            velocity.Y += Gravity * (float)delta;
        
        Velocity = velocity;
        MoveAndSlide();

        // 2. Ép Animation Idle khi không đánh/đau
        if (CurrentState == EnemyState.Patrol || CurrentState == EnemyState.Chase)
        {
            AnimSprite.Play("idle");
            AnimSprite.FlipH = false; // Khóa ngay sau Play
        }

        // 3. Tự động tấn công khi Player ở gần (Thêm buffer để tránh jitter)
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player != null && !player.IsQueuedForDeletion())
        {
            float dist = GlobalPosition.DistanceTo(player.GlobalPosition);
            
            // Nếu đang Idle, chỉ chuyển sang Attack nếu vào sát phạm vi
            if (CurrentState != EnemyState.Attack && dist <= AttackRange)
            {
                if (CanAttackPlayer && CurrentState != EnemyState.Hurt)
                {
                    CurrentState = EnemyState.Attack;
                    AnimSprite.FlipH = false; // Khóa TRƯỚC khi Play để không có frame lật nào
                    AnimSprite.Play("attack");
                    AnimSprite.FlipH = false; // Khóa NGAY SAU khi Play để chắc chắn
                }
            }
            // Nếu đang Attack, chỉ quay lại Idle nếu ra xa hẳn (AttackRange + 30)
            else if (CurrentState == EnemyState.Attack && dist > AttackRange + 30.0f)
            {
                CurrentState = EnemyState.Patrol;
                AnimSprite.FlipH = false;
                AnimSprite.Play("idle");
                AnimSprite.FlipH = false;
            }

            // 4. Sát thương va chạm (Contact Damage) - 2s một lần
            if (_canDealContactDamage && dist < 100.0f)
            {
                player.TakeDamage(AttackDamage);
                _canDealContactDamage = false;
                GD.Print("[BossEnemy] Sát thương va chạm! Player bị mất máu.");
            }
        }

        // 5. ĐỒNG BỘ HƯỚNG MẶT CỐ ĐỊNH (Triệt để)
        // Vì toàn bộ animation (Idle, Attack, Die) đã được chuẩn hóa về hướng Trái 
        // trong SpriteHelper, chúng ta LUÔN giữ FlipH = false.
        if (AnimSprite != null)
        {
            AnimSprite.FlipH = false;
        }
    }

    protected override void OnAnimationFinished()
    {
        if (AnimSprite.Animation == "attack")
        {
            CurrentState = EnemyState.Patrol;
            CanAttackPlayer = false;
            AttackCooldownTimer.Start();
        }
    }

    protected override void Die()
    {
        base.Die();
        SpawnKey();
    }

    private void SpawnKey()
    {
        GD.Print("Đại Boss gục ngã! Lối thoát đã mở.");
        
        // Kích hoạt cổng thoát (nếu có trong scene)
        var exit = GetTree().GetFirstNodeInGroup("LevelExit") as LevelExit;
        if (exit != null)
        {
            exit.Activate();
        }

        // Load key scene nếu chưa gán
        if (KeyScene == null)
        {
            KeyScene = GD.Load<PackedScene>("res://Scenes/Items/BossKey.tscn");
        }

        if (KeyScene != null)
        {
            var key = KeyScene.Instantiate<Node2D>();
            key.GlobalPosition = GlobalPosition;
            GetParent().AddChild(key);
            
            // Hiệu ứng nảy chìa khóa
            var tween = key.CreateTween();
            tween.TweenProperty(key, "position:y", key.Position.Y - 50, 0.5f).SetTrans(Tween.TransitionType.Back);
        }
    }

    protected override void CreatePlaceholderSprites()
    {
        // Sử dụng FinalBoss.jpg với thiết lập 4 dòng (Dòng 0: Idle, 2: Attack, 3: Die)
        AnimSprite.SpriteFrames = SpriteHelper.CreateFinalBossSpriteFrames();
        
        if (AnimSprite.SpriteFrames == null)
        {
            GD.PrintErr("Không thể tạo SpriteFrames cho FinalBoss, dùng fallback.");
            base.CreatePlaceholderSprites();
            return;
        }

        AnimSprite.Play("idle");

        // Điều chỉnh Offset để Boss to đứng đúng trên mặt đất
        // Khung hình 600x600, đưa tâm lên và bù trừ để chân chạm đất
        AnimSprite.Offset = new Vector2(0, -280.0f);
        AnimSprite.Visible = true;
        
        GD.Print("[BossEnemy] Đã chuyển sang dùng FinalBoss.jpg thành công.");
    }

    protected override void SetFacingDirection(bool faceLeft)
    {
        // Vô hiệu hóa để không bị BaseEnemy điều khiển
    }

    public override void _Process(double delta)
    {
        // Khóa hướng mặt ở mức _Process để đảm bảo ổn định tuyệt đối là hướng Trái
        if (AnimSprite != null)
        {
            AnimSprite.FlipH = false;
        }
    }
}
