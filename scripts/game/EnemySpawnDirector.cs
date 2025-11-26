using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EnemySpawnDirector : Node
{
    public static EnemySpawnDirector Instance;

	public static RandomNumberGenerator _rng = new RandomNumberGenerator();

    [Export] public Godot.Collections.Array<PackedScene> Creatures;

    public static List<(int score, PackedScene enemy)> enemies;
    
    
    public static void SetupSpawns()
	{
		enemies = new List<(int score, PackedScene enemy)>();
		foreach(var pack in Instance.Creatures)
		{
			AnyBody body = (AnyBody)pack.Instantiate();
			if(body is EnemyBody enemy) enemies.Add((enemy.MaxHP, pack));
			body.QueueFree();
		}

		Tween hordeTween;
		double hordeDelay = 5;
		float hordeSize = 20;
		hordeTween = Instance.CreateTween();
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
		
		Instance.GetParent().CallDeferred("add_child", spawn);
		spawn.GlobalPosition = position;
		
	}


	public override void _Ready()
    {
        base._EnterTree();
        if (Instance == null) { Instance = this; return; }
        else if (Instance != this) { QueueFree(); return; }
    }
}
