using Godot;
using System;
using System.Collections.Generic;

public partial class Intro : Control
{
	private TextureRect _bgRect;
	private Texture2D[] _bgTextures;
	private AudioStreamPlayer _audioPlayer;

	private int _currentLine = 0;
	private float _timer = 0;
	private float _totalDuration = 48f;
	private float _introTimer = 0f;

	// Thời lượng hiển thị cho từng câu (giây) để chuyển Scene nền khớp với giọng đọc
	private float[] _lineDurations = new float[] { 6.8f, 6.5f, 6.5f, 6.5f, 6.5f, 6.5f, 8.0f };

	public override void _Ready()
	{
		_bgRect = GetNode<TextureRect>("BackgroundRect");
		_audioPlayer = GetNode<AudioStreamPlayer>("IntroAudio");

		// Load 6 hình nền cho Intro
		_bgTextures = new Texture2D[6];
		for (int i = 0; i < 6; i++)
		{
			_bgTextures[i] = GD.Load<Texture2D>($"res://Assets/Sprites/Backgrounds/{i + 1}.jpeg");
		}

		_bgRect.Modulate = new Color(1, 1, 1, 0);

		// Phát lời thoại
		_audioPlayer.Play();

		// Bắt đầu chuỗi chuyển cảnh
		ShowNextLine();
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		_timer += dt;
		_introTimer += dt;

		// Nếu hết tiếng hoặc hết thời gian thì vào game
		if (_introTimer >= _totalDuration || (_introTimer > 5f && !_audioPlayer.Playing))
		{
			StartGame();
		}
	}

	public override void _Input(InputEvent @event) { }

	private void ShowNextLine()
	{
		if (_currentLine >= _lineDurations.Length) return;

		float duration = _lineDurations[_currentLine];

		// 6 ảnh cho 7 đoạn thoại -> đoạn cuối giữ nguyên ảnh cuối
		int bgIndex = Math.Min(_currentLine, 5);
		Texture2D nextBg = _bgTextures[bgIndex];

		_currentLine++;

		// 🟢 Tween cho BACKGROUND (Chuyển Scene mượt mà)
		if (_bgRect.Texture != nextBg)
		{
			var bgTw = CreateTween();
			bgTw.TweenProperty(_bgRect, "modulate:a", 0.5f, 0.5f);
			bgTw.TweenCallback(Callable.From(() => _bgRect.Texture = nextBg));
			bgTw.TweenProperty(_bgRect, "modulate:a", 1.0f, 1.0f);
		}

		// Tự động chuyển slide theo thời gian đã định
		GetTree().CreateTimer(duration).Timeout += () => ShowNextLine();
	}

	private void StartGame()
	{
		SetProcess(false);
		GameManager.Instance.StartGame();
	}
}
