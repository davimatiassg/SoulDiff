using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

[GlobalClass]
public abstract partial class AnyBody : CharacterBody2D, Hitable
{
    [Export] protected bool vulnerable = true;
    [Export] public bool hasDamageFrames = false;


    [Export] public float speed = 128.0f;
    [Export] public double invincibilityTime = 1.0;

    [Export] public bool isHitStunnable = true;

    [Export] public double hitStunTime = 1;
    [Export] protected bool stunned = false;
    [Export] public bool dead = false;

    /// Stats
    [Export] public int MaxHP = 10;
    [Export] public int HP = 0;

    [Export] public bool isPlayer = false;

    /// Components

    [Export] public AnimatedSprite2D sprite;
    [Export] public CollisionShape2D collision;

    /// Controlling

    [Export] public AnyController controller;


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

        HP = MaxHP;
    }


    /// Methods
    public virtual void PossessStart(PlayerController cntrl)
    {
        HP = MaxHP;

        isPlayer = true;
        dead = false;
        stunned = false;
        hasDamageFrames = true;

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
        OutlineColor = Colors.Transparent;
        tweenOutlineColor.Kill(); 
    }

    Tween hitstunControl;
    Tween damageBoostControl;
    public virtual void TakeDamage(int damage, Vector2 knockback)
    {
        if (dead) return;
        var fx = EffectPool.SpawnEffect("Hit", GlobalPosition);
        fx.SetExitTime(0.2);

        if (!vulnerable)
        {
            return;
        }



        HP -= damage;

        HitstunApply(damage);
        KnockbackApply(knockback);
        DamageFrameApply();

        if (HP <= 0) Die();

        if(isPlayer) MainCamera.CameraShake(damage, 0.1f);

    }
    public virtual void HitstunApply(float damage)
    {
        if (isHitStunnable)
        {
            stunned = true;
            hitstunControl = CreateTween();
            hitstunControl.TweenInterval(hitStunTime * Mathf.Sqrt(damage));
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


    public abstract void Die();

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);



        Velocity *= 0.85f;


        
        KinematicCollision2D collision = MoveAndCollide(Velocity * (float)delta);
        if (collision != null)
        {
            Vector2 motion = Velocity.Normalized();
            motion = motion.Slide(collision.GetNormal());
            MoveAndCollide(motion);
        }

        if (dead || stunned) return;
        
        var curr_vel = Velocity;

        curr_vel = moveDirection * speed;

        Velocity = curr_vel;
        
        
    }
}
