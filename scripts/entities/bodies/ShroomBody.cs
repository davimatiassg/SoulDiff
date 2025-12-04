using Godot;
using System;

public partial class ShroomBody : EnemyBody
{
    [ExportGroup("Connections")]

    [Export] Sprite2D Glove_0;
    [Export] Sprite2D Glove_1;

    [Export] PackedScene PunchPrefab;
    [Export] PackedScene BombShroomPrefab;




    [ExportGroup("Balance Variables")]

    [Export] public float baseMoveSpeed = 128f;
    [Export] public int attackDamage = 1;
    [Export] public float attackPushForce = 320f;
    [Export] public float attackCooldown = 12f;

    [Export] public float deployCooldown = 4f;
    [Export] public float deployDuration = 20f; 
    private bool attacking;
    private bool moving;

    public override void Move(Vector2 direction)
    {
        if (dead || stunned) return;
        base.Move(direction);

        bool movingToggled = direction.IsZeroApprox() == moving;
        if (movingToggled) moving = !moving;

        if (attacking || stunned) return;


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

        Glove_0.LookAt(Glove_0.GlobalPosition + direction);
        Glove_1.LookAt(Glove_1.GlobalPosition + direction);
    }

    Tween attackTween;
    private bool canAttack = true;
    public override void Button1(bool pressed)
    {
        if (!pressed || dead || stunned) return;

        var Glove = Glove_0.Visible ? Glove_0 : (Glove_1.Visible ? Glove_1 : null);

        if (Glove == null) return;

        Glove.Visible = false;

        var punch = (Punch)EffectPool.SpawnEffect(PunchPrefab, this);
        
        punch.Position      = Glove.Position;
        punch.playerEffect  = isPlayer;
        punch.damage        = attackDamage;
        punch.knockback     = attackPushForce;
        punch.direction     = aimDirection;
        punch.punchDuration = attackCooldown;

        punch.LaunchPunch();

        var tween = Glove.CreateTween();
        tween.TweenInterval(attackCooldown);
        tween.TweenCallback(Callable.From(() => Glove.Visible = true));

        
    }
    
    bool canDeploy = true;
    public override void Button2(bool pressed)
    {
        if (stunned || !pressed || !canDeploy ) return;
        
        canDeploy = false;
        Tween _fireballtween = CreateTween();
        _fireballtween.TweenInterval(deployCooldown);
        _fireballtween.TweenCallback(Callable.From(() => canDeploy = true));

        if(isPlayer) HudManager.TriggerCooldown(2, deployCooldown);

        var curr_fireball = (Fireball)EffectPool.SpawnEffect(BombShroomPrefab, GetParent<Node2D>());
        curr_fireball.GlobalPosition = GlobalPosition;
        curr_fireball.playerEffect = isPlayer;
        

        var fireballCharger = CreateTween();
        fireballCharger.TweenInterval(deployDuration);
        fireballCharger.TweenCallback(Callable.From(() =>
        {
            curr_fireball?.Dispawn();
            
        }));
    }

}
