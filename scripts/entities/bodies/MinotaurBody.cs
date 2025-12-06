using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class MinotaurBody : EnemyBody
{
    [ExportGroup("Connections")]
    [Export] private PackedScene slashPrefab;

    [ExportGroup("Balance Variables")]

    [Export] public float baseMoveSpeed = 96.0f;
    [Export] public int attackDamage = 10;
    [Export] public float attackPushForce = 320f;
    [Export] public float attackCooldown = 0.5f;
    [Export] public float attackMoveSpeed = 0f;
    [Export] public int stunResistance = 2;


    [ExportGroup("Balance Variables/Rage")]

    [Export] public float rageAttackPushForce = 640f;
    [Export] public float rageAttackCooldown = 0.25f;
    [Export] public float rageCooldown = 6f;
    [Export] public float rageDuration = 3f;
    [Export] public float rageMoveSpeed = 0f;
    [Export] public int rageStunResistance = 2;
    bool _raging;
    [Export]
    protected bool Raging
    {
        get
        {
            return _raging;
        }
        set
        {
            _raging = value;
            isHitStunnable = value;
            if (shaderMat == null) return;
            if (value)
            {
                shaderMat.SetShaderParameter("aura_intensity", 0.2f);
                shaderMat.SetShaderParameter("color_pulse_strength", 0.8f);
            }
            else
            {
                shaderMat.SetShaderParameter("aura_intensity", 0f);
                shaderMat.SetShaderParameter("color_pulse_strength", 0f);
            }

        }
    }


    //# Controlling

    
    [Export] private bool attacking = false;
    [Export] private bool moving;
    public override void Move(Vector2 direction)
    {
        if (attacking || dead || stunned) return;
        base.Move(direction);

        bool movingToggled = (direction.LengthSquared() != 0 != moving);
        if (movingToggled) moving = !moving;

        if (stunned) return;


        if (movingToggled)
        {
            anim.Play(moving ? "walk" : "idle");
        }

    }

    //# Skills

    Tween attackTween;
    [Export] private bool canAttack = true;

    public override void Button1(bool pressed)
    {
        if (dead || !canAttack || attacking || stunned) return;


        anim.Play("RESET");
        anim.Play("attack");
        canAttack = false;
        attacking = true;
        speed = attackMoveSpeed;

    }
    /// <summary>
    /// called by the animation player, a child from the minotaur node.
    /// </summary>
    public void SwingAxe()
    {
        var slash = (SwordSlash)EffectPool.SpawnEffect(slashPrefab, GetParent<Node2D>());
        slash.GlobalPosition = sprite.GlobalPosition;
        slash.LookAt(GlobalPosition + 32 * aimDirection);
        if (sprite.Scale.Y < 0) { slash.Scale = new Vector2(1, -1); }

        slash.playerEffect = isPlayer;
        slash.knockback = Raging ? rageAttackPushForce : attackPushForce;
        slash.direction = aimDirection;
        slash.damage = attackDamage;
    }

    public void EndAttack()
    {
        attacking = false;
        speed = baseMoveSpeed;
        moving = false;

        if(isPlayer) HudManager.TriggerCooldown(1, Raging ? rageAttackCooldown : attackCooldown);

        attackTween = CreateTween();
        attackTween.TweenInterval(Raging ? rageAttackCooldown : attackCooldown);
        attackTween.TweenCallback(Callable.From(() => { canAttack = true; }));
    }

    private bool canRage = true;
    Tween rageTracker;
    public override void Button2(bool pressed)
    {
        if (dead || stunned || Raging || !canRage) return;

        speed = rageMoveSpeed;
        Raging = true;

        if(isPlayer) HudManager.TriggerCooldown(2,rageDuration + rageCooldown);

        rageTracker = CreateTween();
        rageTracker.TweenInterval(rageDuration);
        rageTracker.TweenCallback(Callable.From(() =>
        {
            Raging = false;
            canRage = false;
        }));
        rageTracker.TweenInterval(rageCooldown);
        rageTracker.TweenCallback(Callable.From(() => canRage = true));
    }


    //# Inherited Methods

    public override void TakeDamage(int damage, Vector2 knockback)
    {

        base.TakeDamage(damage, knockback);

        if (dead && attacking) EndAttack();
    }



    public override void HitstunApply(float damage)
    {
        if (dead) return;
        damage -= Raging ? rageStunResistance : stunResistance;
        if (damage > 0)
        {
            base.HitstunApply(damage);
            if (attacking) EndAttack();
            moving = false;
        }
        else HitstunCleanse();
        
        
    }

    public override void HitstunCleanse()
    {
        base.HitstunCleanse();
        moving = false;
    }


    public override void PossessStart(PlayerController cntrl)
    {
        base.PossessStart(cntrl);
        anim.SpeedScale = 2f;
    }
    


    public override void _Ready()
    {
        base._Ready();
        Raging = Raging; //Needed to run the code in the set method
    }

}  