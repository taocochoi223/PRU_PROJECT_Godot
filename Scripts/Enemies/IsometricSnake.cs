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
    private bool _isDead = false;
    private bool _canAttack = true;
    private Timer _attackCooldown;

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
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDead) return;

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
                Velocity = Vector2.Zero;
                if (_canAttack) Attack();
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
        _isDead = true;
        PlayAnimSafe("die", "hurt");
        CollisionLayer = 0;
        CollisionMask = 0;

        var tw = CreateTween();
        tw.TweenProperty(this, "modulate:a", 0f, 1.0f);
        tw.Chain().TweenCallback(Callable.From(() => QueueFree()));
    }
}
