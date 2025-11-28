using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
    public static SceneManager Instance;

    [Export]
    public Godot.Collections.Dictionary<string, string> levels = new();

    [Export]
    public Node currentLevel;


    [Export]
    public TransitionVignette vignette;


    public async static void ChangeScene(string scenePath)
    {

        ResourceLoader.LoadThreadedRequest(scenePath);

        ResourceLoader.ThreadLoadStatus status = ResourceLoader.ThreadLoadStatus.InProgress;

        while (status == ResourceLoader.ThreadLoadStatus.InProgress)
        {
            status = ResourceLoader.LoadThreadedGetStatus(scenePath);

            await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        if (status == ResourceLoader.ThreadLoadStatus.Loaded)
        {
            var scene = ResourceLoader.LoadThreadedGet(scenePath) as PackedScene;

            foreach (var child in Instance.currentLevel.GetChildren()) { child.QueueFree(); }

            Instance.currentLevel.AddChild(scene.Instantiate());
        }

        await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.SceneChanged);

        GD.Print(Instance.GetTree());
    }

    public async static void ChangeLevel(string sceneName)
    {
        
        Instance.vignette.FadeOut();
        while (Instance.vignette.isTransitioning)
        {
            await Task.Delay(1000);
        }
        ChangeScene(Instance.levels[sceneName]);
        Instance.vignette.FadeIn();
    }
    
	public override void _Ready()
    {
        base._Ready();
        base._EnterTree();
        if (Instance == null) { Instance = this; return; }
        else if (Instance != this) { QueueFree(); return; }


    }
    
}
