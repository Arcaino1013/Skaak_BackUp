using Godot;
using System;


public partial class Shop : Node
{
	private void _on_reroll_pressed()
	{

		if(DataHandler.GameState.Gold >= 50)
		{
		DataHandler.GameState.Gold -= 50;
		
		ShopCard ShopCard_Piece1 = GetNode<ShopCard>("HBoxContainer/VBoxContainer_Pieces/ShopCard_Piece1");
		ShopCard ShopCard_Piece2 = GetNode<ShopCard>("HBoxContainer/VBoxContainer_Pieces/ShopCard_Piece2");
		ShopCard ShopCard_PowerUp1 = GetNode<ShopCard>("HBoxContainer/VBoxContainer_PowerUps/ShopCard_PowerUp1");
		ShopCard ShopCard_PowerUp2 = GetNode<ShopCard>("HBoxContainer/VBoxContainer_PowerUps/ShopCard_PowerUp2");
		ShopCard ShopCard_SingleUseItem1 = GetNode<ShopCard>("HBoxContainer/VBoxContainer_SingleUseItems/ShopCard_SingleUseItem1");
		ShopCard ShopCard_SingleUseItem2 = GetNode<ShopCard>("HBoxContainer/VBoxContainer_SingleUseItems/ShopCard_SingleUseItem2");
		
	
		ShopCard_Piece1.recycleItems();
		ShopCard_Piece1.Reroll();
		ShopCard_Piece2.Reroll();
		ShopCard_PowerUp1.Reroll();
		ShopCard_PowerUp2.Reroll();
		ShopCard_SingleUseItem1.Reroll();
		ShopCard_SingleUseItem2.Reroll();
		GD.Print("DataHandler.Player_Inventory: ",DataHandler.Player_Inventory);
		}	
		
		GD.Print(DataHandler.Player_Inventory);
		}	
	
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Holt sich eine Node mit der die bools isShown wieder auf false gesetzt werden, dass beim nächsten Shopbesuch die Items wieder gekauft werden können 
		AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(DataHandler.volume));
		GetNode<ShopCard>("HBoxContainer/VBoxContainer_Pieces/ShopCard_Piece1").recycleItems();
		MusicManager.PlayMusic("Theme_Shop");
		GetNode<RichTextLabel>("%GoldCount").Text = DataHandler.GameState.Gold.ToString();
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//Updates Gold-Number 
		GetNode<RichTextLabel>("%GoldCount").Text = DataHandler.GameState.Gold.ToString();
	}
		
		
	private void _on_main_pressed()
	{
		GD.Print("Button in Shop to Main pressed");
		var main_scene = ResourceLoader.Load<PackedScene>("res://Scenes/main_menu.tscn").Instantiate();
		GetTree().Root.AddChild(main_scene);
	}
	
	private void _on_game_pressed()
	{
		GD.Print("Button in Shop to Game pressed");
		string Fen = DataHandler.ReplaceFenAfterFourthSlash(DataHandler.GameState);
		DataHandler.GameState.Fen = Fen;
		GD.Print("Fen: ", Fen);	
		var game_scene = ResourceLoader.Load<PackedScene>("res://Scenes/main.tscn").Instantiate();
		GetTree().Root.AddChild(game_scene);
	}
}












