using System;
using System.Threading.Tasks;
using Godot;

public partial class GhostBody : AnyBody
{

    [ExportGroup("Balance Variables")]

    [Export]
    public float speed = 400.0f;

    [Export]
    public float acel = 400.0f;

    [Export]
    public int flingDamage = 3;

    [Export]
    public float flingCooldown = 1f;
    private float flingCD;

    [Export]
    public float dashForce = 4f;

    [Export]
    public float dashCooldown = 1f;

    private float dashCD;

    [ExportGroup("Extras & Cosmetics")]
    [Export] private PackedScene ghostPebblePrefab;

    /// Cosmetic Tools
    [Export] public AnimatedSprite2D skull;
    [Export] public AnimatedSprite2D skullGlow;


    [Export] public Line2D ghostTrail;

   
    [Export] private float trailAcel;

    [ExportGroup("")]
     private const int TRAIL_LEN = 20;
    private Vector2[] trailLastPoints = new Vector2[TRAIL_LEN];


    GhostPebble curr_pebble;
    Tween pebbleIncreaser;
    Tween pebbleRotater;
    

    
    public override void Button1(bool pressed)
    {


        if (pressed)
        {
            if (flingCD > 0) return;
            flingCD = flingCooldown;

            curr_pebble = (GhostPebble)EffectPool.SpawnEffect(ghostPebblePrefab, GetParent());
            curr_pebble.Position = Position;
            curr_pebble.StartOrbit(this);

            pebbleIncreaser = curr_pebble.CreateTween();
            pebbleIncreaser.TweenMethod(
                Callable.From((float f) =>
                {
                    curr_pebble.damage = Mathf.FloorToInt(f*flingDamage);
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
        pebbleIncreaser.Kill();
        if (curr_pebble == null) return;
        curr_pebble.Fling(aimDirection);
        curr_pebble = null;

    }


    Tween dashMaker;
    public override void Button2(bool pressed)
    {
        if (!pressed || dashCD > 0) return;
        float spd = speed;
        float a = acel;
        Vector2 dir = lastMoveDirection;

        dashCD = dashCooldown;

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
        var spaceState = GetWorld2D().DirectSpaceState;
        Godot.Collections.Array<Rid> exclusionArray = [GetRid()];

        var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + aimDirection * 128, CollisionMask, exclusionArray);
        var result = spaceState.IntersectRay(query);

        while (result.Count > 0)
        {
            var collider = (Node2D)result["collider"];
            if (collider is EnemyBody enemy)
            {
                if (((float)enemy.HP) / enemy.MaxHP <= 0.1f)
                {
                    GlobalPosition = enemy.GlobalPosition;
                    PossessEnd();
                    GameManager.PossessionUp(enemy);
                    break;
                }

            }

            exclusionArray.Add((Rid)result["rid"]);
            query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + aimDirection * 128, CollisionMask, exclusionArray);
            result = spaceState.IntersectRay(query);
        }
    }




    public override void HitstunApply()
    {
        base.HitstunApply();
        skull.Play("damaged");
       // skullGlow.Play("damaged");
    }

    public override void HitstunCleanse()
    {
        base.HitstunCleanse();
        skull.Play("idle");
        //skullGlow.Play("idle");
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
        GameManager.PlayerDie();
        base.Die();
        
    }



    public void reduceCooldowns(float delta)
    {
        if (flingCD > 0) flingCD -= delta;
        if (dashCD > 0) dashCD -= delta;
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
        base._PhysicsProcess(delta);
        Vector2 currentVelocity = Velocity;

        
        
        if (moveDirection != Vector2.Zero)
        {
            currentVelocity = currentVelocity.MoveToward(moveDirection * speed, (float)delta * acel *
                (currentVelocity.LengthSquared() / (moveDirection + currentVelocity).LengthSquared() + 1f)
            );
        }
        else
        {
            currentVelocity = currentVelocity.MoveToward(Vector2.Zero, (float)delta * acel * 1.5f);
        }

        Velocity = currentVelocity;

        if (stunned) { MoveAndSlide();  return; }

        MoveAndSlide();
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
        reduceCooldowns((float)delta);

        // Vector2 p = GlobalPosition;
        // if (moveDirection == Vector2.Zero) p += Vector2.Down;
        // else ghostTrail.Position = -p;
        // if (p != ghostTrail.Points.LastOrDefault()) ghostTrail.AddPoint(p);



    }
}
