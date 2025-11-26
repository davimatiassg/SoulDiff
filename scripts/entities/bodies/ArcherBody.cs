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
    private bool canShoot;

    [Export]
    public float dashForce = 4f;

    [Export]
    public float dashCooldown = 3f;

    private bool canDash;

    [Export] private PackedScene arrowPrefab;
    [Export] private Node2D bow;
    [Export] private Marker2D bowTip;

    [Export] private AnimationPlayer anim;

    public override void Aim(Vector2 direction)
    {
        base.Aim(direction);
        bow.LookAt(GlobalPosition + direction);
    }

    public override void Button1(bool pressed)
    {
        if (!pressed || !canShoot) return;
        anim.Play("attack");

        canShoot = false;
        var _atkTween = CreateTween();
        _atkTween.TweenInterval(arrowCooldown);
        _atkTween.TweenCallback(Callable.From(() => canShoot = true));
    }


    public override void Button2(bool pressed)
    {
        if (!pressed || !canDash || stunned) return;

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

    }
}
