using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Level 2 Builder: Dark Cave Environment
/// Pro-level environment with occlusion, dark atmosphere, falling rocks, and hidden traps.
/// </summary>
public partial class Level2Builder : Node2D
{
    // Constants & Palette
    private const float MAP_WIDTH = 4000f;
    private const float MAP_HEIGHT = 1200f;
    private static readonly Color DarkCaveColor = new Color("#606068"); // Lighter ambient for "standard" cave feel
    private static readonly Color StonyGray = new Color(0.35f, 0.35f, 0.38f);
    private static readonly Color PathColor = new Color("#5a4a3a"); // Darker trail for contrast against light floor

    // Resources
    private Shader _caveStoneShader = GD.Load<Shader>("res://Assets/Shaders/cave_stone.gdshader");
    private Texture2D _rockTexture = GD.Load<Texture2D>("res://Assets/Sprites/Environment/rock_pixel.png");
    private Texture2D _grassTexture = GD.Load<Texture2D>("res://Assets/Sprites/Environment/grass.png");
    private Texture2D _floorTexture = GD.Load<Texture2D>("res://Assets/Sprites/Backgrounds/cave_ground.png");
    private PackedScene _exitScene = GD.Load<PackedScene>("res://Scenes/Objects/LevelExit.tscn");
    private PackedScene _spikeTrapScene = GD.Load<PackedScene>("res://Scenes/Hazards/SpikeHazard.tscn");
    private PackedScene _snakeScene = GD.Load<PackedScene>("res://Scenes/Enemies/IsometricSnake.tscn");
    private PackedScene _eagleScene = GD.Load<PackedScene>("res://Scenes/Enemies/Eagle.tscn");
    private PackedScene _bossScene = GD.Load<PackedScene>("res://Scenes/Enemies/ChanTinh.tscn");

    private Random _rng = new Random();
    private Texture2D _lightTexture;
    private List<int> _rollingLanes = new List<int>();
    private float[] _laneY = { 200f, 500f, 850f }; // North, Middle, South center Y

    public override void _Ready()
    {
        _rng = new Random();
        _lightTexture = CreateRadialGradient(256); // Generate light in-code to avoid import issues

        // 1. Setup Sorting & Layers
        YSortEnabled = true;
        ZIndex = 0;

        // 2. Build Environment
        BuildAtmosphere();
        BuildFloorAndPath();
        BuildBoundaries();
        BuildObstacles();
        BuildHazards();
        BuildEnemies();
        BuildExitPortal();
        
        // Ensure PlayerLight in the scene also uses the code-generated texture
        var playerLight = GetParent().GetNodeOrNull<PointLight2D>("PlayerLight");
        if (playerLight != null) {
            playerLight.Texture = _lightTexture;
            playerLight.ShadowEnabled = false; // Disable shadows per user's "always bright" request
            playerLight.Energy = 1.3f;
            playerLight.TextureScale = 4.0f;
        }

        // 3. Setup Dynamic Systems
        BuildRollingRockTraps();
    }


    private void BuildAtmosphere()
    {
        // Thêm nền màu tối đồng nhất để không bị lộ các mảng màu khác khi Camera di chuyển
        var bg = new ColorRect();
        bg.Size = new Vector2(MAP_WIDTH + 2000, MAP_HEIGHT + 2000);
        bg.Position = new Vector2(-1000, -1000);
        bg.Color = new Color(0.15f, 0.15f, 0.18f); // Màu đá trầm
        bg.ZIndex = -200; // Dưới cả sàn
        AddChild(bg);
    }

