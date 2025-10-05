using Godot;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


public partial class GameManager : Node
{
    //Prefab List
    [Export] public Godot.Collections.Array<PackedScene> Creatures;


    // Statically-accessible Data
    public static GameManager instance;
    [Export] public PlayerController player;

    
    [Export] public GhostBody ghost;

    public override void _EnterTree()
    {
        base._EnterTree();
        if (instance == null) { instance = this; return; }
        else if (instance != this) { QueueFree(); return; }
    }

    public override void _Ready()
    {
        base._Ready();

        Debug.Assert(player != null);
        Debug.Assert(Creatures[0] != null);

        ghost = (GhostBody)Creatures[0].Instantiate();
        this.GetParent().CallDeferred("add_child", ghost);
        ghost.PossessStart(player);

        

    }

    [Export] public double possessionCD = .5;
    static Tween possessTween;
    public static bool canPossess = true;

    public static void PossessionUp(EnemyBody enemy)
    {
        Debug.Assert(instance != null);
        Debug.Assert(instance.ghost != null);

        
        canPossess = false;
        possessTween = instance.CreateTween();
        possessTween.TweenInterval(instance.possessionCD);
        possessTween.TweenCallback(Callable.From(() => { canPossess = true; }));


        instance.ghost.GetParent().RemoveChild(instance.ghost);
        enemy.PossessStart(instance.player);
    }

    public static void PossessionDown(EnemyBody enemy)
    {
        if (canPossess)
        {
            enemy.PossessEnd();
            instance.GetParent().CallDeferred("add_child", instance.ghost);
            instance.ghost.PossessStart(instance.player);
            instance.ghost.GlobalPosition = enemy.GlobalPosition;
        }
    }  

}
