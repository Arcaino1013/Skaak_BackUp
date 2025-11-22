using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class Board : Control
{
	private GridContainer board_grid;
	PackedScene gameOverScreen = ResourceLoader.Load<PackedScene>("res://Scenes/GameOver.tscn");
	private PackedScene slot_scene = ResourceLoader.Load<PackedScene>("res://Scenes/Slot.tscn");
	private PackedScene piece_scene = ResourceLoader.Load<PackedScene>("res://Scenes/piece.tscn");
	private PackedScene shop_scene = ResourceLoader.Load<PackedScene>("res://Scenes/Shop.tscn");
	private PackedScene win_scene = ResourceLoader.Load<PackedScene>("res://Scenes/Win.tscn");
	private Panel chess_board;
	private Node bitboard;
	private Node GeneratePath;
	Node gameOverScene;
	private Node chessBot;
	private Node shop;
	private Node win;
	private Texture2D wihtefieldTexture;
	private Texture2D blackfieldTexture;
	private TextureRect fieldTexureRect;
	private Slot[] grid_array = new Slot[36];
	private Piece[] piece_array = new Piece[36];
	private Vector2 icon_offset = new Vector2(39, 39);
	private Vector2 icon_size = new Vector2(40, 40);
	private string fen = "";
	private Piece piece_selected = null;
	private bool gamestart = false;
	private bool isKingFreezed = false;
	private Piece freezed_piece = null;
	private int freezed_piece_slot = -1;
	private bool gameOver = false; // Variable, um den Spielstatus zu verfolgen
	private bool setUp = true;
	private Piece piece_selected_2 = null;
	private int lastFromLoc = -1;
	private int lastToLoc = -1;

	private bool blackKingPresentCheck = false;

	public override void _Ready()
	{
		wihtefieldTexture = getTextures(true);
		blackfieldTexture = getTextures(false);
		board_grid = GetNode<GridContainer>("ChessBoard/BoardMargin/BoardGrid");
		bitboard = GetNode<Node>("Bitboard");
		chess_board = GetNode<Panel>("ChessBoard");
		GeneratePath = GetNode<Node>("GeneratePath");
		chessBot = GetNode<Node>("Chessbot");
		shop = shop_scene.Instantiate();
		win = win_scene.Instantiate();

		gameOverScene = gameOverScreen.Instantiate();
		gameOverScene.QueueFree();

		fen = DataHandler.GameState.Fen;
		DataHandler.fieldEffects = new List<DataHandler.FieldEffect>();

		Vector2 windowSize = DisplayServer.WindowGetSize();
		icon_size = new Vector2(windowSize.X / 20, windowSize.X / 20);
		chess_board.Size = new Vector2(windowSize.X / 2, windowSize.X / 2);
		board_grid.Size = new Vector2(windowSize.X / 2 - 20, windowSize.X / 2 - 20);
		var tileSize = new Vector2(board_grid.Size.X / 6, board_grid.Size.Y / 6);

		for (int i = 0; i < 36; i++)
		{
			CreateSlot();
		}

		int colorBit = 0;
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				
				if (j % 2 == colorBit)
				{
					grid_array[i * 6 + j].SetBackground(new Color(0.616f, 0.78f, 0.922f)); // Color.Bisque equivalent
					
					fieldTexureRect = (TextureRect)grid_array[i * 6 + j].GetChild(0).GetChild(0);
					fieldTexureRect.Texture = wihtefieldTexture;
					grid_array[i * 6 + j].SetSize(tileSize);
				}
				else
				{
					fieldTexureRect = (TextureRect)grid_array[i * 6 + j].GetChild(0).GetChild(0);
					//fieldTexureRect.Texture = blackfieldTexture;
				}
			}
			colorBit = (colorBit == 0) ? 1 : 0;
		}

		// Fill the array with default values
		for (int i = 0; i < piece_array.Length; i++)
		{
			piece_array[i] = new Piece(); // Assuming Piece has a default constructor
		}

		CallDeferred(nameof(load_game));
	}

	private Texture2D getTextures(bool white)
	{
		switch (DataHandler.GameState.Level) 
		{
		case 0:
		case 1:
			// World 1 Levels 
			if(white)
			{
			return DataHandler.GameTextures["Stage1_white"];
			}
			else
			{
				return DataHandler.GameTextures["Stage1_black"];
			}
		case 2:
			// World 1 Boss
			if(white)
			{
			return DataHandler.GameTextures["Boss1_white"];
			}
			else
			{
				return DataHandler.GameTextures["Boss1_black"];
			}
		case 3:
		case 4:
			// World 2 Levels
			if(white)
			{
			return DataHandler.GameTextures["Stage2_white"];
			}
			else
			{
				return DataHandler.GameTextures["Stage2_black"];
			}
		case 5:
			// World 2 Boss
			if(white)
			{
			return DataHandler.GameTextures["Boss2_white"];
			}
			else
			{
				return DataHandler.GameTextures["Boss2_black"];
			}
		 
		default:
			if(white)
			{
				return GD.Load<Texture2D>("res://Textures/FieldTextures/chiseled_quartz_block_top.png");
			}
			else
			{
				return GD.Load<Texture2D>("res://Textures/FieldTextures/black_wool.png");
			}
		}

		
		
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("mouse_right") && piece_selected != null)
		{
			RightClick();
		}
	}

	private void RightClick() //Soll das tun was der Aufruf des Events mouse_right getan hat
	{
		piece_selected = null;
		ClearFreeBoardFilter();
	}

	private void CreateSlot()
	{
		Slot new_slot = (Slot)slot_scene.Instantiate();

		new_slot.SlotClicked += _OnSlotClicked;
		new_slot.slot_ID = board_grid.GetChildCount();
		board_grid.AddChild(new_slot);
		grid_array[new_slot.slot_ID] = new_slot;
	}

	private async void _OnSlotClicked(Slot slot)
	{
		if (gameOver) return;
		if (piece_selected == null) return;

		if (slot.state != DataHandler.SlotStates.FREE) return;

		MovePiece(piece_selected, slot.slot_ID);

		// Füge ein neuen Feldeffekt hinzu, welcher dann wiederum Random eine Runde erhält, wann dieser erscheint
		if(DataHandler.fieldEffects.Count < 5 && DataHandler.GameState.Level == 2) { AddFutureFieldEffect((int)DataHandler.PieceNames_secondary.BLACK_FIELD_ICE); }

		// Füge den Feldeffekt hinzu, wenn die Runde erreicht wurde
		AddFieldEffectToBoard();

		// Händle die Effekte auf dem Feld, wenn eine Figur darauf bewegt wurde
		HandleFieldEffects(piece_selected, slot.slot_ID);


		ClearBoardFilter();
		piece_selected = null;

		blackKingPresentCheck = false;

		foreach (Piece piece in piece_array)
		{
			if (piece != null)
			{
				if (piece.type == (int)DataHandler.PieceNames_secondary.BLACK_KING)
				{
					blackKingPresentCheck = true;
				}
			}
		}

		if (gamestart && blackKingPresentCheck == true)
		{
			int[] move = await ((Chessbot)chessBot).FindNextMove(); // Hier warten wir auf den nächsten Zug des Bots
			UpdateBoard(35 - move[0], 35 - move[1]);
		}

		CheckGameOver();
	}

	private void UpdateBoard(int from_loc, int to_loc)
	{
		if (piece_array[to_loc] != null)
		{
			if (piece_array[to_loc].type == (int)DataHandler.PieceNames_secondary.WHITE_LIFEAFTERKNIGHT || 
				piece_array[to_loc].type == (int)DataHandler.PieceNames_secondary.WHITE_LIFEAFTERROOK)
			{
				PlaceRandomPiece(24, 36, DataHandler.PieceNames_secondary.WHITE_PAWN);
			}

			piece_array[to_loc].QueueFree();
			piece_array[to_loc] = null;
		}

		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(piece_array[from_loc], "global_position", grid_array[to_loc].GlobalPosition + icon_offset, 0.1f);
		piece_array[to_loc] = piece_array[from_loc];
		piece_array[from_loc] = null;
		piece_array[to_loc].slot_ID = to_loc;

		// Speichern der letzten Zugpositionen
		lastFromLoc = from_loc;
		lastToLoc = to_loc;

		// Anwenden der Filter für den letzten Zug
		ApplyLastMoveFilter();
	}

	private void ApplyLastMoveFilter()
	{
		ClearBoardFilter();

		if (lastFromLoc != -1)
		{
			grid_array[lastFromLoc].SetFilter(DataHandler.SlotStates.LAST_FROM);
		}
		if (lastToLoc != -1)
		{
			grid_array[lastToLoc].SetFilter(DataHandler.SlotStates.LAST_TO);
		}
	}

	private int PlaceRandomPiece(int startIndex, int endIndex, DataHandler.PieceNames_secondary piece)
	{
		List<int> possibleLocations = new List<int>();
		int location = -1;

		for (int i = startIndex; i < endIndex; i++)
		{
			if (piece_array[i] == null)
			{
				possibleLocations.Add(i);
			}
		}

		if (possibleLocations.Count > 0)
		{
			Random random = new Random();
			int randomIndex = random.Next(possibleLocations.Count);
			location = possibleLocations[randomIndex];
			AddPiece((int)piece, location);
		}

		return location;
	}


	private void MovePiece(Piece piece, int location)
	{
		if(freezed_piece != null && freezed_piece_slot > -1)
		{
			RemoveFreezeFromPiece();
		}

		if (isBlack(piece)) return;

		bool isPawn = piece.type == (int)DataHandler.PieceNames_secondary.BLACK_PAWN || piece.type == (int)DataHandler.PieceNames_secondary.WHITE_PAWN;
		bool isPawnBow = piece.type == (int)DataHandler.PieceNames_secondary.BLACK_PAWNBOW || piece.type == (int)DataHandler.PieceNames_secondary.WHITE_PAWNBOW;
		bool isPawnHunter = piece.type == (int)DataHandler.PieceNames_secondary.BLACK_PAWNHUNTER || piece.type == (int)DataHandler.PieceNames_secondary.WHITE_PAWNHUNTER;
		bool isForwardMove = Math.Abs(piece.slot_ID - location) == 6;
		bool isTwoStepsForwardMove = Math.Abs(piece.slot_ID - location) == 12;
		bool isTwoStepsSideMove = Math.Abs(piece.slot_ID - location) == 2;
		bool isTwoStepsBackwardMove = Math.Abs(piece.slot_ID - location) == 12; // Same distance as forward, but direction depends on implementation
		bool isAttackMove = piece_array[location] != null;

		if (!setUp) { DataHandler.moveCounter += 1; }

		// Handle promotion for regular pawns
		if (isPawn || isPawnBow)
		{
			bool isWhite = piece.type == (int)DataHandler.PieceNames_secondary.WHITE_PAWN || piece.type == (int)DataHandler.PieceNames_secondary.WHITE_PAWNBOW;
			bool isPromotionRow = isWhite ? location < 6 : location >= 30;

			if (isPromotionRow)
			{
				PromotePawn(piece, location);
				return;
			}
		}

		if (isPawnBow)
		{
			if (isForwardMove && isAttackMove)
			{
				// PawnBow schlägt ein Feld vor sich, ohne sich zu bewegen
				RemoveFromBitboard(piece_array[location]);
				piece_array[location].QueueFree();
				piece_array[location] = null;
				MusicManager.PlaySoundEffect("ChessPieceTake");
			}
			else if (isTwoStepsForwardMove)
			{
				// PawnBow schlägt zwei Felder vor sich, bewegt sich aber nicht
				RemoveFromBitboard(piece_array[location]);
				piece_array[location].QueueFree();
				piece_array[location] = null;
				MusicManager.PlaySoundEffect("ChessPieceTake");
			}
			else
			{
				// PawnBow bewegt sich wie ein Bauer
				if (piece_array[location] != null)
				{
					RemoveFromBitboard(piece_array[location]);
					piece_array[location].QueueFree();
					piece_array[location] = null;
					MusicManager.PlaySoundEffect("ChessPieceTake");
				}
				PerformMove(piece, location);
				MusicManager.PlaySoundEffect("ChessPieceMove");
			}
		}
		else if (isPawnHunter)
		{
			if (isTwoStepsForwardMove || isTwoStepsSideMove || isTwoStepsBackwardMove)
			{
				// PawnHunter schlägt zwei Felder in jeder Richtung, bewegt sich aber nicht
				RemoveFromBitboard(piece_array[location]);
				piece_array[location].QueueFree();
				piece_array[location] = null;
				MusicManager.PlaySoundEffect("ChessPieceTake");
			}
			else
			{
				// PawnHunter bewegt sich wie normal
				if (piece_array[location] != null)
				{
					RemoveFromBitboard(piece_array[location]);
					piece_array[location].QueueFree();
					piece_array[location] = null;
					MusicManager.PlaySoundEffect("ChessPieceTake");
				}
				PerformMove(piece, location);
				MusicManager.PlaySoundEffect("ChessPieceMove");
			}
		}
		else
		{
			// Andere Figuren bewegen sich wie gewohnt
			if (piece_array[location] != null)
			{
				RemoveFromBitboard(piece_array[location]);
				piece_array[location].QueueFree();
				piece_array[location] = null;
				MusicManager.PlaySoundEffect("ChessPieceTake");
			}
			PerformMove(piece, location);
			MusicManager.PlaySoundEffect("ChessPieceMove");
		}

		// Überprüfen, ob das bewegte Stück der WHITE_BISHOPEXCOMM ist
	if (piece.type == (int)DataHandler.PieceNames_secondary.WHITE_BISHOPEXCOMM)
	{
		// Definiere den Radius
		int[] radiusOffsets = { -7, -6, -5, -1, 1, 5, 6, 7 };

		foreach (int offset in radiusOffsets)
		{
			int checkLocation = piece.slot_ID + offset;
			if (checkLocation >= 0 && checkLocation < 36) // Überprüfen, ob der Standort innerhalb der Brettgrenzen liegt
			{
				Piece targetPiece = piece_array[checkLocation];
				if (targetPiece != null && (targetPiece.type == (int)DataHandler.PieceNames_secondary.BLACK_KNIGHT || targetPiece.type == (int)DataHandler.PieceNames_secondary.BLACK_BISHOP))
				{
					// Entfernen der gefundenen Figur
					RemoveFromBitboard(targetPiece);
					targetPiece.QueueFree();
					piece_array[checkLocation] = null;

					// Ersetzen durch einen Black_Pawn
					AddPiece((int)DataHandler.PieceNames_secondary.BLACK_PAWN, checkLocation);

					// Aktualisieren Sie das Bitboard für das neue Stück
					Piece newPiece = piece_array[checkLocation];
					if (newPiece != null)
					{
						bitboard.Call("AddPiece", 35 - checkLocation, newPiece.type);
					}
				}
			}
		}
	}

		// Überprüfen, ob das bewegte Stück der WHITE_QUEENCHARM ist
		if (piece.type == (int)DataHandler.PieceNames_secondary.WHITE_QUEENCHARM)
		{
			// Definiere die Kreuzrichtungen: oben, unten, links, rechts
			int[] crossOffsets = { -6, 6, -1, 1 };

			foreach (int offset in crossOffsets)
			{
				int checkLocation = piece.slot_ID + offset;
				if (checkLocation >= 0 && checkLocation < 36) // Überprüfen, ob der Standort innerhalb der Brettgrenzen liegt
				{
					Piece targetPiece = piece_array[checkLocation];
					if (targetPiece != null && targetPiece.type == (int)DataHandler.PieceNames_secondary.BLACK_PAWN)
					{
						// Entfernen des gefundenen BLACK_PAWN
						RemoveFromBitboard(targetPiece);
						targetPiece.QueueFree();
						piece_array[checkLocation] = null;

						// Ersetzen durch einen WHITE_PAWN
						AddPiece((int)DataHandler.PieceNames_secondary.WHITE_PAWN, checkLocation);
					}
				}
			}
		}
	}

	private void PromotePawn(Piece piece, int location)
	{
		int newPieceType;

		switch (piece.type)
		{
			case (int)DataHandler.PieceNames_secondary.WHITE_PAWN:
				newPieceType = (int)DataHandler.PieceNames_secondary.WHITE_COMPASSPAWN;
				break;
			case (int)DataHandler.PieceNames_secondary.BLACK_PAWN:
				newPieceType = (int)DataHandler.PieceNames_secondary.BLACK_COMPASSPAWN;
				break;
			case (int)DataHandler.PieceNames_secondary.WHITE_PAWNBOW:
				newPieceType = (int)DataHandler.PieceNames_secondary.WHITE_PAWNHUNTER;
				break;
			case (int)DataHandler.PieceNames_secondary.BLACK_PAWNBOW:
				newPieceType = (int)DataHandler.PieceNames_secondary.BLACK_PAWNHUNTER;
				break;
			default:
				// Falls kein passender Typ gefunden wird, breche die Methode ab.
				return;
		}

		// Entferne die gegnerische Figur an der Ziellinie, falls vorhanden
		if (piece_array[location] != null)
		{
			RemoveFromBitboard(piece_array[location]);
			piece_array[location].QueueFree();
			piece_array[location] = null;
		}

		// Entferne den alten Bauer aus der Bitboard-Array und gib ihn frei
		RemoveFromBitboard(piece);

		// Speichern der Position des Bauern im Array
		int originalPosition = Array.IndexOf(piece_array, piece);
		if (originalPosition >= 0)
		{
			piece_array[originalPosition] = null;
		}

		piece.QueueFree();
		// Füge die neue Figur an der Array hinzu
		AddPiece(newPieceType, location);

		// Aktualisiere die Position der neuen Figur im Bitboard
		Piece newPiece = piece_array[location];
		if (newPiece != null)
		{
			bitboard.Call("AddPiece", 35 - location, newPiece.type);
		}
	}

	private bool isBlack(Piece piece)
	{
		if (piece.type < DataHandler.FenDict.Count) // nach Datahandler Logik ist alles unter 9 weiß
		{
			return false;
		}
		return true;
	}

	private void PerformMove(Piece piece, int location)
	{
		RemoveFromBitboard(piece);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(piece, "global_position", grid_array[location].GlobalPosition + icon_offset, 0.2f);
		piece_array[piece.slot_ID] = null;
		piece_array[location] = piece;
		piece.slot_ID = location;
		bitboard.Call("AddPiece", 35 - location, piece.type);
	}

	private void RemoveFromBitboard(Piece piece)
	{
		bitboard.Call("RemovePiece", 35 - piece.slot_ID, piece.type);
	}

	private void AddPiece(int piece_type, int location)
	{
		Piece new_piece = (Piece)piece_scene.Instantiate();
		chess_board.AddChild(new_piece);
		new_piece.type = piece_type;
		new_piece.LoadIcon(piece_type, icon_size, DataHandler.assets);
		new_piece.GlobalPosition = grid_array[location].GlobalPosition + icon_offset;
		GD.Print(new_piece.GlobalPosition);
		piece_array[location] = new_piece;
		new_piece.slot_ID = location;
		if(setUp) {new_piece.PieceSelected += _OnPieceSelectedSetup;}	
		else {new_piece.PieceSelected += _OnPieceSelected;}
	}

	private void HandleFieldEffects(Piece piece, int location)
	{
		if(CheckFieldEffectOnSlot(location))
		{
			ApplyFreezeToPiece(piece, location);
		}
	}

	private void AddFutureFieldEffect(int effect_type)
	{
		Random rnd = new Random();
		var fieldEffectRound = rnd.Next(1, 4) + DataHandler.moveCounter;
		DataHandler.fieldEffects.Add(new DataHandler.FieldEffect(effect_type, fieldEffectRound));
	}

	private void AddFieldEffectToBoard()
	{
		foreach(var effect in DataHandler.fieldEffects)
		{
			if(effect.Round == DataHandler.moveCounter)
			{
				var location = PlaceRandomPiece(0, 36, (DataHandler.PieceNames_secondary)effect.Effect);
				bitboard.Call("AddPiece", 35 - location, (int)effect.Effect);
				effect.Location = location;
			}
		}
	}

	private bool CheckFieldEffectOnSlot(int location) =>
		DataHandler.fieldEffects.Any(effect => effect.Round <= DataHandler.moveCounter && effect.Location == location);

	private void ApplyFreezeToPiece(Piece piece, int location)
	{
		freezed_piece = new Piece() { type = piece.type, slot_ID = piece.slot_ID };
		freezed_piece_slot = location;

		DataHandler.fieldEffects.RemoveAll(effect => effect.Location == location && effect.Round <= DataHandler.moveCounter);
		bitboard.Call("RemovePiece", 35 - piece.slot_ID, piece.type);
		piece_array[location].QueueFree();
		piece_array[location] = null;

		AddPiece((int)DataHandler.PieceNames_secondary.WHITE_FIELD_ICE, location);
		bitboard.Call("AddPiece", 35 - location, (int)DataHandler.PieceNames_secondary.WHITE_FIELD_ICE);
		piece_array[location].LoadIcon((int)piece.type, icon_size, DataHandler.FreezedPieceAssets);
		if(piece.type == (int)DataHandler.PieceNames_secondary.WHITE_KINGMIMIC || piece.type == (int)DataHandler.PieceNames_secondary.WHITE_KING)
		{
			isKingFreezed = true;
		}
	}

	private void RemoveFreezeFromPiece()
	{
		bitboard.Call("RemovePiece", freezed_piece_slot, 3);
		piece_array[freezed_piece_slot].QueueFree();
		piece_array[freezed_piece_slot] = null;
		CheckPawnPromotionAfterFreeze();

		AddPiece(freezed_piece.type, freezed_piece_slot);
		bitboard.Call("AddPiece", 35 - freezed_piece_slot, freezed_piece.type);

		isKingFreezed = false;
		freezed_piece = null;
		freezed_piece_slot = -1;
	}

	private void CheckPawnPromotionAfterFreeze()
	{
		if(freezed_piece_slot >= 0 && freezed_piece_slot <= 5 && freezed_piece.type == 9)
		{
			freezed_piece.type = 2;
		}
		if(freezed_piece_slot >= 0 && freezed_piece_slot <= 5 && freezed_piece.type == 10)
		{
			freezed_piece.type = 11;
		}
	}


	private void _OnPieceSelected(Piece piece)
	{

		if(!(DataHandler.moveCounter % 2 == 0)) return;
		if (gameOver) return;
		if (piece_selected != null)
		{
			_OnSlotClicked(grid_array[piece.slot_ID]);
		}
		else
		{
			piece_selected = piece;
			ulong WhiteBoard = (ulong)bitboard.Call("GetWhiteBitboard");
			ulong BlackBoard = (ulong)bitboard.Call("GetBlackBitboard");
			bool pieceIsBlack = true;
			string function_to_call = "";
			if (isBlack(piece) == false) // nach Datahandler Logik ist alles unter 6 weiß
			{
				pieceIsBlack = false;
			}
			else
			{
				RightClick();
				return;
			}

			switch (piece.type % DataHandler.FenDict.Count)
			{
				case 0:
					function_to_call = "BishopPath";
					break;
				case 1: // Power-Up Bishop
					function_to_call = "BishopPath";
					break;
				case 2: // Promoted Pawn
					function_to_call = "CompassPawnPath";
					break;
				case 3:
					function_to_call = "FieldEffectPath";
					break;
				case 4:
					function_to_call = "KingPath";
					break;
				case 5: // Power-Up King
					function_to_call = "KingMimicPath";
					break;
				case 6:
					function_to_call = "KnightPath";
					break;
				case 7: // Power-Up Knight
					function_to_call = "KnightPath";
					break;
				case 8: // Power-Up Rook
					function_to_call = "RookPath";
					break;
				case 9:
					function_to_call = "PawnPath";
					break;
				case 10: // Power-Up Pawn
					function_to_call = "PawnBowPath";
					break;
				case 11: // promote Power-Up Pawn
					function_to_call = "PawnHunterPath";
					break;
				case 12:
					function_to_call = "QueenPath";
					break;
				case 13: // Power-Up Queen
					function_to_call = "QueenPath";
					break;
				case 14:
					function_to_call = "RookPath";
					break;
			}

			if (pieceIsBlack)
			{
				SetBoardFilter((ulong)GeneratePath.Call(function_to_call, 35 - piece.slot_ID, BlackBoard, WhiteBoard, pieceIsBlack));

			}
			else
			{
				SetBoardFilter((ulong)GeneratePath.Call(function_to_call, 35 - piece.slot_ID, WhiteBoard, BlackBoard, pieceIsBlack));
				MusicManager.PlaySoundEffect("ChessPiecePickup");
			}
		
		}
	}

	private void ApplyKingPowerUp()
	{
		// Finde den König auf dem Brett
		int kingLocation = -1;
		for (int i = 0; i < piece_array.Length; i++)
		{
			if (piece_array[i] != null && piece_array[i].type == (int)DataHandler.PieceNames_secondary.WHITE_KINGMIMIC)
			{
				kingLocation = i;
				break;
			}
		}

		// Definiere die möglichen Offsets für den 1-Feld-Radius
		int[] offsets = new int[] { -7, -6, -5, -1, 1, 5, 6, 7 };

		// Überprüfe die Felder im 1-Feld-Radius um den König
		Piece strongestPiece = null;
		foreach (int offset in offsets)
		{
			int neighborLocation = kingLocation + offset;
			if (neighborLocation >= 0 && neighborLocation < piece_array.Length && piece_array[neighborLocation] != null)
			{
				Piece neighborPiece = piece_array[neighborLocation];
				// Nur Dame, Turm, Läufer und Springer berücksichtigen
				if (neighborPiece.type == (int)DataHandler.PieceNames_secondary.WHITE_QUEEN ||
					neighborPiece.type == (int)DataHandler.PieceNames_secondary.WHITE_ROOK ||
					neighborPiece.type == (int)DataHandler.PieceNames_secondary.WHITE_BISHOP ||
					neighborPiece.type == (int)DataHandler.PieceNames_secondary.WHITE_KNIGHT)
				{
					// Überprüfe die Priorität der Figur
					if (strongestPiece == null ||
						GetPiecePriority(neighborPiece.type) > GetPiecePriority(strongestPiece.type))
					{
						strongestPiece = neighborPiece;
					}
				}
			}
		}

		if (strongestPiece != null)
		{
			// Wende das Bewegungsmuster der stärksten Figur auf den König an
			ApplyMovementPatternToKing(strongestPiece.type);
		}
		else
		{
			// Keine starke Figur in der Nähe des Königs, Standardmäßig KingPath anwenden
			ApplyMovementPatternToKing((int)DataHandler.PieceNames_secondary.WHITE_KINGMIMIC);
		}
	}

	private void ApplyMovementPatternToKing(int pieceType)
	{
		switch (pieceType)
		{
			case (int)DataHandler.PieceNames_secondary.WHITE_QUEEN:
				DataHandler.safedKingMovement = "QueenPath";
				break;
			case (int)DataHandler.PieceNames_secondary.WHITE_ROOK:
				DataHandler.safedKingMovement = "RookPath";
				break;
			case (int)DataHandler.PieceNames_secondary.WHITE_BISHOP:
				DataHandler.safedKingMovement = "BishopPath";
				break;
			case (int)DataHandler.PieceNames_secondary.WHITE_KNIGHT:
				DataHandler.safedKingMovement = "KnightPath";
				break;
			default:
				DataHandler.safedKingMovement = "KingPath";
				break;
		}
	}

	private int GetPiecePriority(int pieceType)
	{
		switch (pieceType)
		{
			case (int)DataHandler.PieceNames_secondary.WHITE_QUEEN:
				return 4; // Höchste Priorität
			case (int)DataHandler.PieceNames_secondary.WHITE_ROOK:
				return 3;
			case (int)DataHandler.PieceNames_secondary.WHITE_BISHOP:
				return 2;
			case (int)DataHandler.PieceNames_secondary.WHITE_KNIGHT:
				return 1;
			default:
				return 0; // Bauern oder unbekannte Typen haben die niedrigste Priorität
		}
	}

	
	private void _OnPieceSelectedSetup(Piece piece)
	{
		if(!isBlack(piece))
		{
		// Check if we have two pieces selected for swapping
			if (piece_selected != null && piece_selected_2 == null && piece != null)
			{
				piece_selected_2 = piece;
				// Swap the pieces
				SwapPieces(piece_selected, piece_selected_2);
				piece_selected = null;
				piece_selected_2 = null;
				RightClick();
			}
		else
		{
			if (piece_selected != null)
			{
				_OnSlotClicked(grid_array[piece.slot_ID]);
			}
			else
				{
					// If only one piece is selected or none are selected
					if (piece_selected == null)
					{
						piece_selected = piece;
					}
					
					// Determine if the piece is black or white
					bool pieceIsBlack = isBlack(piece);

					// Logic for movement or special actions based on piece color
					if (!pieceIsBlack) // Assuming false means piece is white
					{
						// Logic for normal movement
						ulong WhiteBoard = (ulong)bitboard.Call("GetWhiteBitboard");
						ulong BlackBoard = (ulong)bitboard.Call("GetBlackBitboard");
						SetBoardFilterSetUp((ulong)GeneratePath.Call("SetUpPath", 35 - piece.slot_ID, WhiteBoard, BlackBoard));
					}
					else
					{
						RightClick(); // Placeholder for your action for black piece
						return;
					}
				}
		}
		}
	}

	private void SwapPieces(Piece piece1, Piece piece2)
	{
		// Get the current locations of the pieces
		int index1 = Array.IndexOf(piece_array, piece1);
		int index2 = Array.IndexOf(piece_array, piece2);

		// Remove pieces from bitboard at their old positions
		bitboard.Call("RemovePiece", 35 - index1, piece1.type);
		bitboard.Call("RemovePiece", 35 - index2, piece2.type);

		// Swap the pieces in the piece_array
		piece_array[index1] = piece2;
		piece_array[index2] = piece1;

		// Update their slot_IDs
		piece1.slot_ID = index2;
		piece2.slot_ID = index1;

		// Add pieces to bitboard at their new positions
		bitboard.Call("AddPiece", 35 - index1, piece2.type);
		bitboard.Call("AddPiece", 35 - index2, piece1.type);

		// Tween animation to visually swap their positions on the board
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(piece1, "global_position", grid_array[index2].GlobalPosition + icon_offset, 0.2f);
		tween.TweenProperty(piece2, "global_position", grid_array[index1].GlobalPosition + icon_offset, 0.2f);
	}

	private void SetBoardFilterSetUp(ulong bitmap)
	{
		for (int i = 0; i < 36; i++)
		{
			if ((bitmap & 1UL) == 1UL)
			{
				grid_array[35 - i].SetFilter(DataHandler.SlotStates.FREE);
			}
			else if (piece_array[35 - i] != null && !isBlack(piece_array[35 - i])) // Überprüft, ob es deine eigene Figur ist
			{
				grid_array[35 - i].SetFilter(DataHandler.SlotStates.SELF);
			}
			else
			{
				grid_array[35 - i].SetFilter(DataHandler.SlotStates.NONE);
			}
			bitmap >>= 1;
		}
	}

	private void SetBoardFilter(ulong bitmap)
	{
		for (int i = 0; i < 36; i++)
		{
			if ((bitmap & 1UL) == 1UL)
			{
				grid_array[35 - i].SetFilter(DataHandler.SlotStates.FREE);
			}
			bitmap >>= 1;
		}
	}

	private void ClearBoardFilter()
	{
		foreach (var slot in grid_array)
		{
			slot.SetFilter(DataHandler.SlotStates.NONE);
		}
	}

	private void ClearFreeBoardFilter()
	{
		foreach (var slot in grid_array)
		{
			if (slot.state == DataHandler.SlotStates.FREE || slot.state == DataHandler.SlotStates.SELF)
			{
				slot.SetFilter(DataHandler.SlotStates.NONE);
			}
		}
	}

	private void ParseFen(string fen)
	{
		DataHandler dataHandlerInstance = new DataHandler();
		string[] board_state = fen.Split(" ");
		int board_index = 0;
		foreach (var c in board_state[0])
		{
			if (c == '/')
			{
				continue;
			}
			if (int.TryParse(c.ToString(), out int num))
			{
				board_index += num;
			}
			else
			{
				AddPiece((int)dataHandlerInstance.fen_dict[c.ToString()], board_index);
				board_index++;
			}
		}
	}

	private List<Tuple<int, int>> SaveCurrentPieces()
	{
		List<Tuple<int, int>> piecesState = new List<Tuple<int, int>>();

		for (int i = 0; i < piece_array.Length; i++)
		{
			if (piece_array[i] != null)
			{
				piecesState.Add(new Tuple<int, int>(piece_array[i].type, piece_array[i].slot_ID));
			}
		}

		return piecesState;
	}

	private void LoadPieces(List<Tuple<int, int>> piecesState)
	{
		foreach (var pieceInfo in piecesState)
		{
			AddPiece(pieceInfo.Item1, pieceInfo.Item2);
		}
	}

	private void _on_test_button_pressed()
	{
		DataHandler.moveCounter = 0;
		gameOver = false;
		_on_clear_board_pressed();
		ParseFen(fen);
		bitboard.Call("InitBitBoard", fen);
		bitboard.Call("GetWhiteBitboard");
		bitboard.Call("GetBlackBitboard");
		gamestart = false;
	}

	private void _on_confirm_button_pressed()
	{
		setUp = false;
		gameOver = false;
		DataHandler.moveCounter = 0;
		// Speichere den aktuellen Zustand der Figuren
		List<Tuple<int, int>> piecesState = SaveCurrentPieces();
		_on_clear_board_pressed();
		LoadPieces(piecesState);
		ClearBoardFilter();
		ApplyKingPowerUp();
		piece_selected = null;
		chessBot.Call("initBot", bitboard);
		gamestart = true;
	}

	private async void _on_resign_button_pressed()
	{
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		GetTree().Root.AddChild(gameOverScreen.Instantiate());
		DataHandler.fieldEffects.Clear();
		DataHandler.isGameOver = true;
		gameOver = true;
	}

	private void load_game()
	{
		gameOver = false;
		_on_clear_board_pressed();
		ParseFen(fen);
		bitboard.Call("InitBitBoard", fen);
		bitboard.Call("GetWhiteBitboard");
		bitboard.Call("GetBlackBitboard");
		gamestart = false;
	}

	private void _on_start_game_btn_pressed()
	{
		DataHandler.moveCounter = 0;
		gameOver = false;
		_on_clear_board_pressed();
		ClearBoardFilter();
		piece_selected = null;
		ParseFen(fen);
		bitboard.Call("InitBitBoard", fen);
		chessBot.Call("initBot", bitboard);
		gamestart = true;
	}

	private void ClearPieceArray()
	{
		foreach (Piece piece in piece_array)
		{
			if (piece != null)
			{
				piece.QueueFree();
			}
		}
		Array.Clear(piece_array, 0, piece_array.Length);
	}

	private void _on_clear_board_pressed()
	{
		DataHandler.moveCounter = 0;
		ClearBoardFilter();
		ClearPieceArray();
	}

	private void CheckGameOver()
	{
		string winner = GetWinner();

		if (winner != null)
		{
			gameOver = true;
			return;
		}
	}

	private void LoadNextLevel()
	{
		DataHandler.GameState.Level++;
		DataHandler.GameState.Gold += 50;
		DataHandler.fieldEffects.Clear();

		DataHandler.GameState.Gold += DataHandler.GameState.Level*5;
		if(DataHandler.moveCounter < 50)
		{
		DataHandler.GameState.Gold += 50-DataHandler.moveCounter;
		}
		DataHandler.GameState.Score += 100;
		if (DataHandler.GameState.Level > DataHandler.StartFens.GetLength(0) - 1)
		{
			var resetGold = DataHandler.GameState.Gold * 0.2;
			DataHandler.GameState.Fen = DataHandler.GetRandomFenForLevel(0);
			DataHandler.GameState.Level = 0;
			DataHandler.GameState.Gold = (int)resetGold;

			GetTree().Root.AddChild(win);
		}
		else
		{
			DataHandler.GameState.Fen = DataHandler.GetRandomFenForLevel(DataHandler.GameState.Level);
			GetTree().Root.AddChild(shop);
		}

		Main.SaveGame();
	}

	private string GetWinner()
	{
		bool whiteKingPresent = false;
		bool blackKingPresent = false;

		foreach (Piece piece in piece_array)
		{
			if (piece != null)
			{
				if (piece.type == (int)DataHandler.PieceNames_secondary.WHITE_KING || piece.type == (int)DataHandler.PieceNames_secondary.WHITE_KINGMIMIC || isKingFreezed)
				{
					whiteKingPresent = true;
				}
				if (piece.type == (int)DataHandler.PieceNames_secondary.BLACK_KING)
				{
					blackKingPresent = true;
				}
			}
		}

		if (!whiteKingPresent && blackKingPresent)
		{
			GD.Print("White king is not present. Black wins.");
			_on_resign_button_pressed();
			return "Black";
		}
		if (whiteKingPresent && !blackKingPresent)
		{
			LoadNextLevel();
			return "White";
		}

		return null; // Falls beide Könige noch vorhanden sind, gibt es keinen Gewinner
	}
}