    private void BuildFloorAndPath()
    {
        // Stone Base with Texture
        var floor = new Sprite2D();
        floor.Texture = _floorTexture;
        floor.RegionEnabled = true;
        floor.RegionRect = new Rect2(0, 0, MAP_WIDTH, MAP_HEIGHT);
        floor.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        floor.TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;
        floor.Position = new Vector2(MAP_WIDTH / 2, MAP_HEIGHT / 2);
        floor.ZIndex = -100;

        var mat = new ShaderMaterial();
        mat.Shader = _caveStoneShader;
        floor.Material = mat;
        floor.SelfModulate = new Color(0.8f, 0.8f, 0.9f); // Lighter ground for better visibility
        AddChild(floor);
        
        // Add Slowdown Area for Floor (Speckled Ground)
        var slowArea = new Area2D();
        slowArea.Name = "GroundSlowArea";
        slowArea.CollisionLayer = 0;
        slowArea.CollisionMask = 1; // Player
        
        var slowShape = new CollisionShape2D();
        slowShape.Shape = new RectangleShape2D { Size = floor.RegionRect.Size };
        slowArea.Position = floor.Position;
        slowArea.AddChild(slowShape);
        AddChild(slowArea);

        slowArea.BodyEntered += (body) => {
            if (body is IsometricPlayer player) {
                float currentSpeed = (float)player.Get("Speed");
                player.Set("Speed", currentSpeed * 0.5f);
                // Only tint if NOT on a path
                if (!player.HasMeta("on_path"))
                    player.Modulate = new Color(0.8f, 0.8f, 1.0f);
            }
        };
        slowArea.BodyExited += (body) => {
            if (body is IsometricPlayer player) {
                float currentSpeed = (float)player.Get("Speed");
                player.Set("Speed", currentSpeed * 2.0f);
                player.Modulate = Colors.White;
            }
        };

        // Define 3 Main Branches
        Vector2[][] branches = new Vector2[][] {
            // Branch 1: North
            new Vector2[] { new Vector2(100, 500), new Vector2(400, 200), new Vector2(1500, 150), new Vector2(3000, 200), new Vector2(3850, 500) },
            // Branch 2: Middle
            new Vector2[] { new Vector2(100, 500), new Vector2(1000, 500), new Vector2(2500, 500), new Vector2(3850, 500) },
            // Branch 3: South
            new Vector2[] { new Vector2(100, 500), new Vector2(400, 800), new Vector2(1500, 950), new Vector2(3000, 800), new Vector2(3850, 500) }
        };

        foreach (var points in branches)
        {
            var path = new Line2D();
            path.Points = points;
            path.DefaultColor = PathColor;
            path.Width = 140f;
            path.ZIndex = -95;
            path.BeginCapMode = Line2D.LineCapMode.Round;
            path.EndCapMode = Line2D.LineCapMode.Round;
            path.JointMode = Line2D.LineJointMode.Round;
            AddChild(path);

            // Add Fast Area for the Path
            var fastArea = new Area2D();
            fastArea.CollisionLayer = 0;
            fastArea.CollisionMask = 1; // Player
            
            // Create collision shapes for each segment of the path
            for (int i = 0; i < points.Length - 1; i++)
            {
                var segment = new CollisionShape2D();
                var shape = new CapsuleShape2D();
                Vector2 p1 = points[i];
                Vector2 p2 = points[i+1];
                float dist = p1.DistanceTo(p2);
                shape.Radius = 70f; // Half of path width
                shape.Height = dist + 140f; 
                segment.Shape = shape;
                segment.Position = (p1 + p2) / 2f;
                segment.Rotation = (p2 - p1).Angle() + Mathf.Pi / 2f;
                fastArea.AddChild(segment);
            }
            AddChild(fastArea);

            fastArea.BodyEntered += (body) => {
                if (body is IsometricPlayer player) {
                    int count = player.HasMeta("on_path") ? (int)player.GetMeta("on_path") : 0;
                    count++;
                    player.SetMeta("on_path", count);
                    
                    if (count == 1) {
                        float currentSpeed = (float)player.Get("Speed");
                        player.Set("Speed", currentSpeed * 2.0f); // Fast (back to 1.0)
                        player.Modulate = Colors.White; // Normal color on path
                    }
                }
            };
            fastArea.BodyExited += (body) => {
                if (body is IsometricPlayer player) {
                    int count = player.HasMeta("on_path") ? (int)player.GetMeta("on_path") : 0;
                    count--;
                    if (count <= 0) {
                        player.RemoveMeta("on_path");
                        float currentSpeed = (float)player.Get("Speed");
                        player.Set("Speed", currentSpeed * 0.5f); // Slow again
                        player.Modulate = new Color(0.8f, 0.8f, 1.0f);
                    } else {
                        player.SetMeta("on_path", count);
                    }
                }
            };
        }
    }

    public void CreateLightAt(Vector2 position)
    {
        // Path lighting is now disabled for a darker "Fog of War" atmosphere
    }

