using System;
using System.Threading.Tasks;
using Godot;

public partial class GhostBody : AnyBody
{

    [ExportGroup("Balance Variables")]

    [Export]
    public float acel = 400.0f;

    [Export]
    public int flingDamage = 3;

    [Export]
    public float flingCooldown = 1f;
    private bool canAttack = true;

    [Export]
    public float dashForce = 4f;

    [Export]
    public float dashCooldown = 1f;

    private bool canDash = true;

    [ExportGroup("Extras & Cosmetics")]

    [Export] private PackedScene ghostBlastPrefab;
    [Export] private PackedScene ghostPebblePrefab;

    /// Cosmetic Tools
    [Export] public AnimationPlayer anim;
    [Export] public AnimatedSprite2D skullGlow;


    [Export] public Line2D ghostTrail;

   
    [Export] private float trailAcel;

    [ExportGroup("")]
     private const int TRAIL_LEN = 20;
    private Vector2[] trailLastPoints = new Vector2[TRAIL_LEN];


    GhostPebble curr_pebble;
    Tween pebbleIncreaser;
    Tween pebbleRotater;

    [Export]
    public bool disembodying = false;

    [Export]
    public EnemyBody targetedCorpse = null;

    public override void Aim(Vector2 direction)
    {
        base.Aim(direction);


        var spaceState = GetWorld2D().DirectSpaceState;
        Godot.Collections.Array<Rid> exclusionArray = [GetRid()];

        var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + aimDirection * 128, 1 << 0, exclusionArray);
        var result = spaceState.IntersectRay(query);

        while (result.Count > 0)
        {
            var collider = (Node2D)result["collider"];
            if (collider is EnemyBody enemy)
            {
                if (enemy.dead)
                {
                    targetedCorpse = enemy;
                    break;
                }

            }

            exclusionArray.Add((Rid)result["rid"]);
            query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + aimDirection * 128, CollisionMask, exclusionArray);
            result = spaceState.IntersectRay(query);
        }
    }

    public override void Button1(bool pressed)
    {
        if (disembodying || dead) return;

        if (pressed && canAttack)
        {
            canAttack = false;
            Tween _atktween = CreateTween();
            _atktween.TweenInterval(flingCooldown);
            if (isPlayer) HudManager.TriggerCooldown(1, flingCooldown);
            _atktween.TweenCallback(Callable.From(() => canAttack = true));

            curr_pebble = (GhostPebble)EffectPool.SpawnEffect(ghostPebblePrefab, GetParent<Node2D>());
            curr_pebble.Position = Position;
            curr_pebble.StartOrbit(this);
            curr_pebble.Fling(aimDirection);
        }


    }


    Tween dashMaker;
    public override void Button2(bool pressed)
    {
        if (disembodying || !pressed || !canDash) return;

        canDash = false;
        Tween _dashtween = CreateTween();
        _dashtween.TweenInterval(dashCooldown);
        _dashtween.TweenCallback(Callable.From(() => canDash = true));

        float spd = speed;
        float a = acel;
        Vector2 dir = lastMoveDirection;
        
        if (isPlayer) HudManager.TriggerCooldown(2, dashCooldown);

        dashMaker = CreateTween();
        dashMaker.TweenMethod(Callable.From((float f) =>
        {
            speed = spd * f;
            acel = a * f;
            moveDirection = dir;
        }), dashForce, 1f, 0.2f);
        
        
    }

    public override void Button3(bool pressed)
    {
        if (disembodying || !pressed || targetedCorpse == null) return;
        GlobalPosition = targetedCorpse.GlobalPosition;
        PossessEnd();
        PlayerController.Embody(targetedCorpse);

    }

    public void EmitSpawnBlast()
    {
        var wave = (Shockwave)EffectPool.SpawnEffect(ghostBlastPrefab, GetParent<Node2D>());
        wave.GlobalPosition = GlobalPosition;
        wave.playerEffect = isPlayer;
        wave.knockback = 50;
        wave.damage = flingDamage * 2;

    }

    public override void PossessStart(PlayerController cntrl)
    {
        base.PossessStart(cntrl);

        anim.Play("spawn");

        isVulnerable = false;
        stunned = true;
        float spd = speed;
        float a = acel;
        Tween spawnTween = CreateTween();
        spawnTween.TweenMethod(Callable.From((float f) =>
        {
            speed = spd * f;
            acel = a * f;
        }), dashForce, 1f, 0.5f);
        spawnTween.TweenInterval(1);
        spawnTween.TweenCallback(Callable.From(() => isVulnerable = true));
        spawnTween.TweenCallback(Callable.From(() => stunned = false));
        spawnTween.TweenCallback(Callable.From(() => anim.Play("idle")));
    }


    public override void HitstunApply(float damage)
    {
        if (disembodying) return;
        base.HitstunApply(damage);
        anim.Play("damaged");
    }

    public override void HitstunCleanse()
    {
        if (disembodying) return;
        base.HitstunCleanse();
        anim.Play("idle");
    }

    public override void DamageFrameApply()
    {
        base.DamageFrameApply();
    }

    public override void DamageFrameCleanse()
    {
        base.DamageFrameCleanse();
    }

    //TODO!
    public override void Die()
    {
        if (disembodying) return;
        dead = true;
        SequenceManager.OnPlayerDie();
        CreateTween().TweenProperty(this, "modulate", Colors.Transparent, 3);
    }


    public override void _Ready()
    {
        base._Ready();
        for (int i = 0; i < TRAIL_LEN; i++)
        {
            trailLastPoints[i] = Vector2.Zero;
        }

        Tween tweenGlowColor = CreateTween();
        tweenGlowColor.TweenProperty(skullGlow, "modulate", new Color(0, 1, 1), .5);
        tweenGlowColor.TweenProperty(skullGlow, "modulate", new Color(1, 1, 1), .5);
        tweenGlowColor.TweenProperty(skullGlow, "modulate", new Color(0, 1, 1), .5);
        tweenGlowColor.TweenProperty(skullGlow, "modulate", new Color(0, 0, 1), 1);
        tweenGlowColor.SetLoops();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (disembodying) return;
        base._PhysicsProcess(delta);
        // Vector2 currentVelocity = Velocity;

        
        
        // if (moveDirection != Vector2.Zero && !stunned)
        // {
        //     currentVelocity = currentVelocity.MoveToward(moveDirection * speed, (float)delta * acel *
        //         (currentVelocity.LengthSquared() / (moveDirection + currentVelocity).LengthSquared() + 1f)
        //     );
        // }
        // else
        // {
        //     currentVelocity = currentVelocity.MoveToward(Vector2.Zero, (float)delta * acel * 1.5f);
        // }

        // Velocity = currentVelocity;

        

        // MoveAndCollide();
    }

    

    private void CalculateTrail(float delta)
    {
        ghostTrail.Position = -Position;

        for (int i = 0; i < ghostTrail.Points.Length; i++)
        {
            Vector2 p = ghostTrail.GetPointPosition(i);
            ghostTrail.SetPointPosition(i, trailLastPoints[i] + Vector2.Down * delta * trailAcel );
            trailLastPoints[i] = p;
        }

        ghostTrail.AddPoint(Position);

        while (ghostTrail.Points.Length > TRAIL_LEN) ghostTrail.RemovePoint(0);  
    }




    public override void _Process(double delta)
    {
        base._Process(delta);
        CalculateTrail((float)delta);

    }
}
