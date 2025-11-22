using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class Main : Control
{
	PackedScene mainMenuScreen = ResourceLoader.Load<PackedScene>("res://Scenes/main_menu.tscn");
	PackedScene gameOverScreen = ResourceLoader.Load<PackedScene>("res://Scenes/GameOver.tscn");
	Node gameOver;
	Node mainMenu;
	Label goldDisplayed;
	Label goldLabel;
	Label movesDisplayed;
	Label timeLabel;
	Label levelLabel;
	Label InformationLabel;
	TextureRect InformationDisplay;
	TextureRect Background;
	TextureRect ConfirmButtonTextureRect;
	Button ConfirmButton;
	bool gameOverDisplayed = false;
	bool setUpCompleted = false;
	double timer = 0;
	bool timerStarter = true;
	
	
	public override void _Ready()
	{

		GetNode<TextureRect>("%Background").Texture = getBackground();
		goldLabel = GetNode<Label>("%GoldLabel");
		goldLabel.Text = " Gold";
		
		// Information Label oben rechts 
		InformationDisplay = GetNode<TextureRect>("%InformationDisplay");
		InformationLabel = GetNode<Label>("%InformationLabel");
		InformationDisplay.Visible = true;
		InformationLabel.Visible = true;
		InformationLabel.Text = "Choose your formation by swapping your pieces";


		ConfirmButtonTextureRect = GetNode<TextureRect>("%ConfirmButtonTextureRect");
		ConfirmButton = GetNode<Button>("%ConfirmButton");
		ConfirmButton.Visible = true;
		ConfirmButtonTextureRect.Visible = true;
		AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(DataHandler.volume));
		if (DataHandler.GameState.Level == 2)
		{
			InformationLabel.Text = "Stepping on the ice freezes the piece for one round";
		}
		if ((DataHandler.GameState.Level + 1) % 3 == 0)
		{
			MusicManager.PlayMusic("Theme_Boss");			
		}
		else
		{
			MusicManager.PlayMusic("Theme_Sound");			
		}

		gameOver = gameOverScreen.Instantiate();
		gameOver.QueueFree();
		mainMenu = mainMenuScreen.Instantiate();

		goldDisplayed = GetNode<Label>("%GoldCount");
		movesDisplayed = GetNode<Label>("%MoveCount");
		levelLabel = GetNode<Label>("%LevelLabel");
		timeLabel = GetNode<Label>("%TimeLabel");
		
		timeLabel.Text = "--:--";
			
			
		goldDisplayed.Text = DataHandler.GameState.Gold.ToString();
		DataHandler.moveCounter = 0;
		if(!gameOverDisplayed && !DataHandler.isGameOver){
			// wofür?
		}

	}

	public override void _Process(double delta)
	{
		if (!gameOverDisplayed && movesDisplayed != null)
		{
			movesDisplayed.Text = DataHandler.moveCounter.ToString();
		}
		if(setUpCompleted)
		{

			timer += delta;
			if(timerStarter)
			{
				timer = 0.0;
				timerStarter = false;
				ConfirmButton.Visible = false;
				ConfirmButtonTextureRect.Visible = false;
				InformationDisplay.Visible = false;
				InformationLabel.Visible = false;
			}
			TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
			timeLabel.Text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
			setLevelText();
		}
		
	}

	private void setLevelText()
	{
		switch (DataHandler.GameState.Level) 
		{
		case 0:
			levelLabel.Text = "1 - 1";
			break;
		case 1:
			levelLabel.Text = "1 - 2";
			break;
		case 2:
			levelLabel.Text = "1 - Boss";
			break;
		case 3:
			levelLabel.Text = "2 - 1";
			break;
		case 4:
			levelLabel.Text = "2 - 2";
			break;
		case 5:
			levelLabel.Text = "2 - Boss";
			break;
		case 6:
			levelLabel.Text = "Lvl = 6";
			break;
		case 7:
			break;
		default:
			levelLabel.Text = "Skaak";
			break;
		}
	}

	private Texture2D getBackground()
	{
		switch (DataHandler.GameState.Level) 
		{
		case 0:
		case 1:
			// World 1 Levels 
			return DataHandler.GameTextures["Stage1_Background"];
		case 2:
			// World 1 Boss
			return DataHandler.GameTextures["Boss1_Background"];
		case 3:
		case 4:
			// World 2 Levels
			return DataHandler.GameTextures["Stage2_Background"];
		case 5:
			// World 2 Boss
			return DataHandler.GameTextures["Boss2_Background"];
		default:
			return GD.Load<Texture2D>("res://Textures/backgrounds/skaak_main_menu.jpg");
		}
		
	}
	
	// Buttons
	
	public void onNewGameClicked()
	{
		int gold = DataHandler.GameState.Gold;
		DataHandler.GameState = CreateNewGame(gold / 2);
		DataHandler.isGameOver = false;
		var gameScene = ResourceLoader.Load<PackedScene>("res://Scenes/main.tscn").Instantiate();
		GetTree().Root.AddChild(gameScene);
	}



	private void _on_to_main_button_pressed()
	{
		
		GD.Print("To Main Button in new_gui pressed");
		SaveGame();
		timer = 0;
		timerStarter = true;
		GetTree().Root.AddChild(ResourceLoader.Load<PackedScene>("res://Scenes/main_menu.tscn").Instantiate());
		
	}
	
	private void _on_confirm_button_pressed()
	{
		GD.Print("_on_confirm_button_pressed() in main.cs called");
		setUpCompleted = true;
		}
	
	private void _on_main_menu_btn_pressed()
	{
		// IN WIN
		// Replace with function body.
	}
	


	public void onMainMenuClicked()
	{
		DataHandler.isGameOver = false;
		
		var gameScene = ResourceLoader.Load<PackedScene>("res://Scenes/main_menu.tscn").Instantiate();
		
		GetTree().Root.AddChild(gameScene);
	}

	// Speicher Mechanik
	
	public static DataHandler.SaveData LoadGame()
	{
		if (System.IO.File.Exists(DataHandler.saveFilePath))
		{
			// Load the existing save game
			string saveDataJson = System.IO.File.ReadAllText(DataHandler.saveFilePath);
			DataHandler.SaveData saveData = JsonSerializer.Deserialize<DataHandler.SaveData>(saveDataJson);
			string Fen = DataHandler.ReplaceFenAfterFourthSlash(saveData);
			saveData.Fen = Fen;
			return saveData;
		}
		else
		{
			// Create a new save game
			var saveData = CreateNewGame(0);

			// Save the new save game
			string saveDataJson = JsonSerializer.Serialize(saveData);
			System.IO.File.WriteAllText(DataHandler.saveFilePath, saveDataJson);
			
			return saveData;
		}
	}

	private static DataHandler.SaveData CreateNewGame(int gold)
	{
			DataHandler.SaveData saveData = new DataHandler.SaveData();
			saveData.Level = 0;
			saveData.Fen = DataHandler.GetRandomFenForLevel(saveData.Level);
			saveData.Score = 0;
			saveData.Gold = gold;
			saveData.BoughtPieces = new List<ShopCard.Item>();
			saveData.BoughtPowerups = new List<ShopCard.Item>();
			saveData.BoughtItems = new List<ShopCard.Item>();
			// StartPieces hinzufügen
			AddStartPieces(saveData.BoughtPieces);
			//Fen durch Startpieces ersetzten
			string Fen = DataHandler.ReplaceFenAfterFourthSlash(saveData);
			saveData.Fen = Fen;

			return saveData;
	}

	public static void SaveGame()
	{
		string saveDataJson = JsonSerializer.Serialize(DataHandler.GameState);
		System.IO.File.WriteAllText(DataHandler.saveFilePath, saveDataJson);
	}

	public static void StartNewGame()
	{
		System.IO.File.Delete(DataHandler.saveFilePath);
		DataHandler.GameState = LoadGame();
	}

	public static void AddStartPieces(List<ShopCard.Item> BoughtPieces)
	{
		// Stücke erstellen
		ShopCard.Item pawn = new ShopCard.Item("P", 50, "res://Sprites/Pieces2D/whitePawn.png", new float[] { 0.9f, 0.8f, 0.7f }, ShopCard.Type.Piece, "P");
		ShopCard.Item rook = new ShopCard.Item("R", 50, "res://Sprites/Pieces2D/whiteRook.png", new float[] { 0.2f, 0.6f, 0.7f }, ShopCard.Type.Piece, "R");
		ShopCard.Item knight = new ShopCard.Item("N", 50, "res://Sprites/Pieces2D/whiteKnight.png", new float[] { 0.6f, 0.6f, 0.6f }, ShopCard.Type.Piece, "N");
		ShopCard.Item bishop = new ShopCard.Item("B", 50, "res://Sprites/Pieces2D/whiteBishop.png", new float[] { 0.6f, 0.6f, 0.6f }, ShopCard.Type.Piece, "B");
		ShopCard.Item queen = new ShopCard.Item("Q", 50, "res://Sprites/Pieces2D/whiteQueen.png", new float[] { 0.10f, 0.15f, 0.2f }, ShopCard.Type.Piece, "Q");
		ShopCard.Item king = new ShopCard.Item("K", 50, "res://Sprites/Pieces2D/whiteKing.png", new float[] { 0.10f, 0.15f, 0.2f }, ShopCard.Type.Piece, "K");
		ShopCard.Item pawnbow = new ShopCard.Item("O", 50, "res://Sprites/Pieces2D/whitePawnBow.png", new float[] { 0.10f, 0.15f, 0.2f }, ShopCard.Type.Piece, "O");

		// 3 Pawns hinzufügen
		for (int i = 0; i < 3; i++)
		{
			BoughtPieces.Add(pawn);
		}

		// 1 King, 1 Bishop und 1 Knight hinzufügen
		BoughtPieces.Add(king);
		BoughtPieces.Add(bishop);
		BoughtPieces.Add(knight);
	}
}