    private void BuildBoundaries()
    {
        // Simple Top/Bottom invisible walls (Layer 2 for CharacterBody2D)
        CreateStaticWall(new Vector2(MAP_WIDTH / 2, -20), new Vector2(MAP_WIDTH, 100)); // Top
        CreateStaticWall(new Vector2(MAP_WIDTH / 2, 1040), new Vector2(MAP_WIDTH, 100)); // Bottom (Moved up from 1220)
        
        // Horizontal Start/End (Layer 2)
        CreateStaticWall(new Vector2(-50, MAP_HEIGHT / 2), new Vector2(100, MAP_HEIGHT)); // Left
        CreateStaticWall(new Vector2(MAP_WIDTH + 50, MAP_HEIGHT / 2), new Vector2(100, MAP_HEIGHT)); // Right
    }

    private void CreateStaticWall(Vector2 pos, Vector2 size)
    {
        var wall = new StaticBody2D { Position = pos };
        wall.CollisionLayer = 2; // Matches Player's Mask 6
        var shape = new CollisionShape2D();
        var rect = new RectangleShape2D { Size = size };
        shape.Shape = rect;
        wall.AddChild(shape);
        AddChild(wall);
    }

    private void BuildObstacles()
    {
        // Solid rock walls (Y=320, Y=680) to split the paths
        // We use Triple Rows to ensure a thick, impassable barrier
        for (int i = 0; i < 150; i++) 
        {
            float x = i * 28 + (float)_rng.NextDouble() * 10;
            // Dừng tạo đá ở cửa hang (3850) để không bị "lòi" ra ngoài
            if (x < 450 || x > 3750) continue; 
            
            // Upper wall Divider (y=310 to 330)
            CreateOccludingWall(new Vector2(x, 310 + (float)_rng.NextDouble() * 20));
            // Lower wall Divider (y=670 to 690)
            CreateOccludingWall(new Vector2(x, 670 + (float)_rng.NextDouble() * 20));

            // Bottom Boundary Wall (Y=1000) to match the other lanes width
            CreateOccludingWall(new Vector2(x, 1000 + (float)_rng.NextDouble() * 20));
        }

        for (int i = 0; i < 80; i++)
        {
            float x = i * 50;
            if (x > 3850) continue; // Dừng tạo đá ở cửa hang
            CreateOccludingWall(new Vector2(x, -20));
        }
    }

    private void CreateOccludingWall(Vector2 pos)
    {
        // To fix Y-sorting issues where the player is hidden by rocks,
        // we must ensure walls/rocks are siblings of the Player node under the root Y-sort enabled parent.
        var wallContainer = new Node2D { Position = pos, YSortEnabled = true };
        var sprite = new Sprite2D();
        sprite.Texture = _rockTexture;
        
        // RESTORED SCALE: Returning to original larger sizes (0.3 to 0.6)
        float s = 0.3f + (float)_rng.NextDouble() * 0.3f; 
        sprite.Scale = new Vector2(s, s);
        sprite.SelfModulate = new Color(0.7f, 0.7f, 0.75f); // High visibility light grey
        sprite.Offset = new Vector2(0, -sprite.Texture.GetSize().Y / 4); 
        wallContainer.AddChild(sprite);

        // PERFECTED SEE-THROUGH LOGIC: Larger detector and aggressive fade
        var detector = new Area2D();
        detector.CollisionLayer = 0;
        detector.CollisionMask = 1; // Player
        var dShape = new CollisionShape2D();
        float texW = sprite.Texture.GetSize().X;
        float texH = sprite.Texture.GetSize().Y;
        // Make the detector cover the whole upper half and overlap enough with player
        var dRect = new RectangleShape2D { Size = new Vector2(texW * s * 1.0f, texH * s * 1.1f) };
        dShape.Shape = dRect;
        dShape.Position = new Vector2(0, -texH * s * 0.45f);
        detector.AddChild(dShape);
        wallContainer.AddChild(detector);

        detector.BodyEntered += (body) => {
            if (body.IsInGroup("player") || body is Player || body is IsometricPlayer) {
                var tw = wallContainer.CreateTween();
                tw.TweenProperty(sprite, "modulate:a", 0.3f, 0.25f);
            }
        };
        detector.BodyExited += (body) => {
            if (body.IsInGroup("player") || body is Player || body is IsometricPlayer) {
                var tw = wallContainer.CreateTween();
                tw.TweenProperty(sprite, "modulate:a", 1.0f, 0.25f);
            }
        };

        // Solid Body with Collision Shape
        var blocker = new StaticBody2D();
        blocker.CollisionLayer = 2; // Matches Player's Mask 6 (Binary 110)
        blocker.CollisionMask = 0; 

        var bShape = new CollisionShape2D();
        // Collision at the base of the rock
        var bRect = new RectangleShape2D { Size = new Vector2(50 * s, 30 * s) }; 
        bShape.Shape = bRect;
        blocker.AddChild(bShape);
        wallContainer.AddChild(blocker);
        
        AddChild(wallContainer);
    }

