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
    public abstract void Button1(bool pressed);
    public abstract void Button2(bool pressed);
    public abstract void Button3(bool pressed);
    public abstract void Move(Vector2 direction);
    public abstract void Aim(Vector2 direction);


    /// Inner Visuals
    /// 
    Tween tweenOutlineColor;

    private ShaderMaterial shaderMat;

    
    [Export]
    private Color OutlineColor
    {
        get {
            if (shaderMat == null) return Colors.Transparent;
            return shaderMat.GetShaderParameter("outline_color").AsColor();
        }
        set {
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

        shaderMat = (ShaderMaterial)sprite.Material;
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
        controller.Button1Action = (bool pressed) => {};
        controller.Button2Action = (bool pressed) => {};
        controller.Button3Action = (bool pressed) => {};

        controller.LeftAxisAction = (Vector2 v) => {};
        controller.RightAxisAction = (Vector2 v) => {};
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
        if (HP <= 0 && isPlayer) { PossessEnd(); return;  }

        if (isHitStunnable)
        {
            hitstunControl = CreateTween();
            hitstunControl.TweenCallback(Callable.From(() => stunned = true));
            hitstunControl.TweenInterval(hitStunTime);
            hitstunControl.TweenCallback(Callable.From(HitstunCleanse));
        }

        
        if(hasDamageFrames){
            
            damageBoostControl = CreateTween();
            damageBoostControl.TweenCallback(Callable.From(() => vulnerable = false));
            damageBoostControl.TweenInterval(invincibilityTime);
            damageBoostControl.TweenCallback(Callable.From(DamageFrameCleanse));
            
        }
    }
    public virtual void HitstunApply()
    {
        stunned = true;
    }

    public virtual void HitstunCleanse()
    {
        stunned = false;
    }
    public virtual void DamageFrameApply()
    {
        vulnerable = false;
    }

    public virtual void DamageFrameCleanse()
    {
        vulnerable = true;
    }

    public virtual void Die() { }
}
