using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Base class for effects that might produce collisions and be created in large ammounts.
/// </summary>
public partial class Effect : Node2D
{
	public string name;
	[Export] public AnimatedSprite2D animation;
	[Export] public Area2D collider;

	/// <summary>
	/// Override this Method to set what happens when this effect colides with an object.
	/// </summary>
	/// <param name="bodyRid">Useful for precise object targeting, such as specific tiles on a tilemap</param>
	/// <param name="body">General node hit by the collider</param>
	/// <param name="bodyShapeIndex"></param>
	/// <param name="localShapeIndex"></param>
	public virtual void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
	{
		if (collider == null) return;
	}

	public virtual void OnCollisionExit(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
	{
		if (collider == null) return;
	}
	public override void _Ready()
	{
		base._Ready();
		collider.BodyShapeEntered += OnCollisionEnter;
		collider.BodyShapeExited += OnCollisionExit;
	}


	Tween dismissTime;
	public void SetExitTime(double seconds)
	{
		dismissTime = CreateTween();
		dismissTime.TweenInterval(seconds);
		dismissTime.TweenCallback(Callable.From(Dispawn));
	}

	public virtual void Dispawn()
	{
		EffectPool.DispawnEffect(this, name);
	}

}
