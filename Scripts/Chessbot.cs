using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks; // Für Task.Delay

public partial class Chessbot : Node
{

	public int maxDepth = 2;
	public int searchCounter = 0;
	//public int maxDepth = 2;			// standard (auch mit DataHander --> kann aber jetzt geändert sein)
	
	public Bitboard currentboard;

	public DataHandler.Move currentMove = new(-1, -1);

	public DataHandler DH = new();

	public override void _Ready() 
	{
		if(DataHandler.difficulty != 0)
		{
			maxDepth = DataHandler.difficulty;
		}	
		GD.Print("depht: ", maxDepth);
	}

	public override void _Process(double delta) { }

	public void initBot(Bitboard board)
	{
		currentboard = board;
	}

	public int SearchMoves(bool isBlackMove, int depth, Bitboard searchBoard, int alpha = int.MinValue + 2, int beta = int.MaxValue)
	{
		searchCounter++;
		if (depth == 0)
		{
			return Evaluate(isBlackMove, searchBoard);
		}

		List<DataHandler.Move> moves = searchBoard.GenerateMoveSet(isBlackMove);

		foreach (DataHandler.Move move in moves)
		{
			Bitboard newBoard = new();
			newBoard.SetBoard(searchBoard.whitePieces, searchBoard.blackPieces);
			newBoard.MakeMove(move, isBlackMove);
			
			int evaluation = -SearchMoves(!isBlackMove, depth - 1, newBoard, -beta, -alpha);
			if (depth == maxDepth && evaluation > alpha)
			{
				currentMove.From = move.From;
				currentMove.To = move.To;
			}
			alpha = Math.Max(evaluation, alpha);
			if (evaluation >= beta)
			{
				return beta;
			}
		}

		return alpha;
	}

	public async Task<int[]> FindNextMove()
	{
		SearchMoves(true, maxDepth, currentboard);
		int[] nextMove = { currentMove.From, currentMove.To };

		await Task.Delay(700); // Warte 1 Sekunde

		currentboard.MakeMove(currentMove, true);
		MusicManager.PlaySoundEffect("ChessPieceMove");
		DataHandler.moveCounter += 1;

		return nextMove;
	}

	public int Evaluate(bool isBlackMove, Bitboard searchboard)
	{
		int whiteValues = pieceValues(searchboard.whitePieces);
		int blackValues = pieceValues(searchboard.blackPieces);
		int evaluation = whiteValues - blackValues;

		return isBlackMove ? -evaluation : evaluation;
	}

	public int pieceValues(ulong[] pieces)
	{
		int totalValue = 0;
		for (int i = 0; i < DataHandler.FenDict.Count; i++)
		{
			totalValue += BitOperations.PopCount(pieces[i]) * DH.pieceValues[i];
		}
		return totalValue;
	}
}
