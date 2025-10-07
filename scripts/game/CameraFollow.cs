using Godot;
using System;

public partial class CameraFollow : Camera2D
{
    [Export] public NodePath TargetPath; // arraste o nó-alvo no editor
    [Export] public float SmoothSpeed = 5f; // quanto maior, mais suave


    public override void _Ready()
    {
    }

    public Vector2 GetPlayerPosition() 
    {
        if(GameManager.instance.player == null) return Vector2.Zero;

        return GameManager.instance.player.currentBody.GlobalPosition;
    }

    public override void _Process(double delta)
    {

        // posição desejada da câmera (alvo + offset)
        Vector2 targetPos = GetPlayerPosition() + Offset;

        // interpolação suave até o alvo
        GlobalPosition = GlobalPosition.Lerp(targetPos, (float)delta * SmoothSpeed);
    }
}