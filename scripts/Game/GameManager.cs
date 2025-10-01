using Godot;
using System.Diagnostics;
using System.Threading.Tasks;


public partial class GameManager : Node
{
    //Prefab List
    [Export] public Godot.Collections.Array<PackedScene> Creatures;


    // Statically-accessible Data
    public static GameManager instance;
    [Export] public PlayerController player;

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

        GhostBody ghost = (GhostBody)Creatures[0].Instantiate();
        this.GetParent().CallDeferred("add_child", ghost);
        ghost.PossessStart(player);

    }
}
