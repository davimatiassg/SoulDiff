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

    public static void PossessionUp(EnemyBody enemy)
    {
        GD.Print(instance == null);
        GD.Print(instance.ghost == null);

        instance.ghost.GetParent().RemoveChild(instance.ghost);
        enemy.PossessStart(instance.player);
    }

    public static void PossessionDown()
    {  
        instance.GetParent().CallDeferred("add_child", instance.ghost);
        instance.ghost.PossessStart(instance.player);
    }  

}
