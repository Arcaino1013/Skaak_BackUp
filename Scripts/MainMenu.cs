using Godot;
using System;

public partial class MainMenu : Node
{
	// Called when the node enters the scene tree for the first time.
	PackedScene mainMenuScreen = ResourceLoader.Load<PackedScene>("res://Scenes/main_menu.tscn");
	PackedScene gameOverScreen = ResourceLoader.Load<PackedScene>("res://Scenes/GameOver.tscn");
	Node mainMenu;
	Node gameOver;
	public override void _Ready()
	{
		mainMenu = mainMenuScreen.Instantiate();
		gameOver = gameOverScreen.Instantiate();
		GD.Print("diff: ", DataHandler.difficulty);
		gameOver.QueueFree();
		MusicManager.PlayMusic("Theme_Shop");
		DataHandler.GameState = Main.LoadGame();
		if (!DataHandler.volume_changed)
		{
			DataHandler.volume = 0.5f;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnStartGameButtonPressed()
	{

		mainMenu.QueueFree();
		var gameScene = ResourceLoader.Load<PackedScene>("res://Scenes/main.tscn").Instantiate();
		GetTree().Root.AddChild(gameScene);

	}

	private void OnNewGameButtonPressed()
	{
		Main.StartNewGame();
		mainMenu.QueueFree();
		var gameScene = ResourceLoader.Load<PackedScene>("res://Scenes/main.tscn").Instantiate();
		GetTree().Root.AddChild(gameScene);
	}

	private void _on_settings_btn_pressed()
	{
		GD.Print("To Settings Button in MainMenu pressed");
		GD.Print("Volume in main: ", DataHandler.volume);
		GetTree().Root.AddChild(ResourceLoader.Load<PackedScene>("res://Scenes/settings.tscn").Instantiate());
	}


	private void OnExitButtonPressed()
	{
		GetTree().Quit();
	}
}







