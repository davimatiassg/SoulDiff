using System;
using System.Diagnostics;
using Godot;

public abstract partial class ABody : CharacterBody2D
{
    [Export] protected bool vulnerable = true;
    [Export] public bool hasDamageFrames = false;
    [Export] public int invincibilityTime = 1000;
    /// Stats
    [Export] protected int HP = 0;

    /// Components

    [Export] public AnimatedSprite2D sprite;
    [Export] public CollisionShape2D collision;

    /// Controlling
    protected BodyController controller;
    public abstract void Button1();
    public abstract void Button2();
    public abstract void Button3();
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
        tweenOutlineColor.Kill();
        controller.Button1Action = () => {};
        controller.Button2Action = () => {};
        controller.Button3Action = () => {};

        controller.LeftAxisAction = (Vector2 v) => {};
        controller.RightAxisAction = (Vector2 v) => {};
    }
    public virtual void TakeDamage(int damage)
    {
        if (!vulnerable)
        {
            //TODO! - show that this is invulnerable
            return;
        }
        HP -= damage;
        if (HP <= 0) PossessEnd();
    }

    public virtual void Die() { }
}