    private void BuildHazards()
    {
        // 1. Random Scattered Hazards
        for (int i = 0; i < 20; i++)
        {
            Vector2 pos = new Vector2(
                (float)_rng.NextDouble() * (MAP_WIDTH - 600) + 300,
                (float)_rng.NextDouble() * (MAP_HEIGHT - 200) + 100
            );

            SpawnSpikeTrap(pos, 0.5f); // Still mostly hidden
        }

        // 2. Strategic Hazards on Branches
        Vector2[][] branches = GetBranches();
        foreach (var points in branches)
        {
            for (int i = 1; i < points.Length - 1; i++)
            {
                // Place a trap between every two path nodes
                Vector2 mid = (points[i] + points[i+1]) / 2f;
                SpawnSpikeTrap(mid, 0.9f); // Much more visible on path
            }
        }
    }

    private void SpawnSpikeTrap(Vector2 pos, float alpha)
    {
        if (_spikeTrapScene == null) return;
        var trap = _spikeTrapScene.Instantiate<Node2D>();
        trap.Position = pos;
        trap.Modulate = new Color(0.7f, 0.6f, 0.6f, alpha);
        // Increase damage for Level 2
        trap.Set("Damage", 35); 
        // Add to local node
        AddChild(trap);
    }

    private void BuildEnemies()
    {
        if (_snakeScene == null || _eagleScene == null) return;

        // Sinh lính canh cố định cho Màn 2: Tổng cộng 15 con
        for (int i = 0; i < 9; i++)
        {
            Vector2 pos = new Vector2(_rng.Next(400, (int)MAP_WIDTH - 400), _rng.Next(150, (int)MAP_HEIGHT - 150));
            var snake = _snakeScene.Instantiate<CharacterBody2D>();
            snake.Position = pos;
            AddChild(snake);
        }

        for (int i = 0; i < 6; i++)
        {
            Vector2 pos = new Vector2(_rng.Next(600, (int)MAP_WIDTH - 600), _rng.Next(100, (int)MAP_HEIGHT - 300));
            var eagle = _eagleScene.Instantiate<CharacterBody2D>();
            eagle.Position = pos;
            AddChild(eagle);
        }
        
        GD.Print($"[Level2Builder] Spawned exactly 15 enemies (9 snakes, 6 eagles).");
    }

    private Vector2[][] GetBranches()
    {
        return new Vector2[][] {
            // Branch 1: North
            new Vector2[] { new Vector2(100, 500), new Vector2(400, 200), new Vector2(1500, 150), new Vector2(3000, 200), new Vector2(3850, 500) },
            // Branch 2: Middle
            new Vector2[] { new Vector2(100, 500), new Vector2(1000, 500), new Vector2(2500, 500), new Vector2(3850, 500) },
            // Branch 3: South
            new Vector2[] { new Vector2(100, 500), new Vector2(400, 800), new Vector2(1500, 950), new Vector2(3000, 800), new Vector2(3850, 500) }
        };
    }

    private void BuildRollingRockTraps()
    {
        // NO RANDOM: Enable rolling rocks in ALL 3 lanes (0, 1, 2)
        _rollingLanes.Clear();
        _rollingLanes.Add(0);
        _rollingLanes.Add(1);
        _rollingLanes.Add(2);

        // Place triggers along the paths (Layer 2 for CharacterBody2D)
        foreach (int laneIdx in _rollingLanes)
        {
            float laneY = _laneY[laneIdx];
            for (int x = 600; x < MAP_WIDTH - 600; x += 800)
            {
                CreateRollingRockTrigger(new Vector2(x, laneY), laneY);
            }
        }
    }

    private void CreateRollingRockTrigger(Vector2 pos, float laneY)
    {
        var trigger = new Area2D { Position = pos };
        trigger.CollisionLayer = 0;
        trigger.CollisionMask = 1; // Player

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(100, 150) };
        trigger.AddChild(shape);
        AddChild(trigger);

        trigger.BodyEntered += (body) => {
            if (body.IsInGroup("player"))
            {
                // Spawn rock slightly ahead of the trigger
                SpawnRollingRockAt(new Vector2(pos.X + 700, laneY), pos.X - 500);
            }
        };

