using Godot;

public partial class HUD : CanvasLayer
{
    private ProgressBar _healthBar;
    private Label _scoreLabel;
    private Label _levelLabel;
    private Label _livesLabel; // Label hiển thị số mạng
    private Label _countdownLabel;
    private Panel _pausePanel;
    private Button _pauseButton;
    private Button _resumeButton;
    private Button _exitButton;
    private Label _keysLabel; // Label hiển thị số chìa khóa
    private Player _player;

    public override void _Ready()
    {
        // Cho phép HUD chạy kể cả khi Game bị Pause
        ProcessMode = ProcessModeEnum.Always;

        _healthBar = GetNode<ProgressBar>("MarginContainer/HBoxContainer/HealthBar");
        _scoreLabel = GetNode<Label>("MarginContainer/HBoxContainer/ScoreLabel");
        _levelLabel = GetNode<Label>("MarginContainer/HBoxContainer/LevelLabel");

        // Ẩn trái tim thừa phía trước
        if (HasNode("MarginContainer/HBoxContainer/HealthIcon"))
            GetNode<Control>("MarginContainer/HBoxContainer/HealthIcon").Visible = false;

        var mainHBox = GetNode<BoxContainer>("MarginContainer/HBoxContainer");

        // Tạo VBox để chứa Thanh Máu và Mạng (xếp chồng lên nhau)
        VBoxContainer healthSection = new VBoxContainer();
        healthSection.AddThemeConstantOverride("separation", 2);
        mainHBox.AddChild(healthSection);
        mainHBox.MoveChild(healthSection, 0);

        // Chuyển Thanh Máu vào VBox
        _healthBar.GetParent().RemoveChild(_healthBar);
        healthSection.AddChild(_healthBar);

        // ── 0. HIỂN THỊ MẠNG (Lives) ──────────────────────────
        _livesLabel = new Label();
        _livesLabel.AddThemeFontSizeOverride("font_size", 18);
        _livesLabel.HorizontalAlignment = HorizontalAlignment.Left; // Căn lề trái
        _livesLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f)); 
        _livesLabel.AddThemeConstantOverride("outline_size", 3);
        _livesLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
        
        healthSection.AddChild(_livesLabel);

        // ── 0.1 HIỂN THỊ CHÌA KHÓA (Keys) ───────────────────
        _keysLabel = new Label();
        _keysLabel.AddThemeFontSizeOverride("font_size", 18);
        _keysLabel.HorizontalAlignment = HorizontalAlignment.Left;
        _keysLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.2f)); // Màu vàng kim
        _keysLabel.AddThemeConstantOverride("outline_size", 3);
        _keysLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
        healthSection.AddChild(_keysLabel);

        // ── 1. NÚT DỪNG GAME (Pause) ──────────────────────────
        _pauseButton = new Button();
        _pauseButton.Text = "⏸️ DỪNG GAME";
        _pauseButton.CustomMinimumSize = new Vector2(120, 45);
        _pauseButton.Pressed += () => GameManager.Instance.TogglePause();
        GetNode<BoxContainer>("MarginContainer/HBoxContainer").AddChild(_pauseButton);

        // ── 2. MENU TẠM DỪNG ──────────────────────────────────
        if (HasNode("PausePanel"))
        {
            _pausePanel = GetNode<Panel>("PausePanel");
            _pausePanel.Visible = false;
            _pausePanel.CustomMinimumSize = new Vector2(450, 350); // Tăng kích thước

            // Thêm background styling cho Panel
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Nền đen mờ
            panelStyle.BorderWidthLeft = 3;
            panelStyle.BorderWidthRight = 3;
            panelStyle.BorderWidthTop = 3;
            panelStyle.BorderWidthBottom = 3;
            panelStyle.BorderColor = new Color(0.8f, 0.6f, 0.2f, 1.0f); // Viền vàng
            panelStyle.SetCornerRadiusAll(15);
            _pausePanel.AddThemeStyleboxOverride("panel", panelStyle);

            // Tạo VBoxContainer để layout các nút theo hàng dọc
            var pauseVBox = new VBoxContainer();
            pauseVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            pauseVBox.Alignment = BoxContainer.AlignmentMode.Center;
            pauseVBox.AddThemeConstantOverride("separation", 25); // Tăng khoảng cách
            pauseVBox.AddThemeConstantOverride("margin_left", 30);
            pauseVBox.AddThemeConstantOverride("margin_right", 30);
            pauseVBox.AddThemeConstantOverride("margin_top", 30);
            pauseVBox.AddThemeConstantOverride("margin_bottom", 30);
            _pausePanel.AddChild(pauseVBox);

            // Tiêu đề Tạm Dừng
            var titleLabel = new Label();
            titleLabel.Text = "⏸️ TẠM DỪNG";
            titleLabel.AddThemeFontSizeOverride("font_size", 40);
            titleLabel.AddThemeColorOverride("font_color", Colors.Yellow);
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            pauseVBox.AddChild(titleLabel);

            // Nút Tiếp tục
            _resumeButton = new Button();
            _resumeButton.Text = "▶️ TIẾP TỤC";
            _resumeButton.CustomMinimumSize = new Vector2(300, 60);
            _resumeButton.AddThemeFontSizeOverride("font_size", 24);
            
            // Style nút
            var buttonStyleNormal = new StyleBoxFlat();
            buttonStyleNormal.BgColor = new Color(0.2f, 0.6f, 0.2f, 1.0f); // Xanh
            buttonStyleNormal.SetCornerRadiusAll(8);
            
            var buttonStyleHover = new StyleBoxFlat();
            buttonStyleHover.BgColor = new Color(0.3f, 0.8f, 0.3f, 1.0f); // Xanh sáng
            buttonStyleHover.SetCornerRadiusAll(8);
            
            _resumeButton.AddThemeStyleboxOverride("normal", buttonStyleNormal);
            _resumeButton.AddThemeStyleboxOverride("hover", buttonStyleHover);
            _resumeButton.AddThemeStyleboxOverride("pressed", buttonStyleHover);
            _resumeButton.Pressed += StartResumeCountdown;
            pauseVBox.AddChild(_resumeButton);

            // Nút Thoát Game
            _exitButton = new Button();
            _exitButton.Text = "❌ THOÁT GAME";
            _exitButton.CustomMinimumSize = new Vector2(300, 60);
            _exitButton.AddThemeFontSizeOverride("font_size", 24);
            
            // Style nút thoát
            var exitStyleNormal = new StyleBoxFlat();
            exitStyleNormal.BgColor = new Color(0.8f, 0.2f, 0.2f, 1.0f); // Đỏ
            exitStyleNormal.SetCornerRadiusAll(8);
            
            var exitStyleHover = new StyleBoxFlat();
            exitStyleHover.BgColor = new Color(1.0f, 0.3f, 0.3f, 1.0f); // Đỏ sáng
            exitStyleHover.SetCornerRadiusAll(8);
            
            _exitButton.AddThemeStyleboxOverride("normal", exitStyleNormal);
            _exitButton.AddThemeStyleboxOverride("hover", exitStyleHover);
            _exitButton.AddThemeStyleboxOverride("pressed", exitStyleHover);
            _exitButton.Pressed += () => GetTree().Quit();
            pauseVBox.AddChild(_exitButton);
        }

        // ── 3. NHÃN ĐẾM NGƯỢC (Countdown) ──────────────────────
        _countdownLabel = new Label();
        _countdownLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _countdownLabel.VerticalAlignment = VerticalAlignment.Center;
        _countdownLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _countdownLabel.GrowHorizontal = Control.GrowDirection.Both;
        _countdownLabel.GrowVertical = Control.GrowDirection.Both;
        _countdownLabel.AddThemeFontSizeOverride("font_size", 120);
        _countdownLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
        _countdownLabel.AddThemeConstantOverride("outline_size", 15);
        _countdownLabel.Visible = false;
        AddChild(_countdownLabel);

        UpdateUI();
    }

    private void StartResumeCountdown()
    {
        _pausePanel.Visible = false;
        _countdownLabel.Visible = true;
        
        var tw = CreateTween();
        // Đếm 3 - 2 - 1
        tw.TweenCallback(Callable.From(() => _countdownLabel.Text = "3")).SetDelay(0);
        tw.TweenCallback(Callable.From(() => _countdownLabel.Text = "2")).SetDelay(1.0f);
        tw.TweenCallback(Callable.From(() => _countdownLabel.Text = "1")).SetDelay(1.0f);
        tw.TweenCallback(Callable.From(() => _countdownLabel.Text = "BẮT ĐẦU!")).SetDelay(1.0f);
        
        tw.TweenCallback(Callable.From(() => 
        {
            _countdownLabel.Visible = false;
            GameManager.Instance.TogglePause(); // Thực sự nhả game ra
        })).SetDelay(0.5f);
    }

    public override void _Process(double delta)
    {
        // Find player if not found or disposed
        if (!IsInstanceValid(_player))
        {
            var playerNode = GetTree().GetFirstNodeInGroup("player");
            if (playerNode is Player p)
            {
                _player = p;
                _player.HealthChanged += OnHealthChanged;
            }
        }

        UpdateUI();

        if (_pausePanel != null && !_countdownLabel.Visible)
        {
            // Chỉ hiện PausePanel nếu không đang trong quá trình đếm ngược
            if (_pausePanel.Visible != GameManager.Instance.IsPaused)
                _pausePanel.Visible = GameManager.Instance.IsPaused;
        }
    }

    private void UpdateUI()
    {
        if (_healthBar != null)
        {
            _healthBar.MaxValue = GameManager.Instance.MaxPlayerHealth;
            _healthBar.Value = GameManager.Instance.PlayerHealth;

            // Set màu cho thanh máu
            float percent = (float)GameManager.Instance.PlayerHealth / GameManager.Instance.MaxPlayerHealth;
            Color color;
            if (percent > 0.66f)
            {
                color = Colors.Green;
            }
            else if (percent > 0.33f)
            {
                color = Colors.Yellow;
            }
            else
            {
                color = Colors.Red;
            }
            _healthBar.Modulate = color;
        }

        if (_scoreLabel != null)
        {
            _scoreLabel.Text = $"Điểm: {GameManager.Instance.Score}";
        }

        if (_levelLabel != null)
        {
            string levelName = GameManager.Instance.CurrentLevel switch
            {
                1 => "Đường Rừng Hiểm Trở",
                2 => "Hang Tối Hiểm Nguy",
                3 => "Đại Chiến Chằn Tinh",
                _ => $"Level {GameManager.Instance.CurrentLevel}"
            };
            _levelLabel.Text = levelName;
        }

        if (_livesLabel != null)
        {
            string hearts = "";
            for (int i = 0; i < GameManager.Instance.PlayerLives; i++) hearts += "❤️ ";
            _livesLabel.Text = hearts.Trim();
        }

        if (_keysLabel != null)
        {
            _keysLabel.Text = $"🗝️ Chìa khóa: {GameManager.Instance.TotalKeys} / 3";
            // Chỉ hiện số chìa khóa khi đã nhặt được ít nhất 1 chìa
            _keysLabel.Visible = GameManager.Instance.TotalKeys > 0;
        }
    }

    private void OnHealthChanged(int newHealth, int maxHealth)
    {
        if (_healthBar != null)
        {
            _healthBar.MaxValue = maxHealth;
            _healthBar.Value = newHealth;

            // Chia thành 3 tầng màu dựa trên ngưỡng máu
            float percent = (float)newHealth / maxHealth;
            Color color;
            if (percent > 0.66f)
            {
                color = Colors.Green; // Xanh lá khi >66%
            }
            else if (percent > 0.33f)
            {
                color = Colors.Yellow; // Vàng khi 33-66%
            }
            else
            {
                color = Colors.Red; // Đỏ khi <33%
            }
            _healthBar.Modulate = color;
        }
    }
}
