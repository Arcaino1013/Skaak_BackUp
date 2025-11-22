using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public partial class DataHandler : Node
{


	public struct Move
	{
		public int From { get; set; }
		public int To { get; set; }
		public Move(int a, int b)
		{
			From = a;
			To = b;
		}
	}

	public int[] pieceValues = {3,5, 2, -1000,100000, 100000, 3, 4, 6, 1, 3, 4, 9, 11, 5};

	public static bool isGameOver = false;

	public static int moveCounter = 0;
	public static string safedKingMovement = "";

	public static string[] assets = new string[]
	{
		"res://Sprites/Pieces2D/whiteBishop.png",
		"res://Sprites/Pieces2D/whiteBishopExComm.png",
		"res://Sprites/Pieces2D/whiteCompassPawn.png",
		"res://Sprites/FieldEffects/field_effect_ice.png",
		"res://Sprites/Pieces2D/whiteKing.png",
		"res://Sprites/Pieces2D/whiteKingMimic.png",
		"res://Sprites/Pieces2D/whiteKnight.png",
		"res://Sprites/Pieces2D/whiteLifeAfterKnight.png",
		"res://Sprites/Pieces2D/whiteLifeAfterRook.png",
		"res://Sprites/Pieces2D/whitePawn.png",
		"res://Sprites/Pieces2D/whitePawnBow.png",
		"res://Sprites/Pieces2D/whitePawnHunter.png",
		"res://Sprites/Pieces2D/whiteQueen.png",
		"res://Sprites/Pieces2D/whiteQueenCharm.png",
		"res://Sprites/Pieces2D/whiteRook.png",
		"res://Sprites/Pieces2D/blackBishop.png",
		"res://Sprites/Pieces2D/blackBishopExComm.png",
		"res://Sprites/Pieces2D/blackCompassPawn.png",
		"res://Sprites/FieldEffects/field_effect_ice.png",
		"res://Sprites/Pieces2D/blackKing.png",
		"res://Sprites/Pieces2D/blackKingMimic.png",
		"res://Sprites/Pieces2D/blackKnight.png",
		"res://Sprites/Pieces2D/blackLifeAfterKnight.png",
		"res://Sprites/Pieces2D/blackLifeAfterRook.png",
		"res://Sprites/Pieces2D/blackPawn.png",
		"res://Sprites/Pieces2D/blackPawnBow.png",
		"res://Sprites/Pieces2D/blackPawnHunter.png",
		"res://Sprites/Pieces2D/blackQueen.png",
		"res://Sprites/Pieces2D/blackQueenCharm.png",
		"res://Sprites/Pieces2D/blackRook.png",
	};

	public static string[] FreezedPieceAssets = new string[]
	{
		"res://Sprites/FieldEffects/FreezedPieces/WhiteBishop.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteBishopExComm.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteCompassPawn.png",
		"res://Sprites/FieldEffects/field_effect_ice.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteKing.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteKingMimic.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteKnight.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteLifeAfterKnight.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteLifeAfterRook.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhitePawn.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhitePawnBow.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhitePawnHunter.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteQueen.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteQueenCharm.png",
		"res://Sprites/FieldEffects/FreezedPieces/WhiteRook.png",
	};

	public enum PieceNames{BISHOP, BISHOPEXCOMM, COMPASSPAWN, FIELD_ICE, KING, KINGMIMIC, KNIGHT, LIFEAFTERKNIGHT, LIFEAFTERROOK, PAWN, PAWNBOW, PAWNHUNTER, QUEEN, QUEENCHARM, ROOK}


	public enum PieceNames_secondary
	{
		WHITE_BISHOP,
		WHITE_BISHOPEXCOMM,
		WHITE_COMPASSPAWN,
		WHITE_FIELD_ICE,
		WHITE_KING,
		WHITE_KINGMIMIC,
		WHITE_KNIGHT,
		WHITE_LIFEAFTERKNIGHT,
		WHITE_LIFEAFTERROOK,
		WHITE_PAWN,
		WHITE_PAWNBOW,
		WHITE_PAWNHUNTER,
		WHITE_QUEEN,
		WHITE_QUEENCHARM,
		WHITE_ROOK,
		BLACK_BISHOP,
		BLACK_BISHOPEXCOMM,
		BLACK_COMPASSPAWN,
		BLACK_FIELD_ICE,
		BLACK_KING,
		BLACK_KINGMIMIC,
		BLACK_KNIGHT,
		BLACK_LIFEAFTERKNIGHT,
		BLACK_LIFEAFTERROOK,
		BLACK_PAWN,
		BLACK_PAWNBOW,
		BLACK_PAWNHUNTER,
		BLACK_QUEEN,
		BLACK_QUEENCHARM,
		BLACK_ROOK
	}
	public static string GetRandomFenForLevel(int level)
	{
		Random random = new Random();
		int randomIndex = random.Next(0, 4); // Zufällige Zahl zwischen 0 und 3 (einschließlich)
		return StartFens[level, randomIndex];
	}

	public static string[,] StartFens = new string[,]
	{
		{
			"2kn2/1pppp1/6/6/6/6",
			"2kb2/1pppp1/6/6/6/6",
			"2nk2/1pppp1/6/6/6/6",
			"2bk2/1pppp1/6/6/6/6"
		},
		{
			"1bkn2/ppppp1/6/6/6/6",
			"1nkb2/ppppp1/6/6/6/6",
			"2nkb1/1ppppp/6/6/6/6",
			"2bkn1/1ppppp/6/6/6/6"
		},
		{
			"1qkn2/ppppp1/6/6/6/6",
			"1qkb2/ppppp1/6/6/6/6",
			"2bkq1/1ppppp/6/6/6/6",
			"2nkq1/1ppppp/6/6/6/6"
		},
		{
			"1rknb1/ppppp1/6/6/6/6",
			"1bkrn1/ppppp1/6/6/6/6",
			"1rbkn1/1ppppp/6/6/6/6",
			"1nbkr1/1ppppp/6/6/6/6"
		},
		{
			"1rknb1/pppppp/6/6/6/6",
			"1bkrn1/pppppp/6/6/6/6",
			"1rbkn1/pppppp/6/6/6/6",
			"1nbkr1/pppppp/6/6/6/6"
		},
		{
			"1rkqb1/pppppp/6/6/6/6",
			"1qkrn1/pppppp/6/6/6/6",
			"1rbkq1/pppppp/6/6/6/6",
			"1nqkr1/pppppp/6/6/6/6"
		}
	};

	public enum SlotStates
	{
		NONE,
		FREE,
		SELF,
		LAST_FROM,
		LAST_TO
	}


	public Dictionary<string, PieceNames_secondary> fen_dict = new Dictionary<string, PieceNames_secondary>()
	{
		{"b", PieceNames_secondary.BLACK_BISHOP},
		{"e", PieceNames_secondary.BLACK_BISHOPEXCOMM},
		{"c", PieceNames_secondary.BLACK_COMPASSPAWN},
		{"f", PieceNames_secondary.BLACK_FIELD_ICE},
		{"k", PieceNames_secondary.BLACK_KING},
		{"g", PieceNames_secondary.BLACK_KINGMIMIC},
		{"n", PieceNames_secondary.BLACK_KNIGHT},
		{"l", PieceNames_secondary.BLACK_LIFEAFTERKNIGHT},
		{"a", PieceNames_secondary.BLACK_LIFEAFTERROOK},
		{"p", PieceNames_secondary.BLACK_PAWN},
		{"o", PieceNames_secondary.BLACK_PAWNBOW},
		{"h", PieceNames_secondary.BLACK_PAWNHUNTER},
		{"q", PieceNames_secondary.BLACK_QUEEN},
		{"m", PieceNames_secondary.BLACK_QUEENCHARM},
		{"r", PieceNames_secondary.BLACK_ROOK},
		{"B", PieceNames_secondary.WHITE_BISHOP},
		{"E", PieceNames_secondary.WHITE_BISHOPEXCOMM},
		{"C", PieceNames_secondary.WHITE_COMPASSPAWN},
		{"F", PieceNames_secondary.WHITE_FIELD_ICE},
		{"K", PieceNames_secondary.WHITE_KING},
		{"G", PieceNames_secondary.WHITE_KINGMIMIC},
		{"N", PieceNames_secondary.WHITE_KNIGHT},
		{"L", PieceNames_secondary.WHITE_LIFEAFTERKNIGHT},
		{"A", PieceNames_secondary.WHITE_LIFEAFTERROOK},
		{"P", PieceNames_secondary.WHITE_PAWN},
		{"O", PieceNames_secondary.WHITE_PAWNBOW},
		{"H", PieceNames_secondary.WHITE_PAWNHUNTER},
		{"Q", PieceNames_secondary.WHITE_QUEEN},
		{"M", PieceNames_secondary.WHITE_QUEENCHARM},
		{"R", PieceNames_secondary.WHITE_ROOK}
	};

	public static Dictionary<string, Texture2D> GameTextures = new Dictionary<string, Texture2D>()
	{
		{"Stage1_white", GD.Load<Texture2D>("res://Textures/FieldTextures/ice.png")},
		{"Stage1_black", GD.Load<Texture2D>("res://Textures/FieldTextures/warped_nylium.png")},
		{"Boss1_white", GD.Load<Texture2D>("res://Textures/FieldTextures/stone_bricks.png")},
		{"Boss1_black", GD.Load<Texture2D>("res://Textures/FieldTextures/polished_blackstone_bricks.png")},
		{"Stage2_white", GD.Load<Texture2D>("res://Textures/FieldTextures/sand.png")},
		{"Stage2_black", GD.Load<Texture2D>("res://Textures/FieldTextures/nether_wart_block.png")},
		{"Boss2_white", GD.Load<Texture2D>("res://Textures/FieldTextures/sandstone.png")},
		{"Boss2_black", GD.Load<Texture2D>("res://Textures/FieldTextures/red_nether_bricks.png")},
		{"Stage1_Background", GD.Load<Texture2D>("res://Textures/backgrounds/Stage1.png")},
		{"Boss1_Background", GD.Load<Texture2D>("res://Textures/backgrounds/Boss1.png")},
		{"Stage2_Background", GD.Load<Texture2D>("res://Textures/backgrounds/Stage2.jpeg")},
		{"Boss2_Background", GD.Load<Texture2D>("res://Textures/backgrounds/Boss2.jpeg")},

	};
	
	public static ShopCard.Item[] Player_Inventory = new ShopCard.Item[32];


	public static Dictionary<char, int> FenDict = new Dictionary<char, int>(){
		{'b', 0},{'e', 1},{'c',2},{'f', 3},{'k', 4},{'g', 5},{'n', 6},{'l',7},{'a',8},{'p', 9},{'o', 10} ,{'h', 11} , {'q', 12}, {'m', 13}, {'r', 14}
	};

	public static string saveFilePath = System.IO.Path.Combine(OS.GetUserDataDir(), "savegame.json");
	public class SaveData
	{
		public int Level { get; set; }
		public int Score { get; set; }          // unused ?
		public string Fen { get; set; }
		public int Gold { get; set; }
		public List<ShopCard.Item> BoughtPieces { get; set; }
		public List<ShopCard.Item> BoughtPowerups { get; set; }
		public List<ShopCard.Item> BoughtItems { get; set; }

	}

	public static float volume;
	public static bool volume_changed;
	public static int difficulty;

	public class FieldEffect
	{
		public int Effect { get; set; }
		public int Round { get; set; }
		public int Location { get; set; }

		public FieldEffect(int effect, int round)
		{
			Effect = effect;
			Round = round;
		}
	}

	public static List<DataHandler.FieldEffect> fieldEffects;

	public static SaveData GameState { get; set; }

	public override void _Ready()
	{
		// Do nothing in _Ready for this class
	}

	public override void _Process(double delta)
	{
		// Do nothing in _Process for this class
	}
	public static string ReplaceFenAfterFourthSlash(DataHandler.SaveData saveData)
	{
		// FEN String auslesen
		string fen = saveData.Fen;

		// FEN String in Teile aufteilen
		string[] fenParts = fen.Split('/');

		// Prüfen, ob der FEN String mindestens 5 Teile hat (damit wir nach dem 4. Slash ersetzen können)
		if (fenParts.Length < 5)
		{
			throw new ArgumentException("FEN string must have at least 5 parts separated by '/'");
		}

		// String aus GetBoughtPiecesNames Methode bekommen
		string boughtPiecesNames = GetBoughtPiecesNames(saveData.BoughtPieces);

		// Teile nach dem 4. Slash ersetzen
		StringBuilder newFen = new StringBuilder();
		for (int i = 0; i < 4; i++)
		{
			newFen.Append(fenParts[i]);
			newFen.Append('/');
		}
		newFen.Append(boughtPiecesNames);

		return newFen.ToString();
	}
	public static string GetBoughtPiecesNames(List<ShopCard.Item> boughtPieces)
	{
		const int maxPieces = 12;
		const int piecesPerRow = 6;
		var result = new StringBuilder(maxPieces + 2); // 12 characters + 2 slashes
		int count = 0;

		foreach (var item in boughtPieces)
		{
			if (count == maxPieces) break;
			if (count > 0 && count % piecesPerRow == 0)
			{
				result.Append('/');
			}
			GD.Print("Item: ", item.Name, " &backend: ", item.backendName);
			result.Append(item.Name);
			count++;
		}

		if (count < maxPieces)
		{
			result.Append(new string('1', maxPieces - count)); // Append '1' for each missing piece
		}

		return result.ToString();
	}
}
