using Godot;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;


public partial class SequenceManager : Node
{


	public static SequenceManager Instance;


	public static Action OnPlayerDie;

	public static Action OnPlayerRefuseToDie;

	[Export]
	public Control DeathMenu;


	[Export]
	public int deathCount = 0;

	private void PlayerFirstDeath()
	{
		OnPlayerDie = PlayerDeath;
		Timer.Pause();

		OnPlayerRefuseToDie = () =>
		{
			GhostSpawn();
			AudioPlayer.PlayMusic("mus_The_Hexsmith");
			Timer.Start();

			//TODO! - Remover gradualmente zoom na câmera

			OnPlayerRefuseToDie = () =>
			{
				SceneManager.ChangeLevel("Level_1");
				Timer.Reset();
				Timer.Start();
				Instance.CreateTween().TweenProperty(AudioPlayer.Instance.musicPlayer, "pitch_scale", 1, 0.5f);
			};
		};


		Instance.CreateTween().TweenProperty(AudioPlayer.Instance.musicPlayer, "pitch_scale", 0.5, 1f);
		//TODO! - Zoom na câmera
		MenuManager.PlayDeathMenu();

	}
	private void PlayerDeath()
	{
		Timer.Pause();
		if (PlayerController.Instance.currentBody is ArcherBody)
		{ GhostSpawn(); return; }
		deathCount++;
		Instance.CreateTween().TweenProperty(AudioPlayer.Instance.musicPlayer, "pitch_scale", 0.5, 1f);
		//TODO! - Zoom na câmera
		MenuManager.PlayDeathMenu();
	}

	public void GhostSpawn()
	{
		var body = PlayerController.Instance.currentBody;
		Debug.Assert(body != null);

		PlayerController.Disembody(body);
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

		OnPlayerDie += PlayerFirstDeath;
		CallDeferred(MethodName.PostReady);
	}

	private void PostReady()
	{
		AudioPlayer.PlayMusic("mus_Hexes_of");
		Timer.Reset();
		Timer.Start();
	}
}
