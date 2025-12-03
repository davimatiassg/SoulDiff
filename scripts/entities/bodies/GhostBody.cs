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
    
    public override void Button1(bool pressed)
    {
        if (disembodying) return;

        if (pressed && canAttack)
        {
            canAttack = false;
            Tween _atktween = CreateTween();
            _atktween.TweenInterval(flingCooldown);
            _atktween.TweenCallback(Callable.From(() => canAttack = true));

            curr_pebble = (GhostPebble)EffectPool.SpawnEffect(ghostPebblePrefab, GetParent<Node2D>());
            curr_pebble.Position = Position;
            curr_pebble.StartOrbit(this);

            pebbleIncreaser = curr_pebble.CreateTween();
            pebbleIncreaser.TweenMethod(
                Callable.From((float f) =>
                {
                    curr_pebble.damage = Mathf.FloorToInt(f * flingDamage);
                    curr_pebble.Scale = Vector2.One * f;
                }),
                1f, //grow from one (damage, size)
                3f, //to three (scale, size)
                4f //em 4 segundinhos
            );

            pebbleRotater = curr_pebble.CreateTween();
            pebbleRotater.TweenProperty(curr_pebble, "rotation_degrees", 180, 0.5f);
            pebbleRotater.TweenProperty(curr_pebble, "rotation_degrees", 360, 0.5f);
            pebbleRotater.TweenProperty(curr_pebble, "rotation_degrees", 0, 0.0f);
            pebbleRotater.SetLoops();
            return;
        }

        pebbleIncreaser.Kill();
        pebbleRotater.Kill();
        if (curr_pebble == null) return;
        curr_pebble.Fling(aimDirection);
        curr_pebble = null;

    }


    Tween dashMaker;
    public override void Button2(bool pressed)
    {
        if (disembodying) return;
        if (pressed && canDash)
        {
            canDash = false;
            Tween _dashtween = CreateTween();
            _dashtween.TweenInterval(dashCooldown + 0.2f);
            _dashtween.TweenCallback(Callable.From(() => canDash = true));
            
            float spd = speed;
            float a = acel;
            Vector2 dir = lastMoveDirection;

            dashMaker = CreateTween();
            dashMaker.TweenMethod(Callable.From((float f) =>
            {
                speed = spd * f;
                acel = a * f;
                moveDirection = dir;
            }), dashForce, 1f, 0.2f);
        }

        
    }

    public override void Button3(bool pressed)
    {
        if (disembodying) return;
        if (!pressed) return;
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
                    GlobalPosition = enemy.GlobalPosition;
                    PossessEnd();
                    PlayerController.Embody(enemy);
                    break;
                }

            }

            exclusionArray.Add((Rid)result["rid"]);
            query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + aimDirection * 128, CollisionMask, exclusionArray);
            result = spaceState.IntersectRay(query);
        }
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

        vulnerable = false;
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
        spawnTween.TweenCallback(Callable.From(() => vulnerable = true));
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
        GameManager.OnPlayerDie();
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
