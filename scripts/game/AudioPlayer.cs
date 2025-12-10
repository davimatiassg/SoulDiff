using Godot;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

public partial class AudioPlayer : Node
{

    public float musicVolume = 1.0f;

    public float sfxVolume = 1.0f;

    public static AudioPlayer Instance;

    public static Random rng;

    public AudioStreamPlayer musicPlayer;

    [Export]
    public Godot.Collections.Dictionary<string, GodotObject> sounds = new();

    public static AudioStreamPlayer2D PlaySound(string sound, Node parent, bool loop = false, float pitch = 1)
    {
        AudioStream soundStream = (AudioStream)Instance.sounds[sound];
        var streamPlayer = new AudioStreamPlayer2D();
        parent.AddChild(streamPlayer);
        streamPlayer.Stream = soundStream;
        streamPlayer.PitchScale = pitch;
        streamPlayer.VolumeLinear = Instance.sfxVolume;
        streamPlayer.Play();

        if (loop) streamPlayer.Finished += () => streamPlayer.Play();
        else streamPlayer.Finished += streamPlayer.QueueFree;

        return streamPlayer;
    }


    public static AudioStreamPlayer2D PlaySoundRandomPitch(string sound, Node parent, bool loop = false, float pitch = 1, float variation = 0.4f)
    {
        float newPitch = pitch + Random.GenerateFloat(-variation, variation);
        return PlaySound(sound, parent, loop, newPitch);   
    }

    public static Tween PlayRandomContinuousSound(string sound, Node parent, float pitch = 1, float variation = 0.4f, float interval = 0.25f)
    {

        Tween tween = Instance.CreateTween();
        tween.TweenInterval(interval);
        tween.TweenCallback(Callable.From(() =>
        {
            PlaySoundRandomPitch(sound, parent, false, pitch, variation);
        }));
        tween.SetLoops();

        return tween;
    }


    public static void PlayMusic(string musicName, float fadeTime = 1f)
    {
        if (Instance.musicPlayer != null)
        {
            Tween tween = Instance.CreateTween();
            tween.TweenProperty(Instance.musicPlayer, "volume_linear", 0, fadeTime);
            tween.TweenCallback(Callable.From(() =>
            {
                Instance.musicPlayer.QueueFree();
                Instance.musicPlayer = null;
                PlayMusic(musicName, fadeTime);
            }));

            return;

        }

        AudioStream newMusicStream = (AudioStream)Instance.sounds[musicName];
        var streamPlayer = new AudioStreamPlayer();
        Instance.AddChild(streamPlayer);
        streamPlayer.Stream = newMusicStream;
        streamPlayer.VolumeLinear = Instance.musicVolume;

        Instance.musicPlayer = streamPlayer;
        Instance.musicPlayer.Play();

        streamPlayer.Finished += () => streamPlayer.Play();

        return;
    }


    public override void _Ready()
    {
        base._Ready();
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            try
            {
                Instance.QueueFree();
            } catch (ObjectDisposedException e) {}
            Instance = this;
        }
        rng = new Random();

    }
}
