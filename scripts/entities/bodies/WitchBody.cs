using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class WitchBody : EnemyBody
{
    [Export] private AnimationPlayer anim;
    [Export] private Sprite2D wand;
    [Export] private Node2D wandTip;
    [Export] private Sprite2D broom;

    [Export] private PackedScene boltPrefab;
    [Export] private PackedScene fireballPrefab;

    [ExportGroup("Balance Variables")]

    [Export]
    public float speed = 128.0f;


    [Export]
    public int attackDamage = 3;

    [Export]
    public float projectileSpeed = 3200f;

    [Export]
    public float attackCooldown = 0.1f;

    [Export]
    public float fireballCooldown = 4f;


    // inner variables
    public virtual bool StartWithDefaultController { get => true; }
    public override AnyController DefaultController => new MeleeAIController();

    public override void Move(Vector2 direction)
    {
        base.Move(direction);
        broom.LookAt(broom.GlobalPosition + direction);
        
    }
    public override void Aim(Vector2 direction)
    {
        base.Aim(direction);
        wand.LookAt(wand.GlobalPosition + direction);
    }


    Action attackAction = null;

    public override void Button1(bool pressed)
    {
        if (pressed) { attackAction = CastBolt; }
        else { attackAction = null;  }
    }

    Tween _atkTween;
    bool canAttack = true;
    public void CastBolt()
    {
        if (!canAttack || stunned) return;

        var bolt = (MagicBolt)EffectPool.SpawnEffect(boltPrefab, GetParent());
        bolt.GlobalPosition = wandTip.GlobalPosition;
        bolt.playerEffect = isPlayer;
        bolt.velocity = aimDirection*projectileSpeed;
        bolt.damage = attackDamage;

        canAttack = false;
        _atkTween = CreateTween();
        _atkTween.TweenInterval(attackCooldown);
        _atkTween.TweenCallback(Callable.From(() => canAttack = true));

    }


    public override void Button2(bool pressed)
    {
        //TODO!
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (stunned) { MoveAndSlide(); return; }

        attackAction?.Invoke();

        Vector2 currentVelocity = Velocity;
        currentVelocity = (moveDirection * speed);

        Velocity = currentVelocity;

        MoveAndSlide();
    }


}  