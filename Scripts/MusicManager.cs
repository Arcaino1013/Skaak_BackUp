using Godot;
using System.Collections.Generic;

public partial class MusicManager : Node
{
private static MusicManager _instance;
	private AudioStreamPlayer _musicPlayer;
	private Dictionary<string, AudioStream> _musicTracks;
	private Dictionary<string, AudioStream> _soundEffects;
	private List<AudioStreamPlayer> _effectPlayers;

	public override void _Ready()
	{
		if (_instance == null)
		{
			_instance = this;
			_musicPlayer = new AudioStreamPlayer();
			_musicPlayer.Autoplay = true;
			AddChild(_musicPlayer);

			_musicTracks = new Dictionary<string, AudioStream>
			{
				{ "Theme_Sound", (AudioStream)GD.Load("res://Sounds/theme_sound.wav") },
				{ "Theme_Shop", (AudioStream)GD.Load("res://Sounds/theme_shop.wav") },
				{ "Theme_Boss", (AudioStream)GD.Load("res://Sounds/boss_theme.wav") }
			};

			_soundEffects = new Dictionary<string, AudioStream>
			{
				{ "ChessPiecePickup", (AudioStream)GD.Load("res://Sounds/chesspiecepickup.wav") },
				{ "ChessPieceMove", (AudioStream)GD.Load("res://Sounds/chesspiecemoving.wav") },
				{ "ChessPieceTake", (AudioStream)GD.Load("res://Sounds/chesspiecetake.wav") }
			};

			_effectPlayers = new List<AudioStreamPlayer>();
			
			// Adding multiple effect players to handle overlapping sound effects
			for (int i = 0; i < 5; i++) 
			{
				AudioStreamPlayer effectPlayer = new AudioStreamPlayer();
				AddChild(effectPlayer);
				_effectPlayers.Add(effectPlayer);
			}

			// Ensure this instance persists across scene changes
			this.SetProcess(false);
		}
		else
		{
			QueueFree();
		}
	}

	public static void PlayMusic(string trackName)
	{
		if (_instance != null && _instance._musicTracks.ContainsKey(trackName))
		{
			if (_instance._musicPlayer.Stream != _instance._musicTracks[trackName])
			{
				_instance._musicPlayer.Stream = _instance._musicTracks[trackName];
				_instance._musicPlayer.Play();
			}
		}
	}

	public static void StopMusic()
	{
		_instance?._musicPlayer.Stop();
	}

	public static void PlaySoundEffect(string effectName)
	{
		if (_instance != null && _instance._soundEffects.ContainsKey(effectName))
		{
			foreach (var player in _instance._effectPlayers)
			{
				if (!player.Playing)
				{
					player.Stream = _instance._soundEffects[effectName];
					player.Play();
					break;
				}
			}
		}
	}
	
}
