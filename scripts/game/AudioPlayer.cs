using Godot;
using System;

public partial class AudioPlayer : Node
{

    public static AudioPlayer Instance;

    public static Random rng;

    [Export]
    public Godot.Collections.Dictionary sounds = new();

    public static void Play(string sound, bool loop = false, float pitch = 1, float volume = 1)
    {
        AudioStream soundStream = (AudioStream)Instance.sounds[sound];
        var streamPlayer = new AudioStreamPlayer();
        Instance.AddChild(streamPlayer);
        streamPlayer.Stream = soundStream;
        streamPlayer.PitchScale = pitch;
        streamPlayer.VolumeDb = volume;
        streamPlayer.Play();

        if(loop) streamPlayer.Finished += () => streamPlayer.Play();
        else streamPlayer.Finished += () => streamPlayer.QueueFree();
    }

    public static void PlayRandomPitch(string sound, bool loop = false, float pitch = 1, float variation = 0.4f, float volume = 1)
    {
        float newPitch = pitch + Random.GenerateFloat(1 - variation, 1 + variation);
        Play(sound, loop, newPitch, volume);
    }

    public override void _Ready()
    {
        base._Ready();
        if (Instance == null) Instance = this;
        else if (Instance != this) QueueFree();
        rng = new Random();
        
    }
}
