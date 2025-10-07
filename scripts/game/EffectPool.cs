using Godot;
using System;
using System.Collections.Generic;


/// <summary>
/// Resource-pool estática para reutilização de efeitos comuns.
/// </summary>
public class EffectPool
{

	//FIXME: O dicionário está completamente inútil até agora.
	public static Dictionary<string, Node2D> objects = new Dictionary<string, Node2D>();

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


	public static Effect SpawnEffect(string path, string name, Node parent) =>
	SpawnEffect(path, name, parent, parent is Node2D parent2d ? parent2d.GlobalPosition : Vector2.Zero);

	public static Effect SpawnEffect(string path, string name, Node parent, Vector2 position)
	{
		Effect fx = (Effect)ResourceLoader.Load<PackedScene>(path).Instantiate();
		fx.name = name;
		parent.AddChild(fx);
		fx.GlobalPosition = position;
		return fx;
	}

	public static Effect SpawnEffect(PackedScene prefab, Node parent)
	{
		Effect fx = (Effect)(prefab.Instantiate());
		fx.name = prefab.ResourceName;
		parent.CallDeferred("add_child", fx);
		fx.Position = Vector2.Zero;
		return fx;
	}

	public static void DispawnEffect(Effect fx, string name)
	{
		fx.GetParent().CallDeferred(Node2D.MethodName.RemoveChild, fx);
		fx.QueueFree();
	}

}