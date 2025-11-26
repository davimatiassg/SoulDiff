using Godot;
using System;

public partial class ArcherBody : AnyBody
{
    [Export]
    public float speed = 300.0f;

    [Export]
    public float acel = 400.0f;

    [Export]
    public int arrowDamage = 4;

    [Export]
    public float projectileSpeed = 3200f;

    [Export]
    public float arrowCooldown = 1f;
    private bool canShoot = true;

    [Export]
    public float dashForce = 4f;

    [Export]
    public float dashCooldown = 3f;

    private bool canDash;

    [Export] private PackedScene arrowPrefab;
    [Export] private Node2D bow;
    [Export] private Marker2D bowTip;

    [Export] private AnimationPlayer anim;

    private bool attacking = false;
    private bool moving;
    public override void Move(Vector2 direction)
    {
        base.Move(direction);

        bool movingToggled = direction.LengthSquared() != 0 != moving;
        if (movingToggled) moving = !moving;

        if (attacking || stunned) return;


        if (movingToggled)
        {
            anim.Play("RESET");
            anim.Play(moving ? "walk" : "idle");
        }

    }
    public override void Aim(Vector2 direction)
    {
        base.Aim(direction);
        bow.LookAt(bow.GlobalPosition + direction);
    }

    public override void Button1(bool pressed)
    {
        if (!pressed || !canShoot) return;
        anim.Play("attack");

        attacking = true;
        canShoot = false;
        var _atkTween = CreateTween();
        _atkTween.TweenInterval(arrowCooldown);
        _atkTween.TweenCallback(Callable.From(() => canShoot = true));


        float spd = speed;
        float a = acel;



        Tween dashCD = CreateTween();
        dashCD.TweenInterval(dashCooldown);
        dashCD.TweenCallback(Callable.From(() => canDash = true));

        Tween dashMaker = CreateTween();
        dashMaker.TweenMethod(Callable.From((float f) =>
        {
            speed = spd * f;
            acel = a * f;
        }), 0f, 1f, 0.2f);
    }


    public override void Button2(bool pressed)
    {
        if (!pressed || !canDash || attacking || stunned) return;

        canDash = false;


        float spd = speed;
        float a = acel;
        Vector2 dir = lastMoveDirection;



        Tween dashCD = CreateTween();
        dashCD.TweenInterval(dashCooldown);
        dashCD.TweenCallback(Callable.From(() => canDash = true));

        Tween dashMaker = CreateTween();
        dashMaker.TweenMethod(Callable.From((float f) =>
        {
            speed = spd * f;
            acel = a * f;
            moveDirection = dir;
        }), dashForce, 1f, 0.2f);

    }

    public override void Button3(bool pressed)
    {
    }


    public void Shoot()
    {
        if (stunned) return;

        MagicBolt arrow = (MagicBolt)EffectPool.SpawnEffect(arrowPrefab, GetParent<Node2D>());
        arrow.GlobalPosition = bowTip.GlobalPosition;
        arrow.playerEffect = isPlayer;
        arrow.velocity = aimDirection * projectileSpeed;
        arrow.damage = arrowDamage;

        attacking = false;
        moving = false;
        anim.Play("RESET");
    }


    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (stunned) { MoveAndSlide(); return; }

        Vector2 currentVelocity = Velocity;
        currentVelocity = (moveDirection * speed);

        Velocity = currentVelocity;

        MoveAndSlide();
    }


    public override void Die()
    {
        anim.Play("hurt");
        PlayerController.Disembody(this);

    }

}
