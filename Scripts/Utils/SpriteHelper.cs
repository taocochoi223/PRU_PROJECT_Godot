using Godot;
using System;
using System.Collections.Generic;

public static class SpriteHelper
{
    private static SpriteFrames _cachedPlayerFrames = null;

    public static SpriteFrames CreatePlayerSpriteFrames()
    {
        if (_cachedPlayerFrames != null) return _cachedPlayerFrames;

        var anims = new Dictionary<string, Texture2D[]>();

        // 1. Load Idle
        var idleImage = LoadAndCleanImage("res://Assets/Sprites/Player/Tháchanh_ đưng yen.png");
        var idleTextures = new List<Texture2D>();

        if (idleImage != null)
        {
            var blobs = FindBlobs(idleImage);
            foreach (var r in blobs)
            {
                idleTextures.Add(SmartPad(idleImage, r, 240, 240));
            }
            if (idleTextures.Count > 0) anims["idle"] = idleTextures.ToArray();
            GD.Print($"Idle: found {blobs.Count} blobs, created {idleTextures.Count} frames");
        }

        // 2. Run animation - load từ sprite sheet thach_sanh_run.png (3 cột x 2 hàng = 6 frames)
        var runImage = LoadAndCleanImage("res://Assets/Sprites/Player/thach_sanh_run.png");
        var runTextures = new List<Texture2D>();

        if (runImage != null)
        {
            // Sprite sheet 3x2 grid
            var runFramesRaw = SliceSpriteSheetGridRaw(runImage, 3, 2);
            GD.Print($"Run: sliced {runFramesRaw.Count} raw frames from sprite sheet");

            foreach (var frameImg in runFramesRaw)
            {
                // Tìm bounding box thực của nhân vật trong frame
                var bounds = FindBounds(frameImg);
                if (bounds.Size.X > 5 && bounds.Size.Y > 5)
                {
                    // Pad vào frame chuẩn 240x240 giống idle
                    runTextures.Add(SmartPad(frameImg, bounds, 240, 240));
                }
            }

            if (runTextures.Count > 0)
            {
                anims["run"] = runTextures.ToArray();
            }
            GD.Print($"Run: created {runTextures.Count} frames from thach_sanh_run.png");
        }
        else
        {
            GD.Print("Run: failed to load thach_sanh_run.png, falling back to idle frames");
            if (idleTextures.Count > 0)
            {
                anims["run"] = idleTextures.ToArray();
            }
        }

        // Jump animation - load from ThachSanh_Ani_Nhay.png (2 cols x 2 rows = 4 frames)
        var jumpImage = LoadAndCleanImage("res://Assets/Sprites/Player/ThachSanh_Ani_Nhay.png");
        var jumpTextures = new List<Texture2D>();

        if (jumpImage != null)
        {
            var jumpFramesRaw = SliceSpriteSheetGridRaw(jumpImage, 2, 2);
            GD.Print($"Jump: sliced {jumpFramesRaw.Count} raw frames from sprite sheet");

            foreach (var frameImg in jumpFramesRaw)
            {
                var bounds = FindBounds(frameImg);
                if (bounds.Size.X > 5 && bounds.Size.Y > 5)
                {
                    // Scale jump animation by 0.65x so it matches the normal character size
                    jumpTextures.Add(SmartPad(frameImg, bounds, 240, 240, 0.65f));
                }
            }

            if (jumpTextures.Count >= 4)
            {
                // Mặc định: 2 frame đầu nhảy lên, 2 frame sau rơi xuống
                anims["jump"] = new[] { jumpTextures[0], jumpTextures[1] };
                anims["fall"] = new[] { jumpTextures[2], jumpTextures[3] };
            }
            else if (jumpTextures.Count > 0)
            {
                anims["jump"] = jumpTextures.ToArray();
                anims["fall"] = jumpTextures.ToArray();
            }
            GD.Print($"Jump: created {jumpTextures.Count} frames from ThachSanh_Ani_Nhay.png");
        }
        else
        {
            GD.Print("Jump: failed to load ThachSanh_Ani_Nhay.png");
        }

        // 3. Load Attack (Sử dụng Grid 3 cột x 4 hàng)
        var attackImage = LoadAndCleanImage("res://Assets/Sprites/Player/ThachSanhChem.png");
        var attackTextures = new List<Texture2D>();

        if (attackImage != null)
        {
            // Cắt lưới 3 cột x 4 hàng (tổng 12 frames)
            var attackFramesRaw = SliceSpriteSheetGridRaw(attackImage, 3, 4);
            GD.Print($"Attack: sliced {attackFramesRaw.Count} raw frames from grid");

            foreach (var frameImg in attackFramesRaw)
            {
                var bounds = FindBounds(frameImg);
                if (bounds.Size.X > 5 && bounds.Size.Y > 5)
                {
                    attackTextures.Add(SmartPad(frameImg, bounds, 240, 240));
                }
                else
                {
                    // Nếu ô trống, tạo một frame rỗng (fallback) để giữ đủ 12 frames
                    attackTextures.Add(CreateColoredRect(240, 240, new Color(0, 0, 0, 0)));
                }
            }

            if (attackTextures.Count >= 12)
            {
                anims["attack1"] = new[] { attackTextures[0], attackTextures[1], attackTextures[2] };
                anims["attack2"] = new[] { attackTextures[3], attackTextures[4], attackTextures[5] };
                anims["attack3"] = new[] { attackTextures[6], attackTextures[7], attackTextures[8] };
                anims["attack4"] = new[] { attackTextures[9], attackTextures[10], attackTextures[11] };
                anims["attack"] = anims["attack1"];
            }
            else if (attackTextures.Count > 0)
            {
                anims["attack"] = attackTextures.ToArray();
            }
            GD.Print($"Attack: successfully assigned 4 combo attacks from {attackTextures.Count} processed frames.");
        }

        // 4. Fallbacks
        Texture2D defaultFrame = idleTextures.Count > 0 ? idleTextures[0] : CreateColoredRect(60, 80, new Color(0.7f, 0.45f, 0.2f));

        if (!anims.ContainsKey("idle")) anims["idle"] = new[] { defaultFrame };
        if (!anims.ContainsKey("run")) anims["run"] = new[] { defaultFrame };
        if (!anims.ContainsKey("attack1")) anims["attack1"] = new[] { defaultFrame };
        if (!anims.ContainsKey("attack2")) anims["attack2"] = new[] { defaultFrame };
        if (!anims.ContainsKey("attack3")) anims["attack3"] = new[] { defaultFrame };
        if (!anims.ContainsKey("attack4")) anims["attack4"] = new[] { defaultFrame };
        if (!anims.ContainsKey("attack")) anims["attack"] = new[] { defaultFrame };

        if (!anims.ContainsKey("jump")) anims["jump"] = new[] { defaultFrame };
        if (!anims.ContainsKey("fall")) anims["fall"] = new[] { defaultFrame };

        anims["hurt"] = new[] { defaultFrame };
        anims["die"] = new[] { defaultFrame };

        _cachedPlayerFrames = BuildSpriteFrames(anims, 12.0f);
        return _cachedPlayerFrames;
    }

    /// <summary>
    /// Tìm các vùng sprite riêng biệt trong ảnh
    /// </summary>
    private static List<Rect2I> FindBlobs(Image img)
    {
        int w = img.GetWidth();
        int h = img.GetHeight();
        bool[,] visited = new bool[w, h];
        var blobs = new List<Rect2I>();

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!visited[x, y] && img.GetPixel(x, y).A > 0.1f)
                {
                    int minX = x, minY = y, maxX = x, maxY = y;
                    var q = new Queue<Vector2I>();
                    q.Enqueue(new Vector2I(x, y));
                    visited[x, y] = true;
                    int pixelCount = 0;

                    while (q.Count > 0)
                    {
                        var curr = q.Dequeue();
                        pixelCount++;

                        if (curr.X < minX) minX = curr.X;
                        if (curr.X > maxX) maxX = curr.X;
                        if (curr.Y < minY) minY = curr.Y;
                        if (curr.Y > maxY) maxY = curr.Y;

                        for (int dx = -2; dx <= 2; dx++)
                        {
                            for (int dy = -2; dy <= 2; dy++)
                            {
                                int nx = curr.X + dx;
                                int ny = curr.Y + dy;
                                if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                                {
                                    if (!visited[nx, ny] && img.GetPixel(nx, ny).A > 0.1f)
                                    {
                                        visited[nx, ny] = true;
                                        q.Enqueue(new Vector2I(nx, ny));
                                    }
                                }
                            }
                        }
                    }

                    if (pixelCount > 50)
                    {
                        blobs.Add(new Rect2I(minX, minY, maxX - minX + 1, maxY - minY + 1));
                    }
                }
            }
        }

        // Merge intersecting blobs
        for (int i = 0; i < blobs.Count; i++)
        {
            for (int j = i + 1; j < blobs.Count; j++)
            {
                if (blobs[i].Intersects(blobs[j].Grow(1)))
                {
                    blobs[i] = blobs[i].Merge(blobs[j]);
                    blobs.RemoveAt(j);
                    j--;
                }
            }
        }

        // Sort by row then column
        blobs.Sort((a, b) =>
        {
            int rowA = (a.Position.Y + a.Size.Y / 2) / 150;
            int rowB = (b.Position.Y + b.Size.Y / 2) / 150;
            if (rowA != rowB) return rowA.CompareTo(rowB);
            return a.Position.X.CompareTo(b.Position.X);
        });

        return blobs;
    }

    /// <summary>
    /// Cắt và căn chỉnh sprite vào frame cố định, cho phép đổi tỷ lệ với customScale
    /// </summary>
    public static ImageTexture SmartPad(Image source, Rect2I rect, int outW, int outH, float customScale = 1.0f)
    {
        int bw = rect.Size.X;
        int bh = rect.Size.Y;

        var crop = Image.CreateEmpty(bw, bh, false, Image.Format.Rgba8);
        for (int x = 0; x < bw; x++)
        {
            for (int y = 0; y < bh; y++)
            {
                int sx = rect.Position.X + x;
                int sy = rect.Position.Y + y;
                if (sx >= 0 && sy >= 0 && sx < source.GetWidth() && sy < source.GetHeight())
                {
                    crop.SetPixel(x, y, source.GetPixel(sx, sy));
                }
            }
        }

        int targetMaxHeight = outH - 20;
        int targetMaxWidth = outW - 20;

        float scaleToFit = 1.0f;
        if (bw > targetMaxWidth || bh > targetMaxHeight)
        {
            float scaleX = (float)targetMaxWidth / bw;
            float scaleY = (float)targetMaxHeight / bh;
            scaleToFit = Math.Min(scaleX, scaleY);
        }

        // Áp dụng scale bằng customScale kết hợp với scaleToFit
        float finalScale = scaleToFit * customScale;

        if (Math.Abs(finalScale - 1.0f) > 0.01f)
        {
            int newW = Math.Max(1, (int)(bw * finalScale));
            int newH = Math.Max(1, (int)(bh * finalScale));

            var interp = (bw > 600 || bh > 600) ? Image.Interpolation.Bilinear : Image.Interpolation.Nearest;
            crop.Resize(newW, newH, interp);
            bw = newW;
            bh = newH;
        }

        var frame = Image.CreateEmpty(outW, outH, false, Image.Format.Rgba8);
        int destX = (outW - bw) / 2;
        int destY = outH - bh - 2;

        for (int x = 0; x < bw; x++)
        {
            for (int y = 0; y < bh; y++)
            {
                if (destX + x >= 0 && destX + x < outW && destY + y >= 0 && destY + y < outH)
                {
                    frame.SetPixel(destX + x, destY + y, crop.GetPixel(x, y));
                }
            }
        }
        return ImageTexture.CreateFromImage(frame);
    }

    /// <summary>
    /// Cắt sprite sheet theo grid và trả về List Image (raw, chưa convert sang texture)
    /// </summary>
    private static List<Image> SliceSpriteSheetGridRaw(Image img, int cols, int rows)
    {
        var frames = new List<Image>();

        int imgW = img.GetWidth();
        int imgH = img.GetHeight();
        int frameW = imgW / cols;
        int frameH = imgH / rows;

        GD.Print($"SliceSpriteSheetGridRaw: img={imgW}x{imgH}, grid={cols}x{rows}, frame={frameW}x{frameH}");

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int x = col * frameW;
                int y = row * frameH;

                var crop = Image.CreateEmpty(frameW, frameH, false, Image.Format.Rgba8);
                for (int px = 0; px < frameW; px++)
                {
                    for (int py = 0; py < frameH; py++)
                    {
                        int sx = x + px;
                        int sy = y + py;
                        if (sx < imgW && sy < imgH)
                        {
                            crop.SetPixel(px, py, img.GetPixel(sx, sy));
                        }
                    }
                }

                frames.Add(crop);
            }
        }

        return frames;
    }

    /// <summary>
    /// Tìm bounding box của pixels visible trong một image
    /// </summary>
    public static Rect2I FindBounds(Image img)
    {
        int w = img.GetWidth();
        int h = img.GetHeight();
        int minX = w, minY = h, maxX = 0, maxY = 0;
        bool found = false;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (img.GetPixel(x, y).A > 0.1f)
                {
                    found = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (!found) return new Rect2I(0, 0, 0, 0);
        return new Rect2I(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    /// <summary>
    /// Cắt sprite sheet theo grid cố định (cols x rows)
    /// </summary>
    private static List<Texture2D> SliceSpriteSheetGrid(Image img, int cols, int rows)
    {
        var textures = new List<Texture2D>();

        int imgW = img.GetWidth();
        int imgH = img.GetHeight();
        int frameW = imgW / cols;
        int frameH = imgH / rows;

        GD.Print($"SliceSpriteSheetGrid: img={imgW}x{imgH}, grid={cols}x{rows}, frame={frameW}x{frameH}");

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int x = col * frameW;
                int y = row * frameH;

                // Crop frame
                var crop = Image.CreateEmpty(frameW, frameH, false, Image.Format.Rgba8);
                for (int px = 0; px < frameW; px++)
                {
                    for (int py = 0; py < frameH; py++)
                    {
                        int sx = x + px;
                        int sy = y + py;
                        if (sx < imgW && sy < imgH)
                        {
                            crop.SetPixel(px, py, img.GetPixel(sx, sy));
                        }
                    }
                }

                textures.Add(ImageTexture.CreateFromImage(crop));
            }
        }

        return textures;
    }

    public static SpriteFrames CreateSnakeSpriteFrames()
    {
        var imgWalk = LoadAndCleanImage("res://Assets/Sprites/Enemies/snack3_move.png");
        var imgHurt = LoadAndCleanImage("res://Assets/Sprites/Enemies/snack_bidanh.png");

        if (imgWalk == null) return CreateFallbackSprites(new Color(0.2f, 0.6f, 0.15f), false);

        // Ảnh di chuyển là lưới 4x1
        var rawFramesWalk = SliceSpriteSheetGridRaw(imgWalk, 4, 1);
        var texturesWalk = new List<Texture2D>();

        foreach (var frameImg in rawFramesWalk)
        {
            var bounds = FindBounds(frameImg);
            if (bounds.Size.X > 5 && bounds.Size.Y > 5)
                texturesWalk.Add(SmartPad(frameImg, bounds, 350, 350)); // Căn giữa và chuẩn hóa size
            else
                texturesWalk.Add(CreateColoredRect(350, 350, new Color(0, 0, 0, 0)));
        }

        var texturesHurt = new List<Texture2D>();
        if (imgHurt != null)
        {
            // Ảnh bị đánh là lưới 2x1
            var rawFramesHurt = SliceSpriteSheetGridRaw(imgHurt, 2, 1);
            foreach (var frameImg in rawFramesHurt)
            {
                var bounds = FindBounds(frameImg);
                if (bounds.Size.X > 5 && bounds.Size.Y > 5)
                    texturesHurt.Add(SmartPad(frameImg, bounds, 350, 350));
                else
                    texturesHurt.Add(CreateColoredRect(350, 350, new Color(0, 0, 0, 0)));
            }
        }
        else
        {
            // Fallback nếu không có ảnh bị đánh
            texturesHurt.Add(texturesWalk[0]);
            texturesHurt.Add(texturesWalk.Count > 3 ? texturesWalk[3] : texturesWalk[0]);
        }

        // Tạo animation
        return BuildSpriteFrames(new Dictionary<string, Texture2D[]> {
            // Di chuyển dùng các khung hình uốn lượn liên tiếp
            {"walk", texturesWalk.ToArray()}, 
            // Tấn công dùng các khung có đầu vươn ra xa nhất
            {"attack", new[] { texturesWalk[0], texturesWalk[1], texturesWalk[3], texturesWalk[0] }},
            // Bị thương là frame 1 của ảnh bidanh
            {"hurt", new[] { texturesHurt[0] }},
            // Chết là frame 2 của ảnh bidanh
            {"die", new[] { texturesHurt[1] }}
        }, 8.0f); // Tốc độ di chuyển tăng lên 8 khung hình / giây
    }

    public static SpriteFrames CreateEagleSpriteFrames()
    {
        var frames = LoadSpriteSheet2x2("res://Assets/Sprites/Enemies/eagle.png");
        if (frames == null) return CreateFallbackSprites(new Color(0.35f, 0.25f, 0.15f), false);
        return BuildSpriteFrames(new Dictionary<string, Texture2D[]> {
            {"walk", new[] { frames[0] }}, {"attack", new[] { frames[1] }},
            {"hurt", new[] { frames[2] }}, {"die", new[] { frames[3] }}
        });
    }

    public static SpriteFrames CreatePrincessSpriteFrames()
    {
        var frames = LoadSpriteSheet2x2("res://Assets/Sprites/NPCs/princess.png");
        if (frames == null) return CreateFallbackSprites(Colors.DeepPink, false);
        return BuildSpriteFrames(new Dictionary<string, Texture2D[]> {
            {"idle", new[] { frames[0], frames[2] }}, {"rescued", new[] { frames[1], frames[3] }}
        }, 4.0f);
    }

    private static Texture2D[] LoadSpriteSheet2x2(string path)
    {
        var img = LoadAndCleanImage(path); if (img == null) return null;
        int w = img.GetWidth() / 2, h = img.GetHeight() / 2;
        return new[] { Crop(img, 0, 0, w, h), Crop(img, w, 0, w, h), Crop(img, 0, h, w, h), Crop(img, w, h, w, h) };
    }

    private static ImageTexture Crop(Image source, int x, int y, int w, int h)
    {
        var crop = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        for (int px = 0; px < w; px++) for (int py = 0; py < h; py++)
            {
                int sx = x + px, sy = y + py;
                if (sx >= 0 && sy >= 0 && sx < source.GetWidth() && sy < source.GetHeight())
                    crop.SetPixel(px, py, source.GetPixel(sx, sy));
            }
        return ImageTexture.CreateFromImage(crop);
    }

    public static Image LoadAndCleanImage(string path)
    {
        var tex = GD.Load<Texture2D>(path); if (tex == null) return null;
        var img = tex.GetImage(); img.Decompress(); img.Convert(Image.Format.Rgba8);
        RemoveBackground(img); return img;
    }

    public static void RemoveBackground(Image img)
    {
        int w = img.GetWidth(), h = img.GetHeight();

        // Sample corner to detect background type
        Color bg = img.GetPixel(0, 0);
        bool isGreenScreen = bg.G > 0.4f && bg.G > bg.R && bg.G > bg.B;
        bool isMagentaScreen = bg.R > 0.4f && bg.B > 0.4f && bg.R > bg.G && bg.B > bg.G;
        bool isBlueScreen = bg.B > 0.4f && bg.B > bg.R && bg.B > bg.G;

        // Detect checkered/caro pattern (transparent grid pattern)
        // Usually alternating light gray and white or similar
        Color bg2 = (w > 1 && h > 1) ? img.GetPixel(1, 0) : bg;
        bool isCheckered = IsCheckeredBackground(bg, bg2);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Color p = img.GetPixel(x, y);
                bool remove = false;

                if (isGreenScreen)
                {
                    // Chromakey removal: Green is dominant
                    if (p.G > 0.3f && p.G > p.R * 1.1f && p.G > p.B * 1.1f) remove = true;
                    // Catch dark green shadows
                    else if (p.G > 0.15f && p.G > p.R * 1.4f) remove = true;
                }
                else if (isMagentaScreen)
                {
                    // Chromakey removal: Magenta (Red + Blue) is dominant
                    if (p.R > 0.3f && p.B > 0.3f && p.R > p.G * 1.1f && p.B > p.G * 1.1f) remove = true;
                }
                else if (isBlueScreen)
                {
                    // Chromakey removal: Blue is dominant
                    if (p.B > 0.3f && p.B > p.R * 1.1f && p.B > p.G * 1.1f) remove = true;
                }
                else if (isCheckered)
                {
                    // Remove checkered/caro background (typically gray/white pattern)
                    if (IsCheckeredPixel(p, bg, bg2)) remove = true;
                }
                else
                {
                    if (ColorsClose(p, bg, 0.1f)) remove = true;
                }

                if (remove) img.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
    }

    /// <summary>
    /// Detect if background is a checkered/caro pattern (transparency grid)
    /// </summary>
    private static bool IsCheckeredBackground(Color c1, Color c2)
    {
        // Checkered background typically has two similar gray tones
        // Common pattern: light gray (#CCCCCC) and white (#FFFFFF)
        // Or: gray (#808080) and light gray (#C0C0C0)

        bool c1IsGrayish = Math.Abs(c1.R - c1.G) < 0.1f && Math.Abs(c1.G - c1.B) < 0.1f;
        bool c2IsGrayish = Math.Abs(c2.R - c2.G) < 0.1f && Math.Abs(c2.G - c2.B) < 0.1f;

        // Both colors are grayish and different from each other
        if (c1IsGrayish && c2IsGrayish)
        {
            float diff = Math.Abs(c1.R - c2.R);
            if (diff > 0.05f && diff < 0.5f) return true;
        }

        // Also detect pure white/light backgrounds
        if (c1.R > 0.7f && c1.G > 0.7f && c1.B > 0.7f && c1IsGrayish)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if a pixel matches the checkered background colors
    /// </summary>
    private static bool IsCheckeredPixel(Color p, Color bg1, Color bg2)
    {
        // Check if pixel is close to either checkered color
        if (ColorsClose(p, bg1, 0.15f)) return true;
        if (ColorsClose(p, bg2, 0.15f)) return true;

        // Also remove any grayish pixels that look like background
        bool isGrayish = Math.Abs(p.R - p.G) < 0.08f && Math.Abs(p.G - p.B) < 0.08f;
        if (isGrayish && p.R > 0.6f) return true;  // Light gray/white

        return false;
    }

    private static bool ColorsClose(Color a, Color b, float tolerance)
    {
        return Math.Abs(a.R - b.R) < tolerance && Math.Abs(a.G - b.G) < tolerance && Math.Abs(a.B - b.B) < tolerance;
    }

    public static SpriteFrames BuildSpriteFrames(Dictionary<string, Texture2D[]> anims, float fps = 8.0f)
    {
        var sf = new SpriteFrames(); sf.RemoveAnimation("default");
        foreach (var a in anims)
        {
            sf.AddAnimation(a.Key);
            sf.SetAnimationSpeed(a.Key, fps);
            bool shouldLoop = !a.Key.Contains("attack") && a.Key != "die";
            sf.SetAnimationLoop(a.Key, shouldLoop);
            foreach (var t in a.Value) sf.AddFrame(a.Key, t);
            GD.Print($"Animation '{a.Key}': {a.Value.Length} frames, fps={fps}, loop={shouldLoop}");
        }
        return sf;
    }

    private static SpriteFrames CreateFallbackSprites(Color c, bool p)
    {
        var tex = CreateColoredRect(40, 50, c);
        var anims = p ? new Dictionary<string, Texture2D[]> { { "idle", new[] { tex } }, { "run", new[] { tex } }, { "attack", new[] { tex } } }
                      : new Dictionary<string, Texture2D[]> { { "walk", new[] { tex } }, { "attack", new[] { tex } } };
        return BuildSpriteFrames(anims);
    }

    public static ImageTexture CreateColoredRect(int w, int h, Color c)
    {
        var img = Image.CreateEmpty(w, h, false, Image.Format.Rgba8); img.Fill(c);
        return ImageTexture.CreateFromImage(img);
    }

    /// <summary>
    /// Xóa cache để force reload sprites mới
    /// </summary>
    public static void ClearCache()
    {
        _cachedPlayerFrames = null;
    }

    /// <summary>
    /// Tạo animation chạy từ sprite idle bằng cách nghiêng và dịch chuyển
    /// </summary>
    private static List<Texture2D> CreateRunAnimationFromIdle(Image sourceImage, Rect2I charRect)
    {
        var frames = new List<Texture2D>();

        // Cắt nhân vật ra
        int cw = charRect.Size.X;
        int ch = charRect.Size.Y;
        var charImg = Image.CreateEmpty(cw, ch, false, Image.Format.Rgba8);

        for (int x = 0; x < cw; x++)
        {
            for (int y = 0; y < ch; y++)
            {
                int sx = charRect.Position.X + x;
                int sy = charRect.Position.Y + y;
                if (sx >= 0 && sy >= 0 && sx < sourceImage.GetWidth() && sy < sourceImage.GetHeight())
                {
                    charImg.SetPixel(x, y, sourceImage.GetPixel(sx, sy));
                }
            }
        }

        // Tạo 6 frames với các biến đổi khác nhau để giả lập chạy
        // Frame 0: nghiêng phải nhẹ + dịch xuống
        // Frame 1: thẳng + dịch lên
        // Frame 2: nghiêng trái nhẹ + dịch xuống
        // Frame 3: thẳng + dịch lên (lặp lại pattern)
        // Frame 4: nghiêng phải
        // Frame 5: nghiêng trái

        float[] rotations = { 0.05f, 0.0f, -0.05f, 0.0f, 0.03f, -0.03f };
        int[] yOffsets = { 3, -2, 3, -2, 1, 1 };
        int[] xOffsets = { 2, 0, -2, 0, 1, -1 };

        int frameSize = 240;

        for (int i = 0; i < 6; i++)
        {
            var frame = Image.CreateEmpty(frameSize, frameSize, false, Image.Format.Rgba8);

            // Tính toán vị trí đặt nhân vật (căn giữa dưới)
            int destX = (frameSize - cw) / 2 + xOffsets[i];
            int destY = frameSize - ch - 5 + yOffsets[i];

            // Copy với biến đổi nhẹ
            float rotation = rotations[i];
            float cosR = Mathf.Cos(rotation);
            float sinR = Mathf.Sin(rotation);
            int centerX = cw / 2;
            int centerY = ch;  // Rotate around feet

            for (int x = 0; x < cw; x++)
            {
                for (int y = 0; y < ch; y++)
                {
                    // Apply rotation around center bottom
                    int rx = x - centerX;
                    int ry = y - centerY;
                    int newX = (int)(rx * cosR - ry * sinR + centerX);
                    int newY = (int)(rx * sinR + ry * cosR + centerY);

                    if (newX >= 0 && newX < cw && newY >= 0 && newY < ch)
                    {
                        Color pixel = charImg.GetPixel(newX, newY);
                        if (pixel.A > 0.1f)
                        {
                            int fx = destX + x;
                            int fy = destY + y;
                            if (fx >= 0 && fx < frameSize && fy >= 0 && fy < frameSize)
                            {
                                frame.SetPixel(fx, fy, pixel);
                            }
                        }
                    }
                }
            }

            frames.Add(ImageTexture.CreateFromImage(frame));
        }

        return frames;
    }
}