        // Note: Repeatable as BodyEntered fires each time the player enters.
    }

    private void SpawnRollingRockAt(Vector2 spawnPos, float targetX)
    {
        var rock = new Node2D { Position = spawnPos };
        AddChild(rock);

        var sprite = new Sprite2D();
        sprite.Texture = _rockTexture;
        sprite.Scale = new Vector2(0.6f, 0.6f);
        sprite.SelfModulate = new Color(0.9f, 0.7f, 0.7f); // Slightly distinct
        rock.AddChild(sprite);

        // Movement & Rolling Animation
        var tween = CreateTween().SetParallel(true);
        float duration = 3.0f;

        tween.TweenProperty(rock, "position:x", targetX, duration);
        tween.TweenProperty(sprite, "rotation_degrees", -720, duration);

        // High Damage Detection (75 damage = 50% of 150 HP)
        var area = new Area2D();
        area.CollisionLayer = 0;
        area.CollisionMask = 1; // Player
        var shape = new CollisionShape2D();
        shape.Shape = new CircleShape2D { Radius = 30f };
        area.AddChild(shape);
        rock.AddChild(area);

        area.BodyEntered += (body) => {
            if (body.HasMethod("TakeDamage"))
            {
                body.Call("TakeDamage", 75); // 50% HP damage
                rock.QueueFree();
            }
        };

        tween.Chain().Finished += () => rock.QueueFree();
    }

    private void BuildExitPortal()
    {
        if (_exitScene == null) return;
        var exit = _exitScene.Instantiate<LevelExit>();
        exit.Position = new Vector2(3850, 500);
        exit.Scale = new Vector2(2.0f, 2.0f); // Phóng to gấp 3 lần để vừa 2 bên đá
        exit.Name = "LevelExit";
        AddChild(exit);

        // 🏆 Rương báu vật cuối màn (Chỉ xuất hiện khi diệt sạch quái - Giống Màn 1)
        var chestScene = GD.Load<PackedScene>("res://Scenes/NPCs/TreasureChest.tscn");
        if (chestScene != null)
        {
            var chest = chestScene.Instantiate<TreasureChest>();
            chest.RequireAllEnemiesDefeated = true;
            chest.GlobalPosition = new Vector2(3650, 520); // Di chuyển về chỗ người chơi đứng
            
            // Thêm vào levelBuilder để Y-sort
            var levelBuilderNode = GetNodeOrNull<Node2D>("LevelBuilder");
            if (levelBuilderNode != null)
                levelBuilderNode.AddChild(chest);
            else
                AddChild(chest);

            // ÉP BUỘC rương phải ẩn đi ngay lập tức sau khi thêm vào cây (Để tránh lỗi đồng bộ _Ready)
            chest.Visible = false;

            GD.Print($"[Level2Builder] Spawned fixed reward chest at {chest.GlobalPosition} (Forced Invisible)");
        }

        // Chặn hướng di chuyển ở cuối map (Phễu dẫn vào Exit) - Chừa khoảng trống ở giữa (Y=500)
        // for (int i = 0; i < 6; i++)
        // {
        //     // Upper funnel wall (Kết thúc ở Y=400)
        //     CreateOccludingWall(new Vector2(3850, 300 + i * 20));
        //     // Lower funnel wall (Bắt đầu từ Y=600)
        //     CreateOccludingWall(new Vector2(3850, 700 - i * 20));
        // }

        var light = new PointLight2D();
        light.Texture = _lightTexture;
        light.TextureScale = 6.0f;
        light.Color = new Color(0.5f, 0.7f, 1.0f);
        light.Energy = 2.0f;
        light.Position = new Vector2(3850, 500);
        AddChild(light);
    }

    public override void _Process(double delta)
    {
        // Make PlayerLight follow the player
        var players = GetTree().GetNodesInGroup("player");
        if (players.Count > 0 && players[0] is Node2D player)
        {
            var pLight = GetParent().GetNodeOrNull<PointLight2D>("PlayerLight");
            if (pLight != null)
            {
                pLight.GlobalPosition = player.GlobalPosition;
            }
        }
    }

    private Texture2D CreateRadialGradient(int size)
    {
        var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        Vector2 center = new Vector2(size / 2, size / 2);
        float radius = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = new Vector2(x, y).DistanceTo(center);
                float alpha = Mathf.Clamp(1.0f - (dist / radius), 0, 1);
                img.SetPixel(x, y, new Color(1, 1, 1, alpha * alpha));
            }
        }
        return ImageTexture.CreateFromImage(img);
    }
}
