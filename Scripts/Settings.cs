using Godot;
using System;

public partial class Settings : Control
{



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MusicManager.PlayMusic("Theme_Shop");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_to_main_button_pressed()
	{
		GD.Print("To Main Button in Settings pressed");
		GetTree().Root.AddChild(ResourceLoader.Load<PackedScene>("res://Scenes/main_menu.tscn").Instantiate());
	}

	private void _on_button_novice_pressed()
	{
		setDifficulty(2);
	}

	private void _on_button_amateur_pressed()
	{
		setDifficulty(3);
	}


	private void _on_button_master_pressed()
	{

		setDifficulty(4);
	}

	private void setDifficulty(int difficulty)
	{
		DataHandler.difficulty = difficulty;
	}

}





