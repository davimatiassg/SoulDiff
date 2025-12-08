using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Class that defines the Witch Enemy
/// </summary>
public partial class WitchBody : EnemyBody
{
    [ExportGroup("Connections")]
    [Export] private Sprite2D wand;
    [Export] private Node2D wandTip;
    [Export] private Sprite2D broom;
    [Export] private PackedScene boltPrefab;
    [Export] private PackedScene fireballPrefab;

    [ExportGroup("Balance Variables")]
    [Export] public int attackDamage = 1;
    [Export] public float projectileSpeed = 3200f;
    [Export] public float attackCooldown = 0.1f;
    [Export] public float fireballDamage = 10f;
    [Export] public float fireballCooldown = 4f;
    [Export] public float fireballCharge = 2f;

    //# Controlling

    public override void Move(Vector2 direction)
    {
        base.Move(direction);
        if (dead || stunned) return;
        broom.LookAt(broom.GlobalPosition + direction);
    }
    public override void Aim(Vector2 direction)
    {
        base.Aim(direction);
        if (dead || stunned) return;
        wand.LookAt(wand.GlobalPosition + direction);
    }


    //# Skills

    Action attackAction = null;
    public override void Button1(bool pressed)
    {

        if (pressed) { attackAction = CastBolt; }
        else { attackAction = null; }

    }

    Tween _atkTween;
    bool canAttack = true;
    public void CastBolt()
    {
        if (!canAttack || stunned) return;

        var bolt = (MagicBolt)EffectPool.SpawnEffect(boltPrefab, GetParent<Node2D>());
        bolt.GlobalPosition = wandTip.GlobalPosition;
        bolt.playerEffect = isPlayer;
        bolt.velocity = aimDirection*projectileSpeed;
        bolt.damage = attackDamage;

        if(isPlayer) HudManager.TriggerCooldown(1, attackCooldown);

        canAttack = false;
        _atkTween = CreateTween();
        _atkTween.TweenInterval(attackCooldown);
        _atkTween.TweenCallback(Callable.From(() => canAttack = true));

    }

    Fireball curr_fireball;
    Tween fireballCharger;
    bool canFireball = true;
    public override void Button2(bool pressed)
    {
        if (stunned || !pressed || !canFireball ) return;

        attackAction = null;
        
        canAttack = false;
        Tween _atktween = CreateTween();
        _atktween.TweenInterval(fireballCharge);
        _atktween.TweenCallback(Callable.From(() => canAttack = true));

        canFireball = false;
        Tween _fireballtween = CreateTween();
        _fireballtween.TweenInterval(fireballCharge + fireballCooldown);
        _fireballtween.TweenCallback(Callable.From(() => canFireball = true));

        if(isPlayer) HudManager.TriggerCooldown(2, fireballCharge + fireballCooldown);

        curr_fireball = (Fireball)EffectPool.SpawnEffect(fireballPrefab, GetParent<Node2D>());
        curr_fireball.Position = Position;
        curr_fireball.playerEffect = isPlayer;
        curr_fireball.StartOrbit(this);
        curr_fireball.chargeTime = fireballCharge;
        

        fireballCharger = CreateTween();
        fireballCharger.TweenInterval(fireballCharge);
        fireballCharger.TweenCallback(Callable.From(() =>
        {
            curr_fireball.Fling(aimDirection);
            
        }));
    }


    //# Inherited Methods

    public override void HitstunCleanse()
    {
        base.HitstunCleanse();
        anim.Play("RESET");
    }
    
    public override void Die()
    {
        base.Die();
        attackAction = null;
    }

    //# Processes
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        attackAction?.Invoke();
    }


}  