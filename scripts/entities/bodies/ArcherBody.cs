using Godot;
using System;

public partial class ArcherBody : AnyBody
{

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

    private bool canDash = true;

    [Export] private PackedScene arrowPrefab;
    [Export] private Node2D bow;
    [Export] private Marker2D bowTip;

    [Export] private AnimationPlayer anim;

    private bool attacking = false;
    private bool moving;

    Tween moveSoundTween;
    public override void Move(Vector2 direction)
    {
        if (stunned || dead) return;
        base.Move(direction);

        bool movingToggled = direction.LengthSquared() != 0 != moving;
        if (movingToggled) moving = !moving;

        if (attacking || stunned) return;


        if (movingToggled)
        {
            if(moveSoundTween != null) moveSoundTween.Kill(); 
            if (moving) moveSoundTween = AudioPlayer.PlayRandomContinuousSound("sfx_human_step", this); 
            
            anim.Play("RESET");
            anim.Play(moving ? "walk" : "idle");
        }

    }
    public override void Aim(Vector2 direction)
    {
        if (!canShoot || stunned || dead) return;
        base.Aim(direction);
        bow.LookAt(bow.GlobalPosition + direction);
    }

    public override void Button1(bool pressed)
    {
        if (!canShoot || stunned || dead) return;

        anim.Play("attack");

        attacking = true;
        canShoot = false;
        var _atkTween = CreateTween();
        _atkTween.TweenInterval(arrowCooldown);
        _atkTween.TweenCallback(Callable.From(() => canShoot = true));

        if(isPlayer) HudManager.TriggerCooldown(1,arrowCooldown);

        float spd = speed;
        float a = acel;


        AudioPlayer.PlaySound("sfx_bow_pull", this, false, 2);
        Tween dashCD = CreateTween();
        dashCD.TweenInterval(0.2f);
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
        if (dead || !pressed || !canDash || attacking || stunned) return;

        canDash = false;


        float spd = speed;
        float a = acel;

        Tween dashCD = CreateTween();
        dashCD.TweenInterval(dashCooldown);
        dashCD.TweenCallback(Callable.From(() => canDash = true));

        if(isPlayer) HudManager.TriggerCooldown(2,dashCooldown);

        Tween dashMaker = CreateTween();
        dashMaker.TweenMethod(Callable.From((float f) =>
        {
            speed = spd * f;
            acel = a * f;
        }), dashForce, 1f, 0.2f);

    }

    public override void Button3(bool pressed)
    { }


    public void Shoot()
    {


        MagicBolt arrow = (MagicBolt)EffectPool.SpawnEffect(arrowPrefab, GetParent<Node2D>());
        arrow.GlobalPosition = bowTip.GlobalPosition;
        arrow.playerEffect = isPlayer;
        arrow.velocity = aimDirection * projectileSpeed;
        arrow.damage = arrowDamage;

        attacking = false;
        moving = false;
        anim.Play("RESET");
        AudioPlayer.PlaySoundRandomPitch("sfx_bow_shot", this);
    }

    public override void KnockbackApply(Vector2 knockbac)
    { 
        
    }

    public override void TakeDamage(int damage, Vector2 knockback)
    {
        base.TakeDamage(damage, knockback);        
    }
    public override void Die()
    {
        anim.Play("hurt");
        stunned = true;
        dead = true;
        moveSoundTween.Kill();

        CollisionLayer = 1 << 0;
        CollisionMask = CollisionLayer;


        if (!isPlayer) return;
        
        SequenceManager.OnPlayerDie();
        MainCamera.CameraShake(5, 0.5f);
        AudioPlayer.PlaySound("sfx_human_ouch", this);
    }

}
