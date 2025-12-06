using Godot;
using System;

public partial class Timer : RichTextLabel
{
    public static Timer Instance;
    private double elapsed;
    private bool isRunning;

    // Inicia ou retoma o timer
    public static void Start ()
    {
        Instance.isRunning = true;
    }

    // Pausa o timer
    public static void Pause()
    {
        Instance.isRunning = false;
    }

    // Zera o timer
    public static void Reset ()
    {
        Instance.elapsed = 0;
    }

    public static double GetTime()
    {
        return Instance.elapsed;
    }

    // Retorna o tempo formatado: mm:ss:ms
    public static string GetFormattedTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(Instance.elapsed);
        return $"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:000}";
    }


    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Instance.QueueFree();  Instance = this; }
    }
    public override void _Process(double delta)
    {
        if (isRunning)
        { 
            elapsed += delta;
            Text = GetFormattedTime();
        }
            
    }
}
