using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class KnightBody : EnemyBody
{
    [Export] private AnimationPlayer anim;
    [Export] private PackedScene slashPrefab;

    [Export] private PackedScene shockwavePrefab;

    [ExportGroup("Balance Variables")]

    [Export]
    public float speed = 128.0f;


    [Export]
    public int attackDamage = 3;
    [Export]
    public float attackPushForce = 32f;
    [Export]
    public float attackCooldown = 0.4f;
    [Export]
    public float attackMoveSpeed = 32f;

    private bool attacking = false;

    [Export]
    public float shieldCooldown = 4f;
    [Export]
    public float shieldMoveSpeed = 0f;

    [Export]
    private bool shielding = false;


    // inner variables

    public override AnyController DefaultController => new MeleeAIController();

    private bool moving;
    public override void Move(Vector2 direction)
    {
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
        base.Aim(direction);
    }


    Tween attackTween;
    private bool canAttack = true;

    public override void Button1(bool pressed)
    {
        if (!canAttack || attacking || stunned) return;




        if (shielding) Unshield();
        anim.Play("RESET");
        anim.Play("attack");
        attacking = true;
        var temp = attackMoveSpeed;
        attackMoveSpeed = speed;
        speed = temp;

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

            var temp = attackMoveSpeed;
            attackMoveSpeed = speed;
            speed = temp;
            moving = false;

        };

        anim.AnimationFinished += stopAtkAction;

    }
    /// <summary>
    /// called by the animation player, a child from the knight node.
    /// </summary>
    public void SwingSword()
    {
        var slash = (SwordSlash)EffectPool.SpawnEffect(slashPrefab, GetParent());
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
        if (attacking || stunned || !canShield) return;
        if (pressed) Shield();
        else Unshield();
    }


    
    Tween shieldTween;
    private void Shield()
    {
        if (shielding) return;

        var temp = shieldMoveSpeed;
        shieldMoveSpeed = speed;
        speed = temp;

        anim.Play("RESET");
        anim.Play("def");
        shielding = true;
    }
    private void Unshield()
    {
        if (!shielding) return;

        var temp = shieldMoveSpeed;
        shieldMoveSpeed = speed;
        speed = temp;

        
        anim.Play("RESET");
        shielding = false;
    }


    public override void TakeDamage(int damage, Vector2 knockback)
    {
        if (shielding)
        {
            Unshield();
            var wave = (Shockwave)EffectPool.SpawnEffect(shockwavePrefab, GetParent());
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



    public override void HitstunApply()
    {
        base.HitstunApply();
        anim.Play("RESET");
        anim.Play("hurt");
    }

    public override void HitstunCleanse()
    {
        base.HitstunCleanse();
        anim.Play("RESET");
        moving = false;
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


}  