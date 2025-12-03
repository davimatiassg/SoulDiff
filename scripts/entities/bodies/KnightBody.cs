using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class KnightBody : EnemyBody
{
    [ExportGroup("Connections")]
    
    [Export] private PackedScene slashPrefab;
    [Export] private PackedScene shockwavePrefab;

    [ExportGroup("Balance Variables")]

    [Export] public float baseMoveSpeed = 128f;
    [Export] public int attackDamage = 3;
    [Export] public float attackPushForce = 32f;
    [Export] public float attackCooldown = 0.4f;
    [Export] public float attackMoveSpeed = 32f;
    [Export] public float shieldCooldown = 4f;
    [Export] public float shieldMoveSpeed = 0f;

    [ExportGroup("")]


    //# Controlling

    private bool attacking = false;
    private bool moving;
    private bool shielding = false;
    public override void Move(Vector2 direction)
    {
        if (dead || stunned) return;
        base.Move(direction);

        bool movingToggled = (direction.LengthSquared() != 0 != moving);
        if (movingToggled) moving = !moving;

        if (attacking || shielding || stunned) return;


        if (movingToggled)
        {
            anim.Play("RESET");
            anim.Play(moving ? "walk" : "idle");
        }

    }
    public override void Aim(Vector2 direction)
    {
        if (dead || stunned) return;
        base.Aim(direction);
    }

    //# Skills
    Tween attackTween;
    private bool canAttack = true;
    public override void Button1(bool pressed)
    {
        if (dead || !canAttack || attacking || stunned) return;

        if (shielding) Unshield();
        anim.Play("RESET");
        anim.Play("attack");
        attacking = true;

        speed = attackMoveSpeed;

        canAttack = false;
        attackTween = CreateTween();
        attackTween.TweenInterval(attackCooldown);
        attackTween.TweenCallback(Callable.From(() => { canAttack = true; }));

        AnimationMixer.AnimationFinishedEventHandler stopAtkAction = (StringName animName) => { };

        stopAtkAction = (StringName animName) =>
        {
            if (animName != "attack") { return; }
            anim.AnimationFinished -= stopAtkAction;
            attacking = false;
            anim.Play("RESET");

            speed = baseMoveSpeed;

        };

        anim.AnimationFinished += stopAtkAction;

    }
    /// <summary>
    /// called by the animation player, a child from the knight node.
    /// </summary>
    public void SwingSword()
    {
        var slash = (SwordSlash)EffectPool.SpawnEffect(slashPrefab, GetParent<Node2D>());
        slash.GlobalPosition = GlobalPosition + (aimDirection * 16f);
        slash.LookAt(GlobalPosition + 32 * aimDirection);
        if(sprite.Scale.Y < 0) { slash.Scale = new Vector2(1, -1); }

        slash.playerEffect = isPlayer;
        slash.knockback = attackPushForce;
        slash.direction = aimDirection;
        slash.damage = attackDamage;
    }

    private bool canShield = true;
    public override void Button2(bool pressed)
    {
        if (dead || attacking || stunned || !canShield) return;
        if (pressed) Shield();
        else Unshield();
    }
    Tween shieldTween;
    private void Shield()
    {
        if (shielding) return;

        speed = shieldMoveSpeed;

        anim.Play("RESET");
        anim.Play("def");
        shielding = true;
    }
    private void Unshield()
    {
        if (!shielding) return;

        speed = baseMoveSpeed;

        
        anim.Play("RESET");
        shielding = false;
    }

    //# Inherited Methods
    public override void TakeDamage(int damage, Vector2 knockback)
    {
        if (dead) return;
        if (shielding)
        {
            Unshield();
            var wave = (Shockwave)EffectPool.SpawnEffect(shockwavePrefab, GetParent<Node2D>());
            wave.GlobalPosition = GlobalPosition;
            wave.playerEffect = isPlayer;
            wave.knockback = knockback.Length();
            wave.damage = damage;


            canShield = false;
            shieldTween = CreateTween();
            shieldTween.TweenInterval(shieldCooldown);
            shieldTween.TweenCallback(Callable.From(() => canShield = true));

            return;
        }
        attacking = false;
        base.TakeDamage(damage, knockback);

    }


    public override void PossessStart(PlayerController playerController)
    {
        base.PossessStart(playerController);

        shielding = false;
        dead = false;
        stunned = false;
        attacking = false;
        moving = false;
        speed = baseMoveSpeed;
    }

    public override void HitstunCleanse()
    {
        base.HitstunCleanse();
        moving = false;
    }



}  