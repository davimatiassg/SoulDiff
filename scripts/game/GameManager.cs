using Godot;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


public partial class GameManager : Node
{
	//Prefab List
	[Export] public Godot.Collections.Array<PackedScene> Creatures;

	public static List<(int score, PackedScene enemy)> enemies;
	public static RandomNumberGenerator _rng = new RandomNumberGenerator();
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

	public override void _ExitTree()
	{
		if(instance == this) instance = null;
	}


	public override void _Ready()
	{
		base._Ready();

		Debug.Assert(player != null);
		Debug.Assert(Creatures[0] != null);

		ghost = (GhostBody)Creatures[0].Instantiate();
		this.GetParent().CallDeferred("add_child", ghost);

		ConnectBodies(ghost, player);
		ghost.PossessStart(player);


		SetupSpawns();
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
		DisconnectBodies(enemy, enemy.controller);
		ConnectBodies(enemy, instance.player);
		enemy.PossessStart(instance.player);
	}

	public static void PossessionDown(EnemyBody enemy)
	{
		if (canPossess)
		{
			enemy.PossessEnd();
			DisconnectBodies(enemy, instance.player);
			ConnectBodies(instance.ghost, instance.player);
			instance.GetParent().CallDeferred("add_child", instance.ghost);
			instance.ghost.PossessStart(instance.player);
			instance.ghost.GlobalPosition = enemy.GlobalPosition;
		}
	}


	public static void ConnectBodies(AnyBody body, AnyController controller)
	{
		GD.Print($"connected {body.GetType()} onto {controller.GetType()}");

		if (controller.GetParent() == null) body.AddChild(controller);

		body.controller = controller;

		controller.Button1Action = body.Button1;
		controller.Button2Action = body.Button2;
		controller.Button3Action = body.Button3;

		controller.LeftAxisAction = body.Move;
		controller.RightAxisAction = body.Aim;
	}

	public static void DisconnectBodies(AnyBody body, AnyController controller)
	{
		GD.Print($"disconnected {body.GetType()} from {controller.GetType()}");
		if (controller.GetParent() == body) body.RemoveChild(controller);

		body.controller = null;

		controller.Button1Action = (bool pressed) => { };
		controller.Button2Action = (bool pressed) => { };
		controller.Button3Action = (bool pressed) => { };

		controller.LeftAxisAction = (Vector2 v) => { };
		controller.RightAxisAction = (Vector2 v) => { };
	}


	public static void SetupSpawns()
	{
		enemies = new List<(int score, PackedScene enemy)>();
		foreach(var pack in instance.Creatures)
		{
			AnyBody body = (AnyBody)pack.Instantiate();
			if(body is EnemyBody enemy) enemies.Add((enemy.MaxHP, pack));
			body.QueueFree();
		}

		Tween hordeTween;
		double hordeDelay = 5;
		float hordeSize = 20;
		hordeTween = instance.CreateTween();
		hordeTween.TweenInterval(hordeDelay);
		hordeTween.TweenCallback(Callable.From(()=>
			{
				SpawnHorde((int)hordeSize);
				hordeSize += 10;
				hordeDelay += 20;
			}));

		hordeTween.SetLoops();
	}

	public static void SpawnHorde(int points = 30)
	{
		while(points > 0)
		{
			int tries = enemies.Count;
			var liveIndexes = new List<int>();
			for(int i = 0; i < tries; i++) liveIndexes.Add(i);

			liveIndexes = liveIndexes.OrderBy(_ => _rng.Randi()).ToList();

			foreach(int index in liveIndexes)
			{
				if(points - enemies[index].score >= 0)
				{
					SpawnEnemy(enemies[index].enemy);
					points -= enemies[index].score;
					break;
				}
			}

		}
	}

	public static void SpawnEnemy(PackedScene enemy)
	{
		GD.Print($"lol {enemy}");
		Vector2 position = new Vector2(_rng.RandfRange(-640, 640), _rng.RandfRange(-360, 360));
		
		var spawn = (Node2D)enemy.Instantiate();
		
		instance.GetParent().CallDeferred("add_child", spawn);
		spawn.GlobalPosition = position;
		
	}


	public static void PlayerDie()
	{
		var tree = instance.GetTree();
		tree.CallDeferred("change_scene_to_file", tree.CurrentScene.SceneFilePath);
	}

}
