using Godot;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


public partial class GameManager : Node
{
	public static GameManager Instance;

	public static void PlayerDie()
	{
		var tree = Instance.GetTree();
		//STUB: fazer transição de tela adequada.
		tree.CallDeferred("change_scene_to_file", tree.CurrentScene.SceneFilePath);
	}

	public override void _Ready()
	{
		base._Ready();
		base._EnterTree();
		if (Instance == null) { Instance = this; return; }
		else if (Instance != this) { QueueFree(); return; }
	}
}
