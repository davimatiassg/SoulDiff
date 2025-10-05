using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public abstract partial class AnyBody : CharacterBody2D, Hitable
{
    [Export] protected bool vulnerable = true;
    [Export] public bool hasDamageFrames = false;
    [Export] public double invincibilityTime = 1.0;

    [Export] public bool isHitStunnable = true;

    [Export] public double hitStunTime = 1;
    [Export] protected bool stunned = false;

    /// Stats
    [Export] public int MaxHP = 10;
    [Export] public int HP = 0;

    [Export] public bool isPlayer = false;

    /// Components

    [Export] public AnimatedSprite2D sprite;
    [Export] public CollisionShape2D collision;

    /// Controlling

    protected BodyController controller;


    [Export] public Vector2 moveDirection = Vector2.Zero;
    protected Vector2 lastMoveDirection = Vector2.Right;

    [Export] public Vector2 aimDirection = Vector2.Zero;
    protected int lastAimDirectionX = 0;

    public abstract void Button1(bool pressed);
    public abstract void Button2(bool pressed);
    public abstract void Button3(bool pressed);
    public virtual void Move(Vector2 direction)
    {
        moveDirection = direction;
        if (direction != Vector2.Zero) lastMoveDirection = direction;
    }
    public virtual void Aim(Vector2 direction)
    {
        aimDirection = direction;
        bool flip = (lastAimDirectionX * (aimDirection.X) < 0);
        lastAimDirectionX = Mathf.Sign(aimDirection.X);
        if (flip)
        {
            sprite.Scale = new Vector2(1, lastAimDirectionX);
            sprite.Rotation = ((1 - lastAimDirectionX) / 2) * Mathf.Pi;
        }
    }


    /// Inner Visuals
    /// 
    Tween tweenOutlineColor;

    protected ShaderMaterial shaderMat;


    [Export]
    protected Color OutlineColor
    {
        get
        {
            if (shaderMat == null) return Colors.Transparent;
            return shaderMat.GetShaderParameter("outline_color").AsColor();
        }
        set
        {
            if (shaderMat == null) return;
            shaderMat.SetShaderParameter("outline_color", value);
        }
    }

    public override void _EnterTree()
    {
        base._EnterTree();

    }

    public override void _Ready()
    {
        base._Ready();

        shaderMat = (ShaderMaterial)sprite.Material.Duplicate();
        sprite.Material = shaderMat;
    }


    /// Methods
    public virtual void PossessStart(PlayerController cntrl)
    {
        HP = MaxHP;

        isPlayer = true;
        cntrl.currentBody = this;
        this.controller = cntrl;
        hasDamageFrames = true;


        controller.Button1Action = Button1;
        controller.Button2Action = Button2;
        controller.Button3Action = Button3;

        controller.LeftAxisAction = Move;
        controller.RightAxisAction = Aim;

        tweenOutlineColor = CreateTween();
        tweenOutlineColor.TweenProperty(this, "OutlineColor", new Color(0, 1, 1), .5);
        tweenOutlineColor.TweenProperty(this, "OutlineColor", new Color(1, 1, 1), .5);
        tweenOutlineColor.TweenProperty(this, "OutlineColor", new Color(0, 1, 1), .5);
        tweenOutlineColor.TweenProperty(this, "OutlineColor", new Color(0, 0, 1), 1);
        tweenOutlineColor.SetLoops();

    }
    public virtual void PossessEnd()
    {
        isPlayer = false;
        tweenOutlineColor.Kill();
        controller.Button1Action = (bool pressed) => { };
        controller.Button2Action = (bool pressed) => { };
        controller.Button3Action = (bool pressed) => { };

        controller.LeftAxisAction = (Vector2 v) => { };
        controller.RightAxisAction = (Vector2 v) => { };
    }

    Tween hitstunControl;
    Tween damageBoostControl;
    public virtual void TakeDamage(int damage, Vector2 knockback)
    {

        // FIXME: esse hitFX ainda não funciona
        //  ----------------------------------vvvvvvvvvv------------>ainda não está setado corretamente
        //EffectPool.SpawnEffect(Hitable.fx, "hitFX", this);

        if (!vulnerable) return;

        HP -= damage;

        HitstunApply();
        KnockbackApply(knockback);
        DamageFrameApply();

    }
    public virtual void HitstunApply()
    {
        if (isHitStunnable)
        {
            stunned = true;
            hitstunControl = CreateTween();
            hitstunControl.TweenInterval(hitStunTime);
            hitstunControl.TweenCallback(Callable.From(HitstunCleanse));
        }

    }

    public virtual void HitstunCleanse()
    {
        stunned = false;
    }
    public virtual void DamageFrameApply()
    {

        if (hasDamageFrames)
        {
            vulnerable = false;
            damageBoostControl = CreateTween();
            damageBoostControl.TweenInterval(invincibilityTime);
            damageBoostControl.TweenCallback(Callable.From(DamageFrameCleanse));
        }
    }

    public virtual void DamageFrameCleanse()
    {
        vulnerable = true;
    }

    public virtual void KnockbackApply(Vector2 force)
    {
        this.Velocity += force;
    }

    public virtual void Die() { }


    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var curr_vel = Velocity;

        curr_vel *= 0.85f;

        Velocity = curr_vel;

        //whatever inherits this must call MoveAndSlide(); after calling this base method.
    }
}
