using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class LevelManager : Node2D
{
    [Export] public int LevelNumber = 1;
    [Export] public PackedScene PlayerScene;

    private Node2D _spawnPoint;
    private HUD _hud;
    private Node2D _player;
    private TutorialManager _tutorialManager;

    // --- Level 3 specific references ---
    private Node2D _princess;
    private Node2D _cage;
    private Node2D _boss;
    private bool _level3EnemiesCleared = false;
    private bool _level3BossSpawned = false;
    private bool _bossFightStarted = false;
    private bool _level3BossCleanupDone = false;
    private Vector2 _bossArenaEntrance = new Vector2(2350, 580);
    private StaticBody2D _princessBarrier;
    private TreasureChest _level1RewardChest;

    // --- Level 3 atmosphere effects ---
    private List<ColorRect> _fireflies = new List<ColorRect>();
    private List<ColorRect> _fogLayers = new List<ColorRect>();
    private ColorRect _lightningFlash;
    private float _lightningTimer = 0f;
    private float _nextLightningTime = 5f;
    private double _elapsedTime = 0;

    private List<Vector2> _checkpoints = new List<Vector2>();

    public override void _Ready()
    {
        GameManager.Instance.CurrentLevel = LevelNumber;

        // Giữ phần reset skill của bạn
        if (LevelNumber == 1)
        {
            GameManager.Instance.UnlockedSkillsCount = 1;
            GD.Print("Level 1: Unlock Skill 1 by default");
        }
        else if (LevelNumber >= 2)
        {
            GameManager.Instance.UnlockedSkillsCount = 3;
            GD.Print($"Level {LevelNumber}: Unlock all skills for testing (Full Skills)");
        }

        if (HasNode("SpawnPoint"))
            _spawnPoint = GetNode<Node2D>("SpawnPoint");

        CollectCheckpoints();
        SpawnPlayer();
        ConnectPlayerSignals();
        CallDeferred(nameof(PlayLevelStartSequence));
        CallDeferred(nameof(InitializeEnemyCount));

        if (LevelNumber == 3)
        {
            SetupLevel3();
            SetupLevel3Atmosphere();
        }

        // Tự động biến các cục đá trang trí ở giữa đường thành chướng ngại vật (vật cản)
        if (LevelNumber != 1) // Bỏ qua ở Level 1 Isometric vì chúng ta dùng hệ thống tường riêng
            MakeRocksSolidObstacles();
    }

    private void MakeRocksSolidObstacles()
    {
        foreach (Node child in GetChildren())
        {
            if (child is Sprite2D sprite && sprite.Name.ToString().StartsWith("Rock_D"))
            {
                // Cho phép Y-sorting tự động xử lý thay vì ép Z-index cố định
                sprite.YSortEnabled = true;
                sprite.ZIndex = 0; // Đưa về 0 để ngang hàng với Player

                // Tạo vật lý cản đường (Environment layer = 2)
                var staticBody = new StaticBody2D();
                staticBody.CollisionLayer = 2;

                var collisionShape = new CollisionShape2D();
                var circleShape = new CircleShape2D();
                // Bán kính hình tròn cản (rock texture lớn, scale 0.3 -> effective radius ~ 35-40px)
                circleShape.Radius = 140f;
                collisionShape.Shape = circleShape;

                // Tâm của đá hơi nhích xuống dưới một chút để Player có thể nhảy lên hoặc đụng cạnh chặn lại
                collisionShape.Position = new Vector2(0, 30f);

                staticBody.AddChild(collisionShape);
                sprite.AddChild(staticBody);

                // Ưu tiên đưa vào LevelBuilder để cùng context Y-Sort với các vật thể khác
                var builderNode = GetNodeOrNull("LevelBuilder");
                if (builderNode != null)
                {
                    CallDeferred(nameof(MoveToBuilder), sprite, builderNode);
                }

                // --- HIỆU ỨNG NHÌN XUYÊN (SEE-THROUGH) ---
                var detector = new Area2D();
                detector.CollisionLayer = 0;
                detector.CollisionMask = 1; // Player
                
                var dShape = new CollisionShape2D();
                // Tăng kích thước detector để đảm bảo luôn thấy nhân vật khi nấp sau đá
                var dRect = new RectangleShape2D { Size = new Vector2(350, 280) }; 
                dShape.Shape = dRect;
                dShape.Position = new Vector2(0, -120);
                detector.AddChild(dShape);
                sprite.AddChild(detector);

                detector.BodyEntered += (body) => {
                    if (body.IsInGroup("player")) {
                        var tw = sprite.CreateTween();
                        tw.TweenProperty(sprite, "modulate:a", 0.3f, 0.25f);
                    }
                };
                detector.BodyExited += (body) => {
                    if (body.IsInGroup("player")) {
                        var tw = sprite.CreateTween();
                        tw.TweenProperty(sprite, "modulate:a", 1.0f, 0.25f);
                    }
                };
            }
        }
    }

    private void CollectCheckpoints()
    {
        _checkpoints.Clear();
        if (_spawnPoint != null) _checkpoints.Add(_spawnPoint.GlobalPosition);
        foreach (var child in GetChildren())
        {
            if (child is Marker2D marker && child.Name.ToString().StartsWith("Checkpoint"))
            {
                _checkpoints.Add(marker.GlobalPosition);
            }
        }
        _checkpoints.Sort((a, b) => a.X.CompareTo(b.X));
    }

    private void SpawnPlayer()
    {
        int checkpointIndex = GameManager.Instance.CurrentCheckpointIndex;

        Vector2 spawnPos = _checkpoints.Count > checkpointIndex
            ? _checkpoints[checkpointIndex]
            : (_spawnPoint?.GlobalPosition ?? Vector2.Zero);

        if (PlayerScene != null)
        {
            _player = PlayerScene.Instantiate<Node2D>();
            _player.GlobalPosition = spawnPos;
            _player.AddToGroup("player");
            
            // Ưu tiên cho Player vào LevelBuilder để Y-Sort hoạt động thống nhất
            var builder = GetNodeOrNull("LevelBuilder");
            if (builder != null) builder.AddChild(_player);
            else AddChild(_player);

            if (LevelNumber >= 2)
            {
                _player.Call("RefreshSkillUI");
            }
        }
    }

    private void MoveToBuilder(Node child, Node builder)
    {
        if (child == null || !IsInstanceValid(child) || builder == null || !IsInstanceValid(builder)) return;
        if (child.GetParent() != null) child.GetParent().RemoveChild(child);
        builder.AddChild(child);
    }

    private void ConnectPlayerSignals()
    {
        if (_player != null)
        {
            if (_player.HasSignal("PlayerDied"))
                _player.Connect("PlayerDied", Callable.From(OnPlayerDied));
        }
    }

    private async void PlayLevelStartSequence()
    {
        if (_player == null || !IsInstanceValid(_player)) return;

        var dm = new DialogueManager();
        AddChild(dm);
        var lines = new List<DialogueManager.DialogueLine>();

        if (LevelNumber == 1)
        {
            lines.Add(new DialogueManager.DialogueLine("Thạch Sanh", "Chằn Tinh đã bắt công chúa vào hang tối rồi. Ta phải cứu người…. Rìu thần đã ở trên tay — đây chính là xứ mệnh của ta, phải đi thôi!", null, "res://Assets/Audio/Voices/ts_m1_intro.mp3"));
        }
        else if (LevelNumber == 2)
        {
            lines.Add(new DialogueManager.DialogueLine("Thạch Sanh", "Cả rắn lẫn đại bàng , chúng bố trí cả trên cao lẫn dưới thấp. Lính canh của Chằn Tinh thật không đơn giản mà… .", null, "res://Assets/Audio/Voices/ts_m2_intro1.mp3"));
            lines.Add(new DialogueManager.DialogueLine("Thạch Sanh", "Hãy chịu thua đi. Điều duy nhất các ngươi có thể làm lúc này là đưa ta đến nơi Chằn Tinh đang ở.", null, "res://Assets/Audio/Voices/ts_demand_boss.mp3"));
        }
        else if (LevelNumber == 3)
        {
            lines.Add(new DialogueManager.DialogueLine("Công Chúa", "Ai đó, cứu tôi, Ai ở đó không?", null, "res://Assets/Audio/Voices/princess_help.mp3"));
            lines.Add(new DialogueManager.DialogueLine("Thạch Sanh", "Tụ lại hết đi! Một cơn lốc là đủ quét sạch bọn ngươi rồi! .", null, "res://Assets/Audio/Voices/ts_m2_tactics3.mp3"));
        }

        if (lines.Count > 0)
        {
            await dm.PlayDialogue(lines, pauseGame: false);
        }

        if (LevelNumber == 1 && GameManager.Instance != null && !GameManager.Instance.HasCompletedOnboardingTutorial)
        {
            if (_player.GetType().Name == "Player")
            {
                _tutorialManager = new TutorialManager();
                AddChild(_tutorialManager);
                await _tutorialManager.RunTutorial((Player)_player);
            }
        }
    }

    public void ActivateCheckpoint(int index)
    {
        if (index > GameManager.Instance.CurrentCheckpointIndex && index < _checkpoints.Count)
        {
            GameManager.Instance.CurrentCheckpointIndex = index;
            GD.Print($"Đã lưu Checkpoint: {index}");
        }
    }

    private void SetupLevel3()
    {
        _princess = GetNodeOrNull<Node2D>("Princess");
        _cage = GetNodeOrNull<Node2D>("BossCage");
        _boss = GetNodeOrNull<Node2D>("ChanTinh");

        if (_princess != null) _princess.Visible = false;
        if (_cage != null) _cage.Visible = false;
        if (_boss != null)
        {
            _boss.Visible = false;
            _boss.ProcessMode = ProcessModeEnum.Disabled;
            _boss.GlobalPosition = new Vector2(5000, 520);
        }
    }

    private void SetupLevel3Atmosphere()
    {
        foreach (var child in GetChildren())
        {
            if (child is ColorRect cr)
            {
                string name = cr.Name.ToString();
                if (name.StartsWith("Firefly"))
                    _fireflies.Add(cr);
                else if (name.StartsWith("Fog_"))
                    _fogLayers.Add(cr);
            }
        }

        _lightningFlash = new ColorRect();
        _lightningFlash.ZIndex = 50;
        _lightningFlash.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _lightningFlash.Color = new Color(1, 1, 1, 0);
        _lightningFlash.MouseFilter = Control.MouseFilterEnum.Ignore;
        _lightningFlash.Size = new Vector2(5000, 1000);
        _lightningFlash.Position = new Vector2(-500, -200);
        AddChild(_lightningFlash);

        _nextLightningTime = (float)GD.RandRange(4.0, 10.0);

        foreach (var fly in _fireflies)
        {
            AnimateFirefly(fly);
        }
    }

    private void AnimateFirefly(ColorRect fly)
    {
        var tw = CreateTween();
        tw.SetLoops();

        float baseY = fly.Position.Y;
        float duration = (float)GD.RandRange(1.5, 3.5);
        float amplitude = (float)GD.RandRange(8.0, 25.0);
        float xDrift = (float)GD.RandRange(-15.0, 15.0);

        tw.TweenProperty(fly, "position", new Vector2(fly.Position.X + xDrift, baseY - amplitude), duration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tw.TweenProperty(fly, "modulate:a", 0.3f, duration * 0.5f).SetTrans(Tween.TransitionType.Sine);
        tw.TweenProperty(fly, "position", new Vector2(fly.Position.X - xDrift, baseY + amplitude * 0.5f), duration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tw.TweenProperty(fly, "modulate:a", 1.0f, duration * 0.5f).SetTrans(Tween.TransitionType.Sine);
    }

    private void TriggerLightning()
    {
        if (_lightningFlash == null) return;
        var tw = CreateTween();
        tw.TweenProperty(_lightningFlash, "color:a", 0.15f, 0.05f);
        tw.TweenProperty(_lightningFlash, "color:a", 0.0f, 0.08f);
        tw.TweenInterval(0.1f);
        tw.TweenProperty(_lightningFlash, "color:a", 0.25f, 0.03f);
        tw.TweenProperty(_lightningFlash, "color:a", 0.0f, 0.15f);
    }

    private void ProcessLevel3Logic()
    {
        if (!_level3EnemiesCleared)
        {
            var enemies = GetTree().GetNodesInGroup("enemies");
            bool anySmallEnemies = false;
            foreach (Node item in enemies)
            {
                if (item != _boss && IsInstanceValid(item) && !item.IsQueuedForDeletion())
                {
                    anySmallEnemies = true;
                    break;
                }
            }

            if (!anySmallEnemies)
            {
                _level3EnemiesCleared = true;
                if (_princess != null) _princess.Visible = true;
                if (_cage != null) _cage.Visible = true;
            }
        }
        else if (!_level3BossSpawned)
        {
            if (_player != null && _princess != null)
            {
                float dist = _player.GlobalPosition.DistanceTo(_princess.GlobalPosition);
                if (dist < 400f)
                {
                    if (_boss != null && !_level3BossSpawned)
                    {
                        _level3BossSpawned = true;
                        _bossFightStarted = true;
                        _boss.GlobalPosition = new Vector2(3300, 520);
                        _boss.Visible = true;
                        _boss.ProcessMode = ProcessModeEnum.Inherit;
                        LockBossArena();
                        CreatePrincessBarrier();
                        CallDeferred(nameof(PlayBossIntroDialogue));
                    }
                }
            }
        }

        if (_bossFightStarted && (_boss == null || !IsInstanceValid(_boss) || (_boss is BaseEnemy enemy && enemy.IsDead)))
        {
            if (_princessBarrier != null && IsInstanceValid(_princessBarrier))
            {
                _princessBarrier.QueueFree();
                _princessBarrier = null;
                var cam = GetTree().GetFirstNodeInGroup("MainCamera") as FollowCamera;
                if (cam != null) cam.LimitRight = 5000;
            }

            if (!_level3BossCleanupDone)
            {
                _level3BossCleanupDone = true;
                CleanupPostBossEnemies();
                if (_princess is Princess princess) princess.RequireAllEnemiesDefeated = false;
            }
        }
    }

    private void CleanupPostBossEnemies()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (var node in enemies)
        {
            if (node == _boss) continue;
            if (node is BaseEnemy enemyNode && !enemyNode.IsDead && IsInstanceValid(enemyNode)) enemyNode.QueueFree();
        }
    }

    private void CreatePrincessBarrier()
    {
        _princessBarrier = new StaticBody2D();
        _princessBarrier.Position = new Vector2(3320, 400);
        _princessBarrier.CollisionLayer = 2;
        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(40, 1000) };
        _princessBarrier.AddChild(shape);
        AddChild(_princessBarrier);
    }

    private void LockBossArena()
    {
        var cam = GetTree().GetFirstNodeInGroup("MainCamera") as FollowCamera;
        if (cam != null)
        {
            cam.LimitLeft = 2350;
            cam.LimitRight = 3502;
        }
        var wall = new StaticBody2D();
        wall.Position = new Vector2(2350, 400);
        wall.CollisionLayer = 2;
        wall.CollisionMask = 1;
        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(60, 1000) };
        wall.AddChild(shape);
        AddChild(wall);
    }

    private async void PlayBossIntroDialogue()
    {
        var dm = new DialogueManager();
        AddChild(dm);
        var lines = new List<DialogueManager.DialogueLine>
        {
            new DialogueManager.DialogueLine("Công Chúa", "Thạch Sanh, hãy cẩn thận, con Chằn Tinh này rất mạnh!", null, "res://Assets/Audio/Voices/princess_warn.mp3"),
            new DialogueManager.DialogueLine("Chằn Tinh", "THẠCH SANH!!! Ngươi thật sự đến được tận đây?! Ta phải thừa nhận, ngươi đã hạ được tất cả lính canh của ta. Nhưng đây là sào huyệt của ta, ngươi nghĩ sẽ thoát được sao hahaha!", null, "res://Assets/Audio/Voices/chantinh_intro.mp3"),
            new DialogueManager.DialogueLine("Thạch Sanh", "Ta đã bước vào đây để cứu người, thì cũng sẵn sàng kết thúc mọi hiểm họa tại đây.", null, "res://Assets/Audio/Voices/ts_boss_phase3.mp3")
        };
        await dm.PlayDialogue(lines);
    }

    public void FastRespawnPlayer()
    {
        if (_player != null && IsInstanceValid(_player))
        {
            Vector2 spawnPos;
            if (LevelNumber == 3 && _bossFightStarted)
            {
                spawnPos = new Vector2(2450, 580);
                if (_boss != null && IsInstanceValid(_boss) && _boss is ChanTinh chantinh) chantinh.ResetBoss(new Vector2(3300, 520));
            }
            else
            {
                int checkpointIndex = GameManager.Instance.CurrentCheckpointIndex;
                spawnPos = _checkpoints.Count > checkpointIndex ? _checkpoints[checkpointIndex] : (_spawnPoint?.GlobalPosition ?? Vector2.Zero);
            }
            _player.GlobalPosition = spawnPos;
            _player.Call("FastReset");
            if (_player.HasMethod("StartInvulnerability")) _player.Call("StartInvulnerability", 1.0f);
        }
        else SpawnPlayer();
    }

    private void OnPlayerDied()
    {
        Engine.TimeScale = 1.0f;
        var timer = GetTree().CreateTimer(1.2, true, false, true);
        timer.Timeout += () => { if (IsInstanceValid(this)) GameManager.Instance.GameOver(); };
    }

    public override void _Process(double delta)
    {
        if (_player == null || _player.IsQueuedForDeletion()) return;
        if (LevelNumber == 3)
        {
            ProcessLevel3Logic();
            _elapsedTime += delta;
            _lightningTimer += (float)delta;
            if (_lightningTimer >= _nextLightningTime)
            {
                _lightningTimer = 0;
                _nextLightningTime = (float)GD.RandRange(5.0, 15.0);
                TriggerLightning();
            }
            foreach (var fog in _fogLayers)
            {
                if (!IsInstanceValid(fog)) continue;
                float shift = Mathf.Sin((float)_elapsedTime * 0.3f + fog.Position.X * 0.01f) * 0.15f;
                var c = fog.Color;
                fog.Color = new Color(c.R, c.G, c.B, Mathf.Clamp(c.A + shift * (float)delta, 0.05f, 0.4f));
            }
        }
        for (int i = GameManager.Instance.CurrentCheckpointIndex + 1; i < _checkpoints.Count; i++)
        {
            if (Mathf.Abs(_player.GlobalPosition.X - _checkpoints[i].X) < 60f)
            {
                ActivateCheckpoint(i);
                break;
            }
        }
    }

    private void InitializeEnemyCount()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        int count = 0;
        foreach (Node node in enemies)
        {
            if (node is BaseEnemy enemy && !enemy.IsDead) count++;
            else if (node is IsometricSnake snake && !snake.IsDead) count++;
        }
        GameManager.Instance.ResetEnemyCount(count);
    }
}