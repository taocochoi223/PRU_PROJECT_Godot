using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Tự động tạo nội dung cho Level 1 Isometric: Rừng Thiêng Đa Cổ Thụ
/// Bao gồm: 5 khu vực với cảnh quan, bẫy, quái vật, checkpoint
/// </summary>
public partial class Level1Builder : Node2D
{
    // ═══════════════════════════════════════════════════════════
    //  COLOR PALETTE — Rừng Thiêng Huyền Bí
    // ═══════════════════════════════════════════════════════════
    private static readonly Color ForestDark = new Color(0.06f, 0.12f, 0.06f);
    private static readonly Color ForestGround = new Color(0.14f, 0.22f, 0.10f);
    private static readonly Color ForestGrass = new Color(0.18f, 0.32f, 0.12f);
    private static readonly Color PathColor = new Color(0.28f, 0.22f, 0.14f);
    private static readonly Color PathEdge = new Color(0.20f, 0.16f, 0.10f);
    private static readonly Color WaterDeep = new Color(0.08f, 0.20f, 0.35f, 0.85f);
    private static readonly Color WaterShallow = new Color(0.15f, 0.35f, 0.50f, 0.7f);
    private static readonly Color RockDark = new Color(0.30f, 0.28f, 0.25f);
    private static readonly Color RockLight = new Color(0.50f, 0.47f, 0.42f);
    private static readonly Color BushDark = new Color(0.10f, 0.24f, 0.08f);
    private static readonly Color BushLight = new Color(0.20f, 0.40f, 0.12f);
    private static readonly Color FlowerPink = new Color(0.85f, 0.35f, 0.55f);
    private static readonly Color FlowerYellow = new Color(0.95f, 0.82f, 0.25f);
    private static readonly Color FogColor = new Color(0.60f, 0.75f, 0.60f, 0.12f);

    // Map bounds
    private const float MAP_W = 4800f;
    private const float MAP_H = 1200f;

    // Các Texture cây mới
    private Texture2D _texTreePixel;
    private Texture2D _texBanana;
    private Texture2D _texPalm;

    private Random _rng = new Random(42); // Seed cố định cho map nhất quán

    public override void _Ready()
    {
        // Load textures (.png) - Cập nhật lại đường dẫn đúng
        _texTreePixel = GD.Load<Texture2D>("res://Assets/Sprites/Environment/tree_pixel.png");
        _texBanana = GD.Load<Texture2D>("res://Assets/Sprites/Environment/banana_tree.png");
        _texPalm = GD.Load<Texture2D>("res://Assets/Sprites/Environment/palm_tree.png");

        BuildBackground();
        BuildPaths();
        BuildWaterFeatures();
        BuildRocks();
        BuildBushesAndFlowers();
        BuildFog();
        BuildGrassPatches();
        BuildFireflies();
        BuildPits();
        BuildSpikeTrap_Zone2();
        BuildSpikeTrap_Zone3();
        BuildSpikeTrap_Zone4();
        BuildEnemySnakes();
        BuildEnemyEagles();
        BuildCheckpoints();
        BuildForestTrees();
        BuildCaveEntrance();
        BuildMudPits();
        
        // Cập nhật: Áp dụng làm mờ cho các cây tĩnh được đặt thủ công trong Scene
        ApplyTransparencyToStaticTrees();
    }

    // ═══════════════════════════════════════════════════════════
    //  NỀN ĐẤT — Gradient rừng nhiều lớp
    // ═══════════════════════════════════════════════════════════
    private void BuildBackground()
    {
        // Lớp nền xa nhất (tối nhất)
        var bg = new ColorRect();
        bg.ZIndex = -100;
        bg.Position = new Vector2(-500, -500);
        bg.Size = new Vector2(MAP_W + 1000, MAP_H + 1000);
        bg.Color = ForestDark;
        AddChild(bg);

        // Lớp đất rừng chính
        var ground = new ColorRect();
        ground.ZIndex = -99;
        ground.Position = new Vector2(0, 0);
        ground.Size = new Vector2(MAP_W, MAP_H);
        ground.Color = ForestGround;
        AddChild(ground);

        // Các vệt cỏ sáng hơn tạo texture cho mặt đất
        for (int i = 0; i < 40; i++)
        {
            var grassPatch = new ColorRect();
            grassPatch.ZIndex = -98;
            grassPatch.Position = new Vector2(
                (float)_rng.NextDouble() * MAP_W,
                (float)_rng.NextDouble() * MAP_H
            );
            float w = 60 + (float)_rng.NextDouble() * 200;
            float h = 30 + (float)_rng.NextDouble() * 80;
            grassPatch.Size = new Vector2(w, h);
            grassPatch.Color = ForestGrass with { A = 0.3f + (float)_rng.NextDouble() * 0.4f };
            grassPatch.Rotation = (float)_rng.NextDouble() * 0.3f - 0.15f;
            AddChild(grassPatch);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  ĐƯỜNG MÒN — Chỉ dẫn cho người chơi
    // ═══════════════════════════════════════════════════════════
    private void BuildPaths()
    {
        // Đường chính xuyên suốt map (ngoằn ngoèo)
        Vector2[] pathPoints = new Vector2[] {
            new(100, 600), new(400, 550), new(700, 500), new(1000, 550),
            new(1300, 600), new(1600, 500), new(1900, 450), new(2200, 500),
            new(2500, 550), new(2800, 500), new(3100, 450), new(3400, 500),
            new(3700, 550), new(4000, 500), new(4300, 500), new(4600, 550)
        };

        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            DrawPathSegment(pathPoints[i], pathPoints[i + 1]);
        }

        // Nhánh phụ (dẫn tới kho báu / lối tắt)
        DrawPathSegment(new Vector2(1000, 550), new Vector2(1100, 300));
        DrawPathSegment(new Vector2(2500, 550), new Vector2(2400, 800));
        DrawPathSegment(new Vector2(3400, 500), new Vector2(3500, 250));
    }

    private void DrawPathSegment(Vector2 from, Vector2 to)
    {
        // Viền đường tối
        var edge = new Line2D();
        edge.ZIndex = -95;
        edge.Width = 48f;
        edge.DefaultColor = PathEdge;
        edge.AddPoint(from);
        edge.AddPoint(to);
        edge.BeginCapMode = Line2D.LineCapMode.Round;
        edge.EndCapMode = Line2D.LineCapMode.Round;
        AddChild(edge);

        // Thân đường sáng
        var path = new Line2D();
        path.ZIndex = -94;
        path.Width = 36f;
        path.DefaultColor = PathColor;
        path.AddPoint(from);
        path.AddPoint(to);
        path.BeginCapMode = Line2D.LineCapMode.Round;
        path.EndCapMode = Line2D.LineCapMode.Round;
        AddChild(path);
    }

    // ═══════════════════════════════════════════════════════════
    //  HỒ NƯỚC — Tạo cảm giác rừng sâu có suối
    // ═══════════════════════════════════════════════════════════
    private void BuildWaterFeatures()
    {
        // Hồ nhỏ Zone 2
        CreatePond(new Vector2(1400, 350), 120, 50);

        // Suối Zone 3-4
        CreatePond(new Vector2(2600, 750), 180, 70);

        // Vũng nước nhỏ rải rác
        CreatePond(new Vector2(800, 200), 60, 30);
        CreatePond(new Vector2(3200, 900), 80, 35);
    }

    private void CreatePond(Vector2 pos, float rx, float ry)
    {
        // Nước sâu
        var deep = new Polygon2D();
        deep.ZIndex = -90;
        deep.Position = pos;
        deep.Color = WaterDeep;
        deep.Polygon = MakeEllipsePolygon(rx, ry, 20);
        AddChild(deep);

        // Viền nước nông
        var shallow = new Polygon2D();
        shallow.ZIndex = -89;
        shallow.Position = pos;
        shallow.Color = WaterShallow;
        shallow.Polygon = MakeEllipsePolygon(rx + 12, ry + 6, 20);
        AddChild(shallow);

        // Hiệu ứng sóng lăn tăn
        var wave = new Polygon2D();
        wave.ZIndex = -88;
        wave.Position = pos + new Vector2(10, -5);
        wave.Color = new Color(1f, 1f, 1f, 0.08f);
        wave.Polygon = MakeEllipsePolygon(rx * 0.5f, ry * 0.3f, 12);
        AddChild(wave);

        // Animate sóng
        var tw = CreateTween();
        tw.SetLoops();
        tw.TweenProperty(wave, "position", pos + new Vector2(-10, 5), 3.0f)
          .SetTrans(Tween.TransitionType.Sine);
        tw.TweenProperty(wave, "position", pos + new Vector2(10, -5), 3.0f)
          .SetTrans(Tween.TransitionType.Sine);
    }

    // ═══════════════════════════════════════════════════════════
    //  ĐÁ LỚN — Chướng ngại vật / Trang trí
    // ═══════════════════════════════════════════════════════════
    private void BuildRocks()
    {
        Vector2[] rockPositions = {
            new(350, 300), new(900, 750), new(1600, 200),
            new(2100, 850), new(2900, 300), new(3600, 800),
            new(4200, 350), new(500, 900), new(3000, 150)
        };

        foreach (var pos in rockPositions)
        {
            float scale = 0.7f + (float)_rng.NextDouble() * 0.6f;
            CreateRock(pos, scale);
        }
    }

    private void CreateRock(Vector2 pos, float scale)
    {
        var rock = new Node2D();
        rock.Position = pos;
        rock.YSortEnabled = true;
        rock.ZIndex = 0;

        // Bóng đá
        var shadow = new Polygon2D();
        shadow.ZIndex = -1;
        shadow.Color = new Color(0, 0, 0, 0.3f);
        shadow.Polygon = new Vector2[] {
            new(-18 * scale, 6 * scale), new(-10 * scale, -4 * scale),
            new(8 * scale, -6 * scale), new(18 * scale, -2 * scale),
            new(14 * scale, 8 * scale), new(-4 * scale, 10 * scale)
        };
        shadow.Position = new Vector2(4, 4);
        rock.AddChild(shadow);

        // Thân đá (mặt tối)
        var body = new Polygon2D();
        body.Color = RockDark;
        body.Polygon = new Vector2[] {
            new(-16 * scale, 4 * scale), new(-12 * scale, -10 * scale),
            new(0, -14 * scale), new(14 * scale, -8 * scale),
            new(16 * scale, 2 * scale), new(8 * scale, 10 * scale),
            new(-6 * scale, 8 * scale)
        };
        rock.AddChild(body);

        // Highlight (mặt sáng trên)
        var highlight = new Polygon2D();
        highlight.Color = RockLight;
        highlight.Polygon = new Vector2[] {
            new(-10 * scale, -6 * scale), new(0, -14 * scale),
            new(10 * scale, -8 * scale), new(4 * scale, -2 * scale),
            new(-6 * scale, -2 * scale)
        };
        rock.AddChild(highlight);

        // Collision (ngăn không cho đi xuyên đá)
        var staticBody = new StaticBody2D();
        staticBody.CollisionLayer = 2;
        var col = new CollisionShape2D();
        var circleShape = new CircleShape2D();
        circleShape.Radius = 14 * scale;
        col.Shape = circleShape;
        staticBody.AddChild(col);
        rock.AddChild(staticBody);

        AddChild(rock);
    }

    // ═══════════════════════════════════════════════════════════
    //  BỤI CÂY & HOA — Tạo không gian sống động
    // ═══════════════════════════════════════════════════════════
    private void BuildBushesAndFlowers()
    {
        // Bụi cây rải đều
        for (int i = 0; i < 30; i++)
        {
            Vector2 pos = new Vector2(
                (float)_rng.NextDouble() * MAP_W,
                (float)_rng.NextDouble() * MAP_H
            );
            CreateBush(pos, 0.6f + (float)_rng.NextDouble() * 0.8f);
        }

        // Hoa trang trí dọc đường
        Vector2[] flowerSpots = {
            new(300, 580), new(600, 520), new(950, 570),
            new(1250, 620), new(1700, 480), new(2050, 520),
            new(2350, 530), new(2750, 510), new(3050, 470),
            new(3350, 510), new(3650, 570), new(4050, 510)
        };

        foreach (var spot in flowerSpots)
        {
            CreateFlowerCluster(spot);
        }
    }

    private void CreateBush(Vector2 pos, float scale)
    {
        var bush = new Node2D();
        bush.Position = pos;
        bush.YSortEnabled = true;

        // Bóng bụi
        var shadow = new Polygon2D();
        shadow.ZIndex = -1;
        shadow.Color = new Color(0, 0, 0, 0.2f);
        shadow.Polygon = MakeEllipsePolygon(14 * scale, 6 * scale, 8);
        shadow.Position = new Vector2(3, 5);
        bush.AddChild(shadow);

        // Lớp lá tối
        var dark = new Polygon2D();
        dark.Color = BushDark;
        dark.Polygon = MakeEllipsePolygon(12 * scale, 10 * scale, 10);
        dark.Position = new Vector2(0, -4 * scale);
        bush.AddChild(dark);

        // Lớp lá sáng
        var light = new Polygon2D();
        light.Color = BushLight;
        light.Polygon = MakeEllipsePolygon(10 * scale, 7 * scale, 10);
        light.Position = new Vector2(-2 * scale, -6 * scale);
        bush.AddChild(light);

        // Animate lá rung nhẹ
        var tw = CreateTween();
        tw.SetLoops();
        float dur = 2f + (float)_rng.NextDouble() * 2f;
        tw.TweenProperty(light, "position:x", light.Position.X + 2, dur)
          .SetTrans(Tween.TransitionType.Sine);
        tw.TweenProperty(light, "position:x", light.Position.X - 2, dur)
          .SetTrans(Tween.TransitionType.Sine);

        AddChild(bush);
    }

    private void CreateFlowerCluster(Vector2 center)
    {
        for (int i = 0; i < 3 + _rng.Next(4); i++)
        {
            var flower = new ColorRect();
            flower.ZIndex = -50;
            flower.Size = new Vector2(4, 4);
            flower.Position = center + new Vector2(
                (float)_rng.NextDouble() * 30 - 15,
                (float)_rng.NextDouble() * 20 - 10
            );
            flower.Color = _rng.Next(2) == 0 ? FlowerPink : FlowerYellow;
            flower.Rotation = (float)_rng.NextDouble() * 0.5f;
            AddChild(flower);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  SƯƠNG MÙ — Khí quyển huyền bí
    // ═══════════════════════════════════════════════════════════
    private void BuildFog()
    {
        for (int i = 0; i < 8; i++)
        {
            var fog = new ColorRect();
            fog.ZIndex = 10;
            fog.MouseFilter = Control.MouseFilterEnum.Ignore;
            float x = i * 600f;
            float y = (float)_rng.NextDouble() * MAP_H;
            fog.Position = new Vector2(x, y);
            fog.Size = new Vector2(400 + (float)_rng.NextDouble() * 300, 80 + (float)_rng.NextDouble() * 60);
            fog.Color = FogColor;
            AddChild(fog);

            // Sương trôi lơ lửng
            var tw = CreateTween();
            tw.SetLoops();
            float dur = 8f + (float)_rng.NextDouble() * 6f;
            tw.TweenProperty(fog, "position:x", x + 100, dur)
              .SetTrans(Tween.TransitionType.Sine);
            tw.TweenProperty(fog, "position:x", x - 100, dur)
              .SetTrans(Tween.TransitionType.Sine);

            // Mờ nhạt rồi hiện lại
            var tw2 = CreateTween();
            tw2.SetLoops();
            tw2.TweenProperty(fog, "modulate:a", 0.4f, dur * 0.7f)
               .SetTrans(Tween.TransitionType.Sine);
            tw2.TweenProperty(fog, "modulate:a", 1.0f, dur * 0.7f)
               .SetTrans(Tween.TransitionType.Sine);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  CỎ LƯỢN SÓNG — Chi tiết pixel-art
    // ═══════════════════════════════════════════════════════════
    private void BuildGrassPatches()
    {
        for (int i = 0; i < 60; i++)
        {
            Vector2 pos = new Vector2(
                (float)_rng.NextDouble() * MAP_W,
                (float)_rng.NextDouble() * MAP_H
            );
            CreateGrassTuft(pos);
        }
    }

    private void CreateGrassTuft(Vector2 pos)
    {
        var tuft = new Node2D();
        tuft.ZIndex = -80;
        tuft.Position = pos;

        int blades = 3 + _rng.Next(4);
        for (int i = 0; i < blades; i++)
        {
            var blade = new Line2D();
            blade.Width = 1.5f;
            float green = 0.3f + (float)_rng.NextDouble() * 0.3f;
            blade.DefaultColor = new Color(0.1f, green, 0.05f, 0.8f);
            float angle = -Mathf.Pi / 2 + ((float)_rng.NextDouble() - 0.5f) * 0.8f;
            float length = 6 + (float)_rng.NextDouble() * 10;
            blade.AddPoint(Vector2.Zero);
            blade.AddPoint(new Vector2(Mathf.Cos(angle) * length, Mathf.Sin(angle) * length));
            blade.Position = new Vector2((float)_rng.NextDouble() * 8 - 4, 0);
            tuft.AddChild(blade);
        }

        // Animate cỏ rung
        var tw = CreateTween();
        tw.SetLoops();
        float dur = 1.5f + (float)_rng.NextDouble() * 2f;
        tw.TweenProperty(tuft, "rotation", 0.05f, dur)
          .SetTrans(Tween.TransitionType.Sine);
        tw.TweenProperty(tuft, "rotation", -0.05f, dur)
          .SetTrans(Tween.TransitionType.Sine);

        AddChild(tuft);
    }

    // ═══════════════════════════════════════════════════════════
    //  ĐOM ĐÓM — Ánh sáng huyền bí (giống Màn 3 cũ)
    // ═══════════════════════════════════════════════════════════
    private void BuildFireflies()
    {
        for (int i = 0; i < 20; i++)
        {
            var fly = new ColorRect();
            fly.ZIndex = 20;
            fly.MouseFilter = Control.MouseFilterEnum.Ignore;
            fly.Size = new Vector2(3, 3);
            float x = (float)_rng.NextDouble() * MAP_W;
            float y = (float)_rng.NextDouble() * MAP_H;
            fly.Position = new Vector2(x, y);
            fly.Color = new Color(0.9f, 1.0f, 0.4f, 0.7f);
            AddChild(fly);

            // Bay lơ lửng
            var tw = CreateTween();
            tw.SetLoops();
            float dur = 2f + (float)_rng.NextDouble() * 3f;
            float amp = 10 + (float)_rng.NextDouble() * 30;
            tw.TweenProperty(fly, "position", new Vector2(x + amp, y - amp * 0.5f), dur)
              .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            tw.TweenProperty(fly, "position", new Vector2(x - amp * 0.3f, y + amp * 0.3f), dur)
              .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

            // Nhấp nháy sáng
            var tw2 = CreateTween();
            tw2.SetLoops();
            float d2 = 0.5f + (float)_rng.NextDouble() * 1.5f;
            tw2.TweenProperty(fly, "modulate:a", 0.2f, d2);
            tw2.TweenProperty(fly, "modulate:a", 1.0f, d2);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  BẪY GAI — 3 vùng bẫy khác nhau
    // ═══════════════════════════════════════════════════════════
    private void BuildSpikeTrap_Zone2()
    {
        // Zone 2 (800-1200): 2 bẫy gai xen kẽ
        var spike1 = new IsometricSpikeField();
        spike1.Position = new Vector2(1000, 550);
        spike1.SpikeCount = 4;
        spike1.UpDuration = 1.5f;
        spike1.DownDuration = 2.5f;
        spike1.StartDelay = 0f;
        AddChild(spike1);

        var spike2 = new IsometricSpikeField();
        spike2.Position = new Vector2(1100, 550);
        spike2.SpikeCount = 4;
        spike2.UpDuration = 1.5f;
        spike2.DownDuration = 2.5f;
        spike2.StartDelay = 1.5f; // Lệch pha!
        AddChild(spike2);
    }

    private void BuildSpikeTrap_Zone3()
    {
        // Zone 3 (2000-2600): Hàng rào gai chặn ngang
        for (int i = 0; i < 4; i++)
        {
            var spike = new IsometricSpikeField();
            spike.Position = new Vector2(2200 + i * 80, 480 + i * 20);
            spike.SpikeCount = 5;
            spike.UpDuration = 2.0f;
            spike.DownDuration = 1.5f;
            spike.StartDelay = i * 0.4f; // Hiệu ứng sóng
            AddChild(spike);
        }
    }

    private void BuildSpikeTrap_Zone4()
    {
        // Zone 4 (3200-3800): Bẫy gai + hố
        var spike = new IsometricSpikeField();
        spike.Position = new Vector2(3500, 500);
        spike.SpikeCount = 6;
        spike.UpDuration = 2.5f;
        spike.DownDuration = 1.0f;
        AddChild(spike);
    }

    // ═══════════════════════════════════════════════════════════
    //  QUÁI VẬT — Rắn tuần tra từng khu vực
    // ═══════════════════════════════════════════════════════════
    private void BuildEnemySnakes()
    {
        Vector2[] snakePositions = {
            new(700, 500),    // Zone 1: Rắn canh gác đầu đường
            new(1300, 580),   // Zone 2: Rắn gần bẫy gai
            new(1800, 450),   // Zone 2-3: Rắn trên đường mòn
            new(2500, 520),   // Zone 3: Rắn gần suối
            new(3000, 480),   // Zone 4: Rắn canh gác
            new(3400, 540),   // Zone 4: Rắn gần cửa hang
            new(3800, 500),   // Zone 5: Rắn canh cửa hang
        };

        var snakeScene = GD.Load<PackedScene>("res://Scenes/Enemies/IsometricSnake.tscn");
        if (snakeScene != null)
        {
            foreach (var pos in snakePositions)
            {
                var snake = snakeScene.Instantiate<Node2D>();
                snake.GlobalPosition = pos;
                AddChild(snake);
            }
        }
        else
        {
            GD.PrintErr("Level1Builder: Cannot load IsometricSnake.tscn!");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  ĐẠI BÀNG — Thêm áp lực tấn công trên không từ màn 1
    // ═══════════════════════════════════════════════════════════
    private void BuildEnemyEagles()
    {
        Vector2[] eaglePositions = {
            new(1100, 320),   // Zone 2: mở đầu gặp đại bàng
            new(2350, 260),   // Zone 3: giữa màn
            new(3650, 300),   // Zone 4-5: gần cửa hang
        };

        var eagleScene = GD.Load<PackedScene>("res://Scenes/Enemies/Eagle.tscn");
        if (eagleScene != null)
        {
            foreach (var pos in eaglePositions)
            {
                var eagle = eagleScene.Instantiate<Node2D>();
                eagle.GlobalPosition = pos;
                AddChild(eagle);
            }
        }
        else
        {
            GD.PrintErr("Level1Builder: Cannot load Eagle.tscn!");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  CHECKPOINTS — Cọc lưu tiến độ
    // ═══════════════════════════════════════════════════════════
    private void BuildCheckpoints()
    {
        float[] cpX = { 1200, 2400, 3600 };
        int idx = 1;
        foreach (float x in cpX)
        {
            var marker = new Marker2D();
            marker.Name = $"Checkpoint{idx}";
            marker.Position = new Vector2(x, 500);
            AddChild(marker);

            // Hiển thị ánh sáng checkpoint
            var light = new ColorRect();
            light.ZIndex = 5;
            light.Size = new Vector2(6, 20);
            light.Position = new Vector2(x - 3, 478);
            light.Color = new Color(0.9f, 0.8f, 0.2f, 0.6f);
            AddChild(light);

            // Ngọn lửa nhỏ trên đỉnh cọc
            var flame = new ColorRect();
            flame.ZIndex = 6;
            flame.Size = new Vector2(8, 8);
            flame.Position = new Vector2(x - 4, 472);
            flame.Color = new Color(1f, 0.5f, 0.1f, 0.9f);
            AddChild(flame);

            var tw = CreateTween();
            tw.SetLoops();
            tw.TweenProperty(flame, "modulate:a", 0.4f, 0.4f);
            tw.TweenProperty(flame, "modulate:a", 1.0f, 0.4f);

            idx++;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  HỆ THỐNG CÂY CỐI — Đa dạng hóa rừng Việt Nam (Chuối, Dừa)
    // ═══════════════════════════════════════════════════════════
    private void BuildForestTrees()
    {
        // Thêm một lớp cây đa dạng từ code thay vì chỉ có cây tĩnh
        var treeParent = GetNodeOrNull<Node2D>("../Trees");
        if (treeParent == null) return;

        // Xóa các cây tĩnh trong Scene để code tự quản lý hoàn toàn
        foreach (var child in treeParent.GetChildren())
        {
            child.QueueFree();
        }

        // Tạo 60 cây ngẫu nhiên (giảm số lượng để rừng thông thoáng hơn)
        int plantedCount = 0;
        int attempts = 0;
        int maxAttempts = 300; // Giới hạn số lần thử

        while (plantedCount < 60 && attempts < maxAttempts)
        {
            attempts++;
            Vector2 pos = new Vector2(
                (float)_rng.NextDouble() * MAP_W,
                (float)_rng.NextDouble() * (MAP_H - 150) + 75
            );

            // Tỷ lệ xuất hiện: 50% Cây cổ thụ, 30% Cây Chuối, 20% Cây Dừa
            double dice = _rng.NextDouble();
            Texture2D tex = _texTreePixel;
            float scale = 1.0f;
            string typePrefix = "Tree";

            if (dice < 0.5) {
                tex = _texTreePixel;
                scale = 0.8f + (float)_rng.NextDouble() * 0.5f;
                typePrefix = "AncientTree";
            }
            else if (dice < 0.8) {
                tex = _texBanana;
                scale = 0.6f + (float)_rng.NextDouble() * 0.4f;
                typePrefix = "BananaTree";
            }
            else {
                tex = _texPalm;
                scale = 0.9f + (float)_rng.NextDouble() * 0.4f;
                typePrefix = "PalmTree";
            }
            
            // XÍCH TRÁNH ĐƯỜNG: Kiểm tra trùng lặp
            if (IsPositionBlocked(pos)) continue;

            var tree = CreateTreeItem(treeParent, pos, tex, scale);
            tree.Name = $"{typePrefix}_{plantedCount}";
            plantedCount++;
        }
    }

    /// <summary>
    /// Tìm và áp dụng hiệu ứng làm mờ cho các cây được đặt thủ công trong Level 1 root
    /// </summary>
    private void ApplyTransparencyToStaticTrees()
    {
        var root = GetParent();
        if (root == null) return;

        foreach (var child in root.GetChildren())
        {
            if (child is Sprite2D sprite && sprite.GetNodeOrNull("DetectionArea") == null)
            {
                string name = sprite.Name.ToString().ToLower();
                // Kiểm tra tên hoặc texture để xác định đó là cây
                bool isTree = name.Contains("tree") || name.Contains("palm") || name.Contains("banana") || name.Contains("ancient");
                
                if (isTree)
                {
                    // Ước lượng scale từ scale hiện tại của sprite
                    float avgScale = (sprite.Scale.X + sprite.Scale.Y) / 2.0f;
                    SetupTransparencyForTree(sprite, avgScale);
                }
            }
        }
    }

    /// <summary>
    /// Kiểm tra xem vị trí có bị chặn bởi hồ, hố bẫy hoặc đường mòn không
    /// </summary>
    private bool IsPositionBlocked(Vector2 pos)
    {
        // 1. Kiểm tra Đường mòn (Dựa trên danh sách pathPoints hiện có trong BuildPaths)
        Vector2[] pathPoints = {
            new(100, 600), new(400, 550), new(700, 500), new(1000, 550),
            new(1300, 600), new(1600, 500), new(1900, 450), new(2200, 500),
            new(2500, 550), new(2800, 500), new(3100, 450), new(3400, 500),
            new(3700, 550), new(4000, 500), new(4300, 500), new(4600, 550)
        };
        foreach (var p in pathPoints) {
            if (pos.DistanceTo(p) < 80) return true; // Tránh đường mòn 80px
        }

        // 2. Kiểm tra Hồ nước (Ví dụ các vị trí hồ đã đặt)
        Vector2[] ponds = { new(1400, 350), new(2600, 750), new(800, 200), new(3200, 900) };
        foreach (var p in ponds) {
            if (pos.DistanceTo(p) < 130) return true; // Tránh hồ nước 130px
        }

        // 3. Kiểm tra Hố bẫy (Các vị trí hố bẫy vừa thêm)
        Vector2[] pits = { new(1200, 600), new(2500, 850), new(3800, 400), new(1800, 300), new(1800, 500) };
        foreach (var p in pits) {
            if (pos.DistanceTo(p) < 70) return true; // Tránh hố bẫy 70px
        }

        // 4. KIỂM TRA KHOẢNG TRỐNG CỬA HANG (Tránh 4300 -> 4800)
        if (pos.X > 4300) return true; 

        return false;
    }

    private Sprite2D CreateTreeItem(Node2D parent, Vector2 pos, Texture2D tex, float scale)
    {
        if (tex == null) return null;
        
        var tree = new Sprite2D();
        tree.Texture = tex;
        tree.Position = pos;
        tree.Scale = new Vector2(scale, scale);
        tree.YSortEnabled = true;
        tree.Offset = new Vector2(0, -32); // Thụt gốc cây lên trên để Y-sort đúng

        // Thêm Shader đổ bóng cho sinh động
        var shader = GD.Load<Shader>("res://Assets/Shaders/drop_shadow.gdshader");
        if (shader != null) {
            var mat = new ShaderMaterial();
            mat.Shader = shader;
            mat.SetShaderParameter("shadow_color", new Color(0, 0, 0, 0.4f));
            mat.SetShaderParameter("offset", new Vector2(8, 8));
            tree.Material = mat;
        }

        parent.AddChild(tree);
        // Gán Owner để cây hiển thị trên bảng Scene của Editor
        tree.Owner = GetTree().EditedSceneRoot;

        SetupTransparencyForTree(tree, scale);
        return tree;
    }

    /// <summary>
    /// Thiết lập vùng nhận diện và logic làm mờ cho một Sprite cây
    /// </summary>
    private void SetupTransparencyForTree(Sprite2D tree, float scale)
    {
        // THÊM VÙNG NHẬN DIỆN LÀM MỜ (Detection Area)
        var area = new Area2D();
        area.Name = "DetectionArea";
        area.CollisionLayer = 0;
        area.CollisionMask = 1; // Nhận diện Player (Layer 1)
        
        var col = new CollisionShape2D();
        
        // Lấy kích thước thực tế của Texture để tạo vùng nhận diện bao phủ toàn bộ cây
        Vector2 texSize = new Vector2(128, 128); // Mặc định nếu không lấy được texture
        if (tree.Texture != null)
        {
            texSize = tree.Texture.GetSize();
        }

        var shape = new RectangleShape2D();
        // Thu nhỏ vùng nhận diện lại một chút (75% chiều rộng, 90% chiều cao) 
        // để tránh việc mờ quá sớm khi mới chạm vào khoảng trống của file PNG
        shape.Size = new Vector2(texSize.X * 0.75f, texSize.Y * 0.9f) * scale;
        col.Shape = shape;

        // Căn chỉnh vị trí vùng nhận diện khớp với hình ảnh cây (dựa trên Offset của Sprite)
        col.Position = tree.Offset * scale;
        
        area.AddChild(col);
        tree.AddChild(area);

        // Debug: Vẽ vùng nhận diện nếu cần (có thể bật trong Editor)
        // area.ZIndex = 100;

        // Kết nối sự kiện làm mờ
        area.BodyEntered += (body) => {
            if (body.IsInGroup("player") || body is IsometricPlayer || body is CharacterBody2D) {
                var tw = tree.CreateTween();
                tw.TweenProperty(tree, "modulate:a", 0.35f, 0.2f);
            }
        };
        area.BodyExited += (body) => {
            if (body.IsInGroup("player") || body is IsometricPlayer || body is CharacterBody2D) {
                var tw = tree.CreateTween();
                tw.TweenProperty(tree, "modulate:a", 1.0f, 0.2f);
            }
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  CỬA HANG — Điểm kết thúc màn
    // ═══════════════════════════════════════════════════════════
    private void BuildCaveEntrance()
    {
        var cave = new Node2D();
        cave.Position = new Vector2(4500, 500);
        cave.YSortEnabled = true;

        // Vách đá trái
        var cliffL = new Polygon2D();
        cliffL.Color = RockDark;
        cliffL.Polygon = new Vector2[] {
            new(-30, -200), new(0, -220), new(20, -180),
            new(20, -60), new(-5, -40), new(-30, -60)
        };
        cave.AddChild(cliffL);

        // Vách đá phải
        var cliffR = new Polygon2D();
        cliffR.Color = RockDark;
        cliffR.Polygon = new Vector2[] {
            new(-30, 60), new(-5, 40), new(20, 60),
            new(20, 200), new(0, 220), new(-30, 190)
        };
        cave.AddChild(cliffR);

        // Bóng tối cửa hang
        var darkness = new ColorRect();
        darkness.ZIndex = -1;
        darkness.Size = new Vector2(200, 120);
        darkness.Position = new Vector2(10, -60);
        darkness.Color = new Color(0, 0, 0, 0.95f);
        cave.AddChild(darkness);

        // Highlight viền hang (tạo chiều sâu)
        var rimL = new Polygon2D();
        rimL.Color = RockLight;
        rimL.Polygon = new Vector2[] {
            new(-5, -60), new(5, -60), new(5, 60), new(-5, 60)
        };
        cave.AddChild(rimL);

        // Va chạm vách hang
        var sb = new StaticBody2D();
        sb.CollisionLayer = 2;
        var colL = new CollisionShape2D();
        var shapeL = new RectangleShape2D();
        shapeL.Size = new Vector2(50, 140);
        colL.Shape = shapeL;
        colL.Position = new Vector2(-5, -130);
        sb.AddChild(colL);

        var colR = new CollisionShape2D();
        var shapeR = new RectangleShape2D();
        shapeR.Size = new Vector2(50, 140);
        colR.Shape = shapeR;
        colR.Position = new Vector2(-5, 130);
        sb.AddChild(colR);
        cave.AddChild(sb);

        // Dùng scene cổng hang có sẵn để xử lý chuyển màn.
        var caveDoorScene = GD.Load<PackedScene>("res://Scenes/Levels/CaveDoor.tscn");
        if (caveDoorScene != null)
        {
            var caveDoor = caveDoorScene.Instantiate<Node2D>();
            caveDoor.Position = new Vector2(110, 0);
            cave.AddChild(caveDoor);
        }
        else
        {
            GD.PrintErr("Level1Builder: Cannot load CaveDoor.tscn!");
        }

        AddChild(cave);
    }

    // ═══════════════════════════════════════════════════════════
    //  HỐ BẪY — Chướng ngại vật nguy hiểm
    // ═══════════════════════════════════════════════════════════
    private void BuildPits()
    {
        // Thêm một vài hố bẫy tại các vị trí chiến thuật
        CreatePit(new Vector2(1200, 600), 45, 22); // Chặn đường chính Zone 2
        CreatePit(new Vector2(2500, 850), 55, 28); // Gần khu vực suối Zone 3
        CreatePit(new Vector2(3800, 400), 40, 20); // Gần cuối map Zone 5
                                                   
        // Một cặp hố nhỏ tạo thành khe hẹp
        CreatePit(new Vector2(1800, 300), 30, 15);
        CreatePit(new Vector2(1800, 500), 30, 15);
    }

    private void CreatePit(Vector2 pos, float rx, float ry)
    {
        // 1. Phần hình ảnh hiển thị hố
        var pitVisual = new Polygon2D();
        pitVisual.ZIndex = -95; // Nằm trên mặt đất nhưng dưới cỏ/cây
        pitVisual.Position = pos;
        pitVisual.Color = new Color(0.02f, 0.02f, 0.02f); // Màu đen sâu thẳm
        pitVisual.Polygon = MakeEllipsePolygon(rx, ry, 16);
        AddChild(pitVisual);

        // Viền hố để trông tự nhiên hơn
        var edge = new Polygon2D();
        edge.ZIndex = -96;
        edge.Position = pos;
        edge.Color = PathEdge;
        edge.Polygon = MakeEllipsePolygon(rx + 4, ry + 2, 16);
        AddChild(edge);

        // 2. Logic va chạm (Sử dụng class IsometricPit)
        var pitLogic = new IsometricPit();
        pitLogic.Position = pos;
        
        var collision = new CollisionPolygon2D();
        collision.Polygon = pitVisual.Polygon;
        pitLogic.AddChild(collision);
        
        AddChild(pitLogic);
    }

    // ═══════════════════════════════════════════════════════════
    //  HỐ BÙN — Chướng ngại vật làm chậm
    // ═══════════════════════════════════════════════════════════
    private void BuildMudPits()
    {
        // Thêm hố bùn rải rác trên bản đồ (thường xuất hiện gần hồ nước hoặc đường mòn)
        CreateMudPit(new Vector2(1600, 520), 80, 45); // Chặn hướng đi Zone 2
        CreateMudPit(new Vector2(2800, 350), 65, 35); // Gần suối Zone 3
        CreateMudPit(new Vector2(600, 250), 55, 30);  // Zone 1
        CreateMudPit(new Vector2(3400, 850), 75, 40); // Zone 4
    }

    private void CreateMudPit(Vector2 pos, float rx, float ry)
    {
        // 1. Phân giác màu bùn (Nước bùn đậm)
        var mudVisual = new Polygon2D();
        mudVisual.ZIndex = -92; // Dưới cỏ nhưng trên đất
        mudVisual.Position = pos;
        mudVisual.Color = new Color(0.24f, 0.18f, 0.12f, 0.85f);
        // Tạo đa giác méo mó tự nhiên
        mudVisual.Polygon = MakeIrregularEllipse(rx, ry, 18, 0.25f);
        AddChild(mudVisual);

        // 2. Viền bùn nhạt hơn (Vũng lầy)
        var rim = new Polygon2D();
        rim.ZIndex = -93;
        rim.Position = pos;
        rim.Color = new Color(0.35f, 0.28f, 0.20f, 0.6f);
        rim.Polygon = MakeIrregularEllipse(rx + 15, ry + 10, 18, 0.2f);
        AddChild(rim);

        // Hiệu ứng bọt khí bùn thỉnh thoảng hiện lên
        for (int i = 0; i < 3; i++)
        {
            var bubble = new ColorRect();
            bubble.Size = new Vector2(4, 4);
            bubble.Color = new Color(0.4f, 0.35f, 0.3f, 0.7f);
            bubble.Position = new Vector2(
                (float)_rng.NextDouble() * rx - rx/2,
                (float)_rng.NextDouble() * ry - ry/2
            );
            mudVisual.AddChild(bubble);
            
            var tw = CreateTween();
            tw.SetLoops();
            tw.TweenProperty(bubble, "scale", new Vector2(2, 2), 2.0f).SetDelay((float)_rng.NextDouble() * 3);
            tw.TweenProperty(bubble, "modulate:a", 0f, 0.5f);
            tw.TweenCallback(Callable.From(() => bubble.Scale = Vector2.One));
        }

        // 3. Logic làm chậm (IsometricMudPit)
        var mudLogic = new IsometricMudPit();
        mudLogic.Position = pos;
        
        var collision = new CollisionPolygon2D();
        collision.Polygon = mudVisual.Polygon;
        mudLogic.AddChild(collision);
        
        AddChild(mudLogic);
    }

    // ═══════════════════════════════════════════════════════════
    //  HELPER — Tạo polygon hình oval rách rưới tự nhiên
    // ═══════════════════════════════════════════════════════════
    private Vector2[] MakeEllipsePolygon(float rx, float ry, int segments)
    {
        var points = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.Pi * 2;
            points[i] = new Vector2(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry);
        }
        return points;
    }

    private Vector2[] MakeIrregularEllipse(float rx, float ry, int segments, float jitter = 0.15f)
    {
        var points = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.Pi * 2;
            // Áp dụng jitter để ngẫu nhiên biến dạng các đỉnh, tạo vẻ tự nhiên
            float r_jitter = 1.0f + ((float)_rng.NextDouble() - 0.5f) * jitter;
            points[i] = new Vector2(Mathf.Cos(angle) * rx * r_jitter, Mathf.Sin(angle) * ry * r_jitter);
        }
        return points;
    }
}
