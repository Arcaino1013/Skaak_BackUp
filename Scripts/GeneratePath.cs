using Godot;
using System;
using System.Linq;

public partial class GeneratePath : Node
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public ulong PawnBowPath(int PawnBowPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong legalMoves = 0;

		if (isBlack)
		{
			if (PawnBowPosition >= 6)
			{
				// Normale Bewegungen wie Bauer
				if ((selfboard & (1UL << (PawnBowPosition - 6))) == 0 && (enemyboard & (1UL << (PawnBowPosition - 6))) == 0)
				{
					legalMoves |= 1UL << (PawnBowPosition - 6);
				}
				if ((enemyboard & (1UL << (PawnBowPosition - 7))) != 0 && (PawnBowPosition % 6 != 0))
				{
					legalMoves |= 1UL << (PawnBowPosition - 7);
				}
				if ((enemyboard & (1UL << (PawnBowPosition - 5))) != 0 && (PawnBowPosition % 6 != 5))
				{
					legalMoves |= 1UL << (PawnBowPosition - 5);
				}
				// Neuer spezieller Zug für schwarzen PawnBow: Angreifen zwei Felder vor sich
				// Angreifen ein Feld vor sich
				if ((enemyboard & (1UL << (PawnBowPosition - 6))) != 0)
				{
					legalMoves |= 1UL << (PawnBowPosition - 6);
				}
				// Angreifen zwei Felder vor sich
				if ((enemyboard & (1UL << (PawnBowPosition - 12))) != 0 && (selfboard & (1UL << (PawnBowPosition - 6))) == 0)
				{
					legalMoves |= 1UL << (PawnBowPosition - 12);
				}		
			}
		}
		else
		{
			if (PawnBowPosition <= 29)
			{
				// Normale Bewegungen wie Bauer
				if ((selfboard & (1UL << (PawnBowPosition + 6))) == 0 && (enemyboard & (1UL << (PawnBowPosition + 6))) == 0)
				{
					legalMoves |= 1UL << (PawnBowPosition + 6);
				}
				if ((enemyboard & (1UL << (PawnBowPosition + 7))) != 0 && (PawnBowPosition % 6 != 5))
				{
					legalMoves |= 1UL << (PawnBowPosition + 7);
				}
				if ((enemyboard & (1UL << (PawnBowPosition + 5))) != 0 && (PawnBowPosition % 6 != 0))
				{
					legalMoves |= 1UL << (PawnBowPosition + 5);
				}
				// Neuer spezieller Zug für weißen PawnBow: Angreifen ein Feld vor sich
				// Angreifen ein Feld vor sich
				if ((enemyboard & (1UL << (PawnBowPosition + 6))) != 0)
				{
					legalMoves |= 1UL << (PawnBowPosition + 6);
				}
				// Angreifen zwei Felder vor sich
				if ((PawnBowPosition + 12) <= 35 && (enemyboard & (1UL << (PawnBowPosition + 12))) != 0 && ((selfboard | enemyboard) & (1UL << (PawnBowPosition + 6))) == 0)
				{
					legalMoves |= 1UL << (PawnBowPosition + 12);
				}
			}
		}

		return legalMoves;
	}

	public ulong RookPath(int rookPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong legalMoves = 0;
		//right
		for (int i = rookPosition + 1; i <= 35 && i % 6 != 0; i++)
		{
			if ((enemyboard & (1UL << i)) != 0)
			{
				legalMoves |= 1UL << i;
				break;
			}
			if ((selfboard & (1UL << i)) != 0)
			{
				break;
			}
			legalMoves |= 1UL << i;
		}
		//left
		for (int i = rookPosition - 1; i >= 0 && i % 6 != 5; i--)
		{
			if ((enemyboard & (1UL << i)) != 0)
			{
				legalMoves |= 1UL << i;
				break;
			}
			if ((selfboard & (1UL << i)) != 0)
			{
				break;
			}
			legalMoves |= 1UL << i;
		}
		//up
		for (int i = rookPosition + 6; i < 36; i += 6)
		{
			if ((enemyboard & (1UL << i)) != 0)
			{
				legalMoves |= 1UL << i;
				break;
			}
			if ((selfboard & (1UL << i)) != 0)
			{
				break;
			}
			legalMoves |= 1UL << i;
		}
		//down
		for (int i = rookPosition - 6; i >= 0; i -= 6)
		{
			if ((enemyboard & (1UL << i)) != 0)
			{
				legalMoves |= 1UL << i;
				break;
			}
			if ((selfboard & (1UL << i)) != 0)
			{
				break;
			}
			legalMoves |= 1UL << i;
		}
		return legalMoves;
	}

	public ulong KnightPath(int knightPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong knightMoves = 0;

		int row = knightPosition / 6;
		int col = knightPosition % 6;

		// Alle möglichen Bewegungen eines Ritters
		int[][] offsets = new int[][]
		{
			new int[] { 2, 1 },
			new int[] { 2, -1 },
			new int[] { -2, 1 },
			new int[] { -2, -1 },
			new int[] { 1, 2 },
			new int[] { 1, -2 },
			new int[] { -1, 2 },
			new int[] { -1, -2 }
		};

		foreach (int[] offset in offsets)
		{
			int newRow = row + offset[0];
			int newCol = col + offset[1];

			if (newRow >= 0 && newRow < 6 && newCol >= 0 && newCol < 6)
			{
				int targetPosition = newRow * 6 + newCol;
				ulong targetMask = 1UL << targetPosition;

				// Nur Züge berücksichtigen, die nicht auf eigenen Figuren landen
				if ((selfboard & targetMask) == 0)
				{
					knightMoves |= targetMask;
				}
			}
		}

		return knightMoves;
	}

	public ulong KingPath(int kingPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong legalMoves = 0;
		int[] kingMoves = {-7, -6, -5, -1, 1, 5, 6, 7};
		foreach (int move in kingMoves)
		{
			int newPosition = kingPosition + move;
			if (newPosition >= 0 && newPosition < 36 && Math.Abs((newPosition % 6) - (kingPosition % 6)) <= 1)
			{
				if ((selfboard & (1UL << newPosition)) == 0)
				{
					legalMoves |= 1UL << newPosition;
				}
			}
		}
		return legalMoves & ~selfboard;
	}

	public ulong BishopPath(int bishopPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong legalMoves = 0;
		//top left
		for (int i = bishopPosition + 7; i <= 35 && i % 6 != 0; i += 7)
		{
			if ((enemyboard & (1UL << i)) != 0)
			{
				legalMoves |= 1UL << i;
				break;
			}
			if ((selfboard & (1UL << i)) != 0)
			{
				break;
			}
			legalMoves |= 1UL << i;
		}
		//top right
		for (int i = bishopPosition + 5; i <= 35 && i % 6 != 5; i += 5)
		{
			if ((enemyboard & (1UL << i)) != 0)
			{
				legalMoves |= 1UL << i;
				break;
			}
			if ((selfboard & (1UL << i)) != 0)
			{
				break;
			}
			legalMoves |= 1UL << i;
		}
		//bottom left
		for (int i = bishopPosition - 5; i >= 0 && i % 6 != 0; i -= 5)
		{
			if ((enemyboard & (1UL << i)) != 0)
			{
				legalMoves |= 1UL << i;
				break;
			}
			if ((selfboard & (1UL << i)) != 0)
			{
				break;
			}
			legalMoves |= 1UL << i;
		}
		//bottom right
		for (int i = bishopPosition - 7; i >= 0 && i % 6 != 5; i -= 7)
		{
			if ((enemyboard & (1UL << i)) != 0)
			{
				legalMoves |= 1UL << i;
				break;
			}
			if ((selfboard & (1UL << i)) != 0)
			{
				break;
			}
			legalMoves |= 1UL << i;
		}
		return legalMoves;
	}


	public ulong QueenPath(int queenPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		return RookPath(queenPosition, selfboard, enemyboard, isBlack) | BishopPath(queenPosition, selfboard, enemyboard, isBlack);
	}

	public ulong PawnPath(int pawnPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong legalMoves = 0;

		bool isFieldEffectInFront = DataHandler.fieldEffects.Any(effect => 35 - effect.Location == pawnPosition + 6);
		bool isFieldEffectLeft = DataHandler.fieldEffects.Any(effect => 35 - effect.Location == pawnPosition + 5);
		bool isFieldEffectRight = DataHandler.fieldEffects.Any(effect => 35 - effect.Location == pawnPosition + 7);

		if (isBlack)
		{
			if (pawnPosition >= 6)
			{
				if ((selfboard & (1UL << (pawnPosition - 6))) == 0 && (enemyboard & (1UL << (pawnPosition - 6))) == 0)
				{
					legalMoves |= 1UL << (pawnPosition - 6);
				}
				if ((enemyboard & (1UL << (pawnPosition - 7))) != 0 && (pawnPosition % 6 != 0))
				{
					legalMoves |= 1UL << (pawnPosition - 7);
				}
				if ((enemyboard & (1UL << (pawnPosition - 5))) != 0 && (pawnPosition % 6 != 5))
				{
					legalMoves |= 1UL << (pawnPosition - 5);
				}
			}
		}
		else
		{
			if (pawnPosition <= 29)
			{
				if ((selfboard & (1UL << (pawnPosition + 6))) == 0 && (enemyboard & (1UL << (pawnPosition + 6))) == 0)
				{
					legalMoves |= 1UL << (pawnPosition + 6);
				}
				if ((enemyboard & (1UL << (pawnPosition + 6))) != 0 && isFieldEffectInFront)
				{
					legalMoves |= 1UL << (pawnPosition + 6);
				}
				if ((enemyboard & (1UL << (pawnPosition + 7))) != 0 && (pawnPosition % 6 != 5) && !isFieldEffectRight)
				{
					legalMoves |= 1UL << (pawnPosition + 7);
				}
				if ((enemyboard & (1UL << (pawnPosition + 5))) != 0 && (pawnPosition % 6 != 0) && !isFieldEffectLeft)
				{
					legalMoves |= 1UL << (pawnPosition + 5);
				}
			}
		}
		return legalMoves;
	}
	public ulong CompassPawnPath(int pawnPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong legalMoves = 0;
		
		legalMoves = KingPath(pawnPosition, selfboard, enemyboard, isBlack);

		return legalMoves;
	}
	public ulong SetUpPath(int position, ulong selfboard, ulong enemyboard)
	{
		ulong legalMoves = 0;

		// Überprüfen, ob die Position in den ersten zwei Reihen von Weiß ist
		if (position >= 0 && position < 12)
		{
			// Erlauben der Bewegung zu allen Slots in den ersten zwei Reihen
			for (int i = 0; i < 12; i++)
			{
				// Überprüfen, ob das Zielfeld nicht von einer eigenen Figur besetzt ist
				if ((selfboard & (1UL << i)) == 0)
				{
					legalMoves |= 1UL << i;
				}
			}
		}

		return legalMoves;
	}

	public ulong FieldEffectPath(int kingPosition, ulong selfboard, ulong enemyboard, bool isBlack) => 0;

	public ulong KingMimicPath(int kingPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong legalMoves = 0;

		string safedKingMovement = DataHandler.safedKingMovement; // Annahme: Die Variable DataHandler.safedKingMovement enthält den Namen der erlaubten Bewegung.
		legalMoves = KingPath(kingPosition, selfboard, enemyboard, isBlack);

		// Überprüfen, ob zusätzlich zu den Königszügen auch andere spezifizierte Züge erlaubt sind
		switch (safedKingMovement)
		{
			case "QueenPath":
				legalMoves |= QueenPath(kingPosition, selfboard, enemyboard, isBlack);
				break;
			case "RookPath":
				legalMoves |= RookPath(kingPosition, selfboard, enemyboard, isBlack);
				break;
			case "BishopPath":
				legalMoves |= BishopPath(kingPosition, selfboard, enemyboard, isBlack);
				break;
			case "KnightPath":
				legalMoves |= KnightPath(kingPosition, selfboard, enemyboard, isBlack);
				break;
			default:
				// Fallback: Nur die Königsbewegungen erlauben
				break;
		}
					
		return legalMoves;
	}
	public ulong PawnHunterPath(int pawnPosition, ulong selfboard, ulong enemyboard, bool isBlack)
	{
		ulong legalMoves = 0;

		// Include two squares attack moves
		int pawnBowPosition = pawnPosition; // Assuming PawnBowPosition refers to the same position as pawnPosition

		// Attack two squares ahead (right)
		if ((pawnBowPosition % 6) < 4 && (enemyboard & (1UL << (pawnBowPosition + 2))) != 0 && ((selfboard | enemyboard) & (1UL << (pawnBowPosition + 1))) == 0)
		{
			legalMoves |= 1UL << (pawnBowPosition + 2);
		}

		// Attack two squares ahead (left)
		if ((pawnBowPosition % 6) > 1 && (enemyboard & (1UL << (pawnBowPosition - 2))) != 0 && ((selfboard | enemyboard) & (1UL << (pawnBowPosition - 1))) == 0)
		{
			legalMoves |= 1UL << (pawnBowPosition - 2);
		}

		// Attack two squares ahead (up)
		if ((pawnBowPosition / 6) > 1 && (enemyboard & (1UL << (pawnBowPosition - 12))) != 0 && ((selfboard | enemyboard) & (1UL << (pawnBowPosition - 6))) == 0)
		{
			legalMoves |= 1UL << (pawnBowPosition - 12);
		}

		// Attack two squares ahead (down)
		if ((pawnBowPosition / 6) < 4 && (enemyboard & (1UL << (pawnBowPosition + 12))) != 0 && ((selfboard | enemyboard) & (1UL << (pawnBowPosition + 6))) == 0)
		{
			legalMoves |= 1UL << (pawnBowPosition + 12);
		}

		// Include KingPath moves
		legalMoves |= KingPath(pawnPosition, selfboard, enemyboard, isBlack);

		return legalMoves;
	}

}
