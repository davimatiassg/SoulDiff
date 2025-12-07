using Godot;
using System;

public partial class FinalDoor : Node2D
{
    [Export] Area2D triggerCollision;
    [Export] AnimatedSprite2D anim;

    [Export] public int remainingKeys = 2;

    public override void _Ready()
    {
        base._Ready();
        if (triggerCollision == null) return;
        DoorKey.OnCollectKey += CollectKey;

		triggerCollision.BodyShapeEntered += OnCollisionEnter;
	
    }

    public void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
	{
        GD.Print("collided");
        if (body is AnyBody entityBody && entityBody.isPlayer)
        {
            if (remainingKeys <= 0)
            {
                anim.Play("open");
                //TODO: open the door
                //STUB:
                GD.Print("door opened");
            }
            else
            {
                GD.Print("door still closed");
                //TODO: Show dialog 
                // "The door is locked. There are {remainingKeys} Keys left around the dungeon."
            }
        }
	}


    public void CollectKey()
    {
        remainingKeys--;
    }


    

}
