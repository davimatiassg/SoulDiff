using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;


/// <summary>
/// Resource-pool estática para reutilização de efeitos comuns.
/// </summary>

[GlobalClass]
public partial class EffectPool : Node
{
	public static EffectPool Instance;

	public static Dictionary<string, Stack<Effect>> pool = new();

	[Export]
	public Godot.Collections.Dictionary<string, PackedScene> objects = new();

	// public static Node2D SpawnProp(string path, string name, Node parent)
	// {
	// 	Node2D fx;
	// 	if(objects.ContainsKey(name))
	// 	{
	// 		fx = objects[name];
	// 		objects.Remove(name);

	// 		return fx;
	// 	}
	// 	fx = (Node2D)ResourceLoader.Load<PackedScene>(path).Instantiate();
	// 	parent.AddChild(fx);
	// 	return fx;
	// }


	public static Effect SpawnEffect(string name, Vector2 globalPosition)
	{
		Effect fx = null;

		if (pool.ContainsKey(name))
		{
			if (pool[name].Count > 0) fx = pool[name].Pop();
		}
		else pool.Add(name, new Stack<Effect>());

		if (fx == null)
		{
			var obj = Instance.objects;
			if (!obj.ContainsKey(name)) return null;
			fx = (Effect)obj[name].Instantiate();
		}


		Instance.AddChild(fx);

		fx.name = name;

		fx.GlobalPosition = globalPosition;

		return fx;
	}


	public static Effect SpawnEffect(PackedScene prefab, Node2D parent)
	{
		Effect fx = SpawnEffect(prefab.ResourcePath, parent.GlobalPosition);
		if (fx != null) return fx;

		fx = (Effect)(prefab.Instantiate());
		fx.name = prefab.ResourcePath;
		parent.CallDeferred("add_child", fx);
		fx.GlobalPosition = parent.GlobalPosition;
		return fx;
	}

	public static void DispawnEffect(Effect fx)
	{
		fx.GetParent().CallDeferred(Node2D.MethodName.RemoveChild, fx);
		pool[fx.name].Append(fx);
	}


	public override void _Ready()
	{
		base._Ready();
		if (Instance == null) Instance = this;
		else if (Instance != this) { QueueFree(); return; }
	}

}