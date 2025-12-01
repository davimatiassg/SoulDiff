using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class MinotaurBody : EnemyBody
{
    [Export] private PackedScene slashPrefab;

    [ExportGroup("Balance Variables")]

    [Export]
    public float baseMoveSpeed = 96.0f;




    [Export]
    public int attackDamage = 10;
    [Export]
    public float attackPushForce = 320f;
    [Export]
    public float attackCooldown = 1f;
    [Export]
    public float attackMoveSpeed = 0f;

    private bool attacking = false;
    
    [ExportGroup("Balance Variables/Rage")]

    [Export]
    public float rageAttackPushForce = 640f;

    [Export]
    public float rageAttackCooldown = 0.5f;

    [Export]
    public float rageCooldown = 6f;
    [Export]
    public float rageDuration = 3f;
    [Export]
    public float rageMoveSpeed = 0f;



    public float speed = 96;

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

    // inner variables

    public override void _Ready()
    {
        base._Ready();
        Raging = false;
    }

    public override AnyController DefaultController => new MeleeAIController();

    private bool moving;
    public override void Move(Vector2 direction)
    {
        if (dead) return;
        base.Move(direction);

        bool movingToggled = (direction.LengthSquared() != 0 != moving);
        if (movingToggled) moving = !moving;

        if (attacking || stunned) return;


        if (movingToggled)
        { 
            anim.Play(moving ? "walk" : "idle");
        }
        
    }
    public override void Aim(Vector2 direction)
    {
        if (dead) return;
        base.Aim(direction);
    }


    Tween attackTween;
    private bool canAttack = true;

    public override void Button1(bool pressed)
    {
        if (dead || !canAttack || attacking || stunned) return;



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
        if(sprite.Scale.Y < 0) { slash.Scale = new Vector2(1, -1); }

        slash.playerEffect = isPlayer;
        slash.knockback = attackPushForce;
        slash.direction = aimDirection;
        slash.damage = attackDamage;
    }

    public void EndAttack()
    {
        attacking = false;
        speed = baseMoveSpeed;
        moving = false;

        attackTween = CreateTween();
        attackTween.TweenInterval(Raging ? rageAttackCooldown : attackCooldown);
        attackTween.TweenCallback(Callable.From(() => { canAttack = true; }));
    }

    private bool canRage = true;
    Tween rageTracker;
    public override void Button2(bool pressed)
    {
        if (dead || stunned || Raging || !canRage) return;

        rageTracker = CreateTween();
        rageTracker.TweenCallback(Callable.From(() =>
        {
            Raging = true;
            float temp = attackCooldown;
            attackCooldown = rageAttackCooldown;
            rageAttackCooldown = temp;

            temp = attackPushForce;
            attackPushForce = rageAttackPushForce;
            rageAttackPushForce = temp;

            temp = speed;
            speed = rageMoveSpeed;
            rageMoveSpeed = temp;

        }));
        rageTracker.TweenInterval(rageDuration);
        rageTracker.TweenCallback(Callable.From(() =>
        {
            Raging = false;
            canRage = false;

            float temp = attackCooldown;
            attackCooldown = rageAttackCooldown;
            rageAttackCooldown = temp;

            temp = attackPushForce;
            attackPushForce = rageAttackPushForce;
            rageAttackPushForce = temp;

            temp = speed;
            speed = rageMoveSpeed;
            rageMoveSpeed = temp;

        }));
        rageTracker.TweenInterval(rageCooldown);
        rageTracker.TweenCallback(Callable.From(() => canRage = true ));

    }


    

    public override void TakeDamage(int damage, Vector2 knockback)
    {
        attacking = false;
        base.TakeDamage(damage, knockback);

    }



    public override void HitstunApply()
    {
        base.HitstunApply();
        if (Raging) HitstunCleanse();
        else { anim.Play("hurt"); }
    }

    public override void HitstunCleanse()
    {
        base.HitstunCleanse();
        moving = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (dead || stunned) { MoveAndSlide(); return; }

        Vector2 currentVelocity = Velocity;
        currentVelocity = (moveDirection * speed);

        Velocity = currentVelocity;

        MoveAndSlide();
    }


}  