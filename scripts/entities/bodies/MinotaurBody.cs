using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class MinotaurBody : EnemyBody
{
    [Export] private PackedScene slashPrefab;

    [ExportGroup("Balance Variables")]

    [Export]
    public float speed = 96.0f;


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
    public float rageAttackPushForce = 1080f;

    [Export]
    public float rageAttackCooldown = 0.5f;

    [Export]
    public float rageCooldown = 6f;
    [Export]
    public float rageDuration = 3f;
    [Export]
    public float rageMoveSpeed = 0f;





    bool _raging;
    [Export]
    protected bool raging
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
        raging = false;
    }

    public override AnyController DefaultController => new MeleeAIController();

    private bool moving;
    public override void Move(Vector2 direction)
    {
        base.Move(direction);

        bool movingToggled = (direction.LengthSquared() != 0 != moving);
        if (movingToggled) moving = !moving;

        if (attacking || stunned) return;


        if (movingToggled)
        { 
            sprite.Play(moving ? "walk" : "idle");
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


        Action swing = () => { };
        swing = () =>
        {
            if (sprite.Frame == 1)
            {
                SwingAxe();
                sprite.FrameChanged -= swing;
            }
        };
        sprite.FrameChanged += swing;

        sprite.Play("atk");      
        attacking = true;
        var temp = attackMoveSpeed;
        attackMoveSpeed = speed;
        speed = temp;

        canAttack = false;
        attackTween = CreateTween();
        attackTween.TweenInterval(attackCooldown);
        attackTween.TweenCallback(Callable.From(() => { canAttack = true; }));

        Action stopAtkAction = () => { };

        stopAtkAction = () =>
        {
            sprite.AnimationFinished -= stopAtkAction;
            attacking = false;

            var temp = attackMoveSpeed;
            attackMoveSpeed = speed;
            speed = temp;

        };

        sprite.AnimationFinished += stopAtkAction;

    }
    /// <summary>
    /// called by the animation player, a child from the knight node.
    /// </summary>
    public void SwingAxe()
    {
        var slash = (SwordSlash)EffectPool.SpawnEffect(slashPrefab, GetParent());
        slash.GlobalPosition = sprite.GlobalPosition;
        slash.LookAt(GlobalPosition + 32 * aimDirection);
        if(sprite.Scale.Y < 0) { slash.Scale = new Vector2(1, -1); }

        slash.playerEffect = isPlayer;
        slash.knockback = attackPushForce;
        slash.direction = aimDirection;
        slash.damage = attackDamage;


        
    

    }

    private bool canRage = true;
    Tween rageTracker;
    public override void Button2(bool pressed)
    {
        if (stunned || raging || !canRage) return;

        rageTracker = CreateTween();
        rageTracker.TweenCallback(Callable.From(() =>
        {
            raging = true;
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
            raging = false;
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
        sprite.Play("hurt");
    }

    public override void HitstunCleanse()
    {
        base.HitstunCleanse();
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