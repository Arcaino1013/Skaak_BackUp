using Godot;
using System;
using System.Linq;
using System.Text.Json.Serialization;

public partial class ShopCard : Control
{
	public class Item
	{
		public int Price { get; set; }
		public string Name { get; set; }

		public string backendName { get; set; }
		public string TexturePath { get; set; }
		public float[] Rarities { get; set; }
		[JsonIgnore]
		public Texture2D Texture2D { get; set; }
		public bool IsShown { get; set; }
		public Type type { get; set; }
		private PackedScene animationManager = (PackedScene)ResourceLoader.Load("res://Scenes/animation_manager.tscn");
		public AnimatedSprite2D sprite2dManager;

		// Parameterloser Konstruktor für die Deserialisierung
		public Item() { }

		// Konstruktor mit Parametern, die den Eigenschaften entsprechen (wird für LoadGame benötigt)
		public Item(int price, string name, string texturePath, float[] rarities, bool isShown, Type type)
		{
			Price = price;
			Name = name;
			TexturePath = texturePath;
			Rarities = rarities;
			IsShown = isShown;
			this.type = type;
		}

		public Item(string name = "standard item", int price = 0, string texture_path = "res://Shop/PowerUps/Item_0.png", float[] rarities = null,Type type = Type.Undefined, string backendName_in = "not required")
		{
			Name = name;
			Price = price;
			TexturePath = texture_path;
			Rarities = rarities ?? new float[] { 0.5f, 0.5f, 0.5f };
			if(texture_path != "" || texture_path != String.Empty)
			{
				Texture2D = GD.Load<Texture2D>(texture_path);
			}
			IsShown = false;
			this.type = type;
			backendName = backendName_in;
		}
	}
	
	public enum Type
	{
		Undefined,
		Piece,
		PowerUp,
		SingleUseItem
	}
	
	// // ITEM CREATION
	#region
	private static Item soldOut = new Item("sold out", 100000, "res://Textures/SkaakBoardShaderBasic/CheckerAlbedo.png", new float[] { 0.0f, 0.0f, 0.0f }, Type.PowerUp);
	//PowerUps
	private static Item Bogen = new Item("Bow", 40, "", new float[] { 0.75f, 0.70f, 0.65f }, Type.PowerUp);
	
	private static Item Exkommunikation = new Item("Excommunication", 45, "", new float[] { 0.5f, 0.6f, 0.7f }, Type.PowerUp);
	private static Item Leben_nach_dem_Pferd = new Item("Life after Knight", 45, "", new float[] { 0.5f, 0.65f, 0.75f }, Type.PowerUp);
	
	private static Item Leben_nach_den_Mauern = new Item("Life after Rook", 45, "", new float[] { 0.5f, 0.65f, 0.75f }, Type.PowerUp);
	
	private static Item Liebesamulett = new Item("Amulet of love", 150, "", new float[] { 0.05f, 0.1f, 0.3f }, Type.PowerUp);

	private static Item Mimic = new Item("Mimic", 100, "", new float[] { 0.11f, 0.21f, 0.31f }, Type.PowerUp);

	// Pieces
	private static Item pawn = new Item("P", 55, "res://Sprites/Pieces2D/whitePawn.png", new float[] { 0.80f, 0.70f, 0.60f }, Type.Piece, "P");
	private static Item bishop = new Item("B", 75, "res://Sprites/Pieces2D/whiteBishop.png", new float[] { 0.5f, 0.55f, 0.6f }, Type.Piece, "B");
	private static Item knight = new Item("N", 75, "res://Sprites/Pieces2D/whiteKnight.png", new float[] { 0.5f, 0.55f, 0.6f }, Type.Piece, "N");
	private static Item rook = new Item("R", 95, "res://Sprites/Pieces2D/whiteRook.png", new float[] { 0.3f, 0.6f, 0.7f }, Type.Piece, "R");
	private static Item queen = new Item("Q", 135, "res://Sprites/Pieces2D/whiteQueen.png", new float[] { 0.10f, 0.25f, 0.50f }, Type.Piece, "Q");

	// not in Shop:
	private static Item king = new Item("K", 1000, "res://Sprites/Pieces2D/whiteKing.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece, "K");
	private static Item pawnbow = new Item("O", 50, "res://Sprites/Pieces2D/whitePawnBow.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece, "O");
	private static Item lifeafterknight = new Item("L", 50, "res://Sprites/Pieces2D/whiteLifeAfterKnight.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece, "L");
	private static Item lifeafterrook = new Item("A", 50, "res://Sprites/Pieces2D/whiteLifeAfterKnight.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece, "A");
	private static Item bishopexcomm = new Item("E", 50, "res://Sprites/Pieces2D/whiteBishopExComm.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece, "E");
	private static Item queencharm = new Item("M", 50, "res://Sprites/Pieces2D/whiteQueenCharm.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece, "M");
	private static Item kingmimic = new Item("G", 50, "res://Sprites/Pieces2D/whiteKingMimic.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece, "G");

	// Single Use Items
	private static Item Holzschild = new Item("Holzschild", 25,"" , new float[] { 0.7f, 0.4f, 0.3f }, Type.SingleUseItem);
	private static Item Eisenschild = new Item("Eisenschild", 50, "", new float[] { 0.5f, 0.55f, 0.6f }, Type.SingleUseItem);
	private static Item royalerSchild = new Item("royaler Schild", 105, "", new float[] { 0.3f, 0.35f, 0.4f }, Type.SingleUseItem);
	#endregion
	
	
	private RichTextLabel priceLabel;
	private RichTextLabel nameLabel;
	private TextureRect itemImage;
	private AnimatedSprite2D itemAnimation;
	private Button buyButton;
	private RichTextLabel descriptionLabel;


	public static Item[] shopContents = new Item[15];

	public static int ShopVersion = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		//Labels und Bilder refernzieren
		priceLabel = GetNode<RichTextLabel>("%price");
		nameLabel = GetNode<RichTextLabel>("%itemname");
		itemImage = GetNode<TextureRect>("%ItemImage");
		itemAnimation = GetNode<AnimatedSprite2D>("%ItemAnimation");
		buyButton = GetNode<Button>("%BuyButton");
		descriptionLabel = GetNode<RichTextLabel>("%Description");

		// Connect the signals for mouse hover
		itemImage.Connect("mouse_entered", Callable.From(_on_item_image_mouse_entered));
		itemImage.Connect("mouse_exited", Callable.From(_on_item_image_mouse_exited));
	
		
		// shopContents mit PowerUps füllen
		shopContents[0] = soldOut;
		shopContents[1] = Bogen;		
		shopContents[2] = Leben_nach_dem_Pferd;
		shopContents[3] = Leben_nach_den_Mauern;
		shopContents[4] = Exkommunikation;
		shopContents[5] = Liebesamulett;
		shopContents[6] = Mimic;
		
		// shopContents mit Pieces füllen
		shopContents[7] = pawn;
		shopContents[8] = rook;
		shopContents[9] = knight;
		shopContents[10] = bishop;
		shopContents[11] = queen;
		
		// shopContents mit SingleUseItems füllen
		shopContents[12] = Holzschild;
		shopContents[13] = Eisenschild;
		shopContents[14] = royalerSchild;
		

		// für die intial angezeigten Items
		Reroll(true);
		
		
	}
	  private Item GetDisplayedItem()
	{
		foreach (var item in shopContents)
		{
			if (item.Name == nameLabel.Text)
			{
				return item;
			}
		}
		return null;
	}

	private void DisplayItem(Item item)
	{
		if(item.Price == 100000)
		{
			priceLabel.Text = "-";
			buyButton.Text = "sold out";
		}
		else
		{
			priceLabel.Text = item.Price.ToString()+" Gold";
			buyButton.Text =  item.Price.ToString()+" Gold";
		}
		switch (item.Name)
				{
					case "R":
						nameLabel.Text = "Rook";
						break;
					case "P":
						nameLabel.Text = "Pawn";
						break;
					case "Q":
						nameLabel.Text = "Queen";
						break;
					case "B":
						nameLabel.Text = "Bishop";
						break;
					case "N":
						nameLabel.Text = "Knight";
						break;
					default:
						nameLabel.Text = item.Name;
						break;
				}
		if(item.Texture2D == null)
		{
			itemAnimation.Play(item.Name);
			itemImage.Material._Set("hide",true);
		} else
		{
			Shader shader = (Shader)GD.Load("res://Shader/ShopCard.gdshader");
			ShaderMaterial material = new ShaderMaterial();
			material.Shader = shader;
			itemImage.Material = material;
			itemImage.Texture = item.Texture2D;
			itemImage.Material._Set("hide",true);
			itemAnimation.Hide();
		}
	}


	public void Reroll(bool calledbyReady = false, bool verbose = false)
	{
		Type type = getType();
		bool itemPicked = false;
		Random indexRandom = new Random();
		Random rarityRandom = new Random();
		int randomIndex;
		float probability;
		int maxAttempts = 90; // Maximum number of attempts to prevent endless loop
		int attemptCount = 0;
		// staffelt Wahrscheinlichkeiten (bessere Items wieter im Spiel wahrscheinlicher)
		ShopVersion = DataHandler.GameState.Level switch
		{
			0 or 1 => 0,
			2 or 3 => 1,
			4 or 5 => 2,
			_ => 1,
		};

		do
		{
			randomIndex = indexRandom.Next(1, shopContents.Length);
			probability = rarityRandom.Next(100) / 100F;
			if (verbose)
			{
				GD.Print("probability: ", probability);
				GD.Print("shopContents.Length: ", randomIndex);
				GD.Print("shopContents[randomIndex]: ", shopContents[randomIndex]);
				GD.Print("shopContents[randomIndex].Name: ", shopContents[randomIndex].Name);
				GD.Print("shopContents[randomIndex].type: ", shopContents[randomIndex].type);
				GD.Print("type: ", type);
			}

			if (shopContents[randomIndex].type == type)
			{
				if (!shopContents[randomIndex].IsShown)
				{
					if (shopContents[randomIndex].Rarities[ShopVersion] > probability)
					{
						itemPicked = true;
						shopContents[randomIndex].IsShown = true;
					}
				}
			}

			attemptCount++;
			if (attemptCount >= maxAttempts)
			{
				break;
			}

		} while (!itemPicked);

		if (!itemPicked)
		{
			GD.Print("Reroll failed to find a valid item after max attempts.");
		}
		else
		{
			DisplayItem(shopContents[randomIndex]);
		}
	}
	
	/// <summary>
	/// Returns the Item-Type of the parent VBoxContainer - Piece, PowerUp or SingleUseItem - pay attention to the naming of the Boxes
	/// </summary>
	/// <param name="verbose">returns more information if true</param>
	/// <returns></returns>
	public Type getType(bool verbose = false)
	{
		if(verbose)
		{
		GD.Print("getType() called");
		}
		// könnte verschälert werden
		// genau auf meine Benamung (HBoxContainer denk ich) angepasst 
		string nodeParentName = GetParent().Name;
		string[] splits = nodeParentName.Split('_');
		string typeString = splits[1].Remove(splits[1].Length-1); // cuts of the s at the end of the word
		Type typeValue;
		if (Enum.TryParse(typeString, true, out typeValue))
		{
			return typeValue;
		}
		else
		{
			if(verbose)
			{
			GD.Print("Invalid type string");
			}
			return Type.Undefined;
		}
	}
	
	//bools isShown wieder auf false gesetzt werden, dass beim nächsten Shopbesuch die Items wieder gekauft werden können 
	public void recycleItems()
	{
		foreach (var item in shopContents)
		{
			item.IsShown = false;
		}
	}

	public Item getItem()
	{
		foreach (var item in shopContents)
		{
			if (item.Name == nameLabel.Text)
			{
				return item;
			}
			else if(nameLabel.Text == "Bishop")
			{
				return shopContents[10];
			}
			else if(nameLabel.Text == "Rook" )
			{
				return shopContents[8];
			}
			else if(nameLabel.Text == "Pawn" )
			{
				return shopContents[7];
			}
			else if(nameLabel.Text == "Knight")
			{
				return shopContents[9];
			}
			else if(nameLabel.Text == "Queen")
			{
				GD.Print(item.Name);
				return shopContents[11];
			}
		}

		GD.Print("getItem() in ShopCard.cs was unsucessful and now returns null");
		return null;
	}

	public void Buy()
	{    
		Item i = getItem();
		int countBoughtPieces = CountBoughtPieces();

		// Bestimme die Kosten des Gegenstands
		int cost = Convert.ToInt32(priceLabel.Text.Split(' ')[0]);

		// Überprüfe den Typ des Gegenstands und handle entsprechend
		if (i.type == Type.Piece && countBoughtPieces < 12)
		{        
			if (DataHandler.GameState.Gold >= cost)
			{
				DataHandler.GameState.Gold -= cost;
				DisplayItem(shopContents[0]);

				// Überprüfe, ob es ein passendes Powerup gibt
				var matchingPowerup = DataHandler.GameState.BoughtPowerups.FirstOrDefault(p => IsMatchingPowerup(p, i));

				if (matchingPowerup != null)
				{
					// Entferne das gefundene Powerup aus BoughtPowerups
					DataHandler.GameState.BoughtPowerups.Remove(matchingPowerup);

					// Erstelle eine Figur mit dem Powerup
					var figureWithPowerup = CreateFigureWithPowerup(i, matchingPowerup);

					// Füge die Figur mit Powerup zu BoughtPieces hinzu
					DataHandler.GameState.BoughtPieces.Add(figureWithPowerup);
				}
				else
				{
					// Füge das Powerup zu BoughtPowerups hinzu
					DataHandler.GameState.BoughtPieces.Add(i);
					
				}
			}
		}
		else if (i.type == Type.PowerUp)
		{
			if (DataHandler.GameState.Gold >= cost)
			{
				DataHandler.GameState.Gold -= cost;
				DisplayItem(shopContents[0]);

				// Überprüfe, ob es eine passende Figur gibt
				var matchingFigure = DataHandler.GameState.BoughtPieces.FirstOrDefault(f => IsMatchingFigure(f, i));

				if (matchingFigure != null)
				{
					// Entferne die gefundene Figur aus BoughtPieces
					DataHandler.GameState.BoughtPieces.Remove(matchingFigure);

					// Erstelle eine neue Figur mit dem Powerup
					var figureWithPowerup = CreateFigureWithPowerup(matchingFigure, i);

					// Füge die Figur mit Powerup zu BoughtPieces hinzu
					DataHandler.GameState.BoughtPieces.Add(figureWithPowerup);
				}
				else
				{
					// Füge das Powerup zu BoughtPowerups hinzu
					DataHandler.GameState.BoughtPowerups.Add(i);
				}
			}
		}
		else if (i.type == Type.SingleUseItem)
		{
			if (DataHandler.GameState.Gold >= cost)
			{
				DataHandler.GameState.Gold -= cost;
				DisplayItem(shopContents[0]);
				DataHandler.GameState.BoughtItems.Add(i);
			}
		}
	}
		


	private bool IsMatchingPowerup(ShopCard.Item powerup, ShopCard.Item figure)
	{
		
		return (powerup.Name == "Bow" && figure.Name == "P") ||
		   (powerup.Name == "Life after Knight" && figure.Name == "N") ||
		   (powerup.Name == "Life after Rook" && figure.Name == "R") ||
		   (powerup.Name == "Excommunication" && figure.Name == "B") ||
		   (powerup.Name == "Amulet of love" && figure.Name == "Q") ||
		   (powerup.Name == "Mimic" && figure.Name == "K");
	}

	private bool IsMatchingFigure(ShopCard.Item figure, ShopCard.Item powerup)
	{
		return (figure.Name == "P" && powerup.Name == "Bow") ||
		   (figure.Name == "N" && powerup.Name == "Life after Knight") ||
		   (figure.Name == "R" && powerup.Name == "Life after Rook") ||
		   (figure.Name == "B" && powerup.Name == "Excommunication") ||
		   (figure.Name == "Q" && powerup.Name == "Amulet of love") ||
		   (figure.Name == "K" && powerup.Name == "Mimic");
	}


	private ShopCard.Item CreateFigureWithPowerup(ShopCard.Item figure, ShopCard.Item powerup)
	{
		// Erstelle eine neue Figur mit dem Powerup basierend auf den Eigenschaften der Figur   
		switch (figure.Name)
		{
			case "P":
				return new ShopCard.Item("O", 50, "res://Sprites/Pieces2D/whitePawnBow.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece);
			// Fügen Sie hier weitere Cases für andere Figuren hinzu, falls benötigt
			case "N":
				return new ShopCard.Item("L", 50, "res://Sprites/Pieces2D/whiteLifeAfterKnight.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece);
			case "R":
				return new ShopCard.Item("A", 50, "res://Sprites/Pieces2D/whiteLifeAfterRook.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece);
			case "B":
				return new ShopCard.Item("E", 50, "res://Sprites/Pieces2D/whiteBishopExComm.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece);
			case "Q":
				return new ShopCard.Item("M", 50, "res://Sprites/Pieces2D/whiteQueenCharm.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece);
			case "K":
				return new ShopCard.Item("G", 50, "res://Sprites/Pieces2D/whiteKingMimic.png", new float[] { 0.10f, 0.15f, 0.2f }, Type.Piece);
			default:
				return figure; // Falls keine passende Figur gefunden wurde, gib einfach die ursprüngliche Figur zurück
		}
	}

	public static int CountBoughtPieces()
	{
		// Zähler initialisieren
		int count = 0;

		// Liste durchgehen und Elemente zählen
		foreach (var item in DataHandler.GameState.BoughtPieces)
		{
			count++;
		}

		// Anzahl der Elemente zurückgeben
		return count;
	}


	private void _on_buy_button_pressed()
	{
		GD.Print("BuyButton pressed in ShopCard.cs");
		Buy();
	}
	
	private void _on_item_image_mouse_entered()
	{
		
		Item currentItem = GetDisplayedItem();
		if (currentItem != null)
		{
			AssignItemDescription(currentItem);
		}
	}
	
	private void _on_item_image_mouse_exited()
	{
		descriptionLabel.Visible = false;
	}

	private void AssignItemDescription(Item currentItem)
	{
		switch (currentItem.Name)
		{
			case "Bow":
				descriptionLabel.Text = "Allows the pawn to attack two squares forward without moving.";
				break;
			case "Life after Knight":
				descriptionLabel.Text = "When the knight is captured, a pawn remains, which will be placed on the player's back rank. If no space is available, it is placed in the next row.";
				break;
			case "Excommunication":
				descriptionLabel.Text = "If the player's bishop is adjacent to an opponent's knight or bishop, the opponent's piece can be demoted to a pawn.";
				break;
			case "Life after Rook":
				descriptionLabel.Text = "When the rook is captured, a pawn remains, which will be placed on the player's back rank. If no space is available, it is placed in the next row.";
				break;
			case "Amulet of love":
				descriptionLabel.Text = "The queen can make enemy pawns switch sides and change color, provided they are directly adjacent (AOE: cross).";
				break;
			case "Mimic":
				descriptionLabel.Text = "Copies movement of an adjacent piece with the following priority: Queen, Rook, Bishop, Knight.";
				break;
			case "P":
				descriptionLabel.Text = "The pawn may only move one square forwards vertically at a time. It is the only piece in the game of chess that may not move backwards. Furthermore, a pawn cannot move forwards if the square in front of it is blocked by another piece. It captures opposing pieces diagonally one square away.";
				break;
			case "B":
				descriptionLabel.Text = "The bishop moves and captures diagonally. It can move as far as it likes in one direction. Because the bishops move diagonally, they never change colour. Bishops cannot jump over other pieces.";
				break;
			case "N":
				descriptionLabel.Text = "The knight moves and captures identically. It moves two squares horizontally or vertically and then one square to the left, right, up or down. The knight's move resembles an 'L'. The knight is the only chess piece that can jump over other pieces. Tip: When a knight moves, it always ends up on a square of the other colour.";
				break;
			case "R":
				descriptionLabel.Text = "The Rook moves and strikes vertically or horizontally. It can move as far as it likes in one direction. Rooks cannot jump over other pieces.";
				break;
			case "Q":
				descriptionLabel.Text = "The queen moves and captures in the same way. It moves diagonally, vertically or horizontally. How far you move your queen in one direction is up to you. However, the queen is not allowed to jump over other pieces.";
				break;
			default:
				descriptionLabel.Text = "No description available.";
				break;
		}
		descriptionLabel.Visible = true;
	}

}






