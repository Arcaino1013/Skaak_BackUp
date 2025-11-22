using Godot;
using System;
using System.Collections.Generic;


public partial class Bitboard : Node
{
	public ulong[] whitePieces = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
	public ulong[] blackPieces = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
	const ulong lower36BitMask = 0xFFFFFFFFF; // 36 Einsen in Binärform

	public override void _Ready() {}

	public override void _Process(double delta) {}

	public ulong GetBlackBitboard() {
		ulong ans = 0;
		foreach (ulong i in blackPieces) {
			ans |= i;
		}
		return ans & lower36BitMask;
	}

	public ulong GetWhiteBitboard() {
		ulong ans = 0;
		foreach (ulong i in whitePieces) {
			ans |= i;
		}
		return ans & lower36BitMask;
	}

	public void ClearBitboard() {
		Array.Clear(whitePieces, 0, whitePieces.Length);
		Array.Clear(blackPieces, 0, blackPieces.Length);
	}

	public void SetBoard(ulong[] Whites, ulong[] Blacks) {
		Array.Copy(Whites, whitePieces, Whites.Length);
		Array.Copy(Blacks, blackPieces, Blacks.Length);
	}

	public void InitBitBoard(string fen) {
		ClearBitboard();
		string[] fen_split = fen.Split(" ");
		int shiftCount = 0;
		foreach (char i in fen_split[0]) {
			if (i == '/') continue;
			if (char.IsDigit(i)) {
				int shiftAmount = int.Parse(i.ToString());
				shiftCount += shiftAmount;
				continue;
			}
			shiftCount++;
			if (char.IsUpper(i)) {
				whitePieces[DataHandler.FenDict[char.ToLower(i)]] |= (1UL << (36 - shiftCount)) & lower36BitMask;
			} else {
				blackPieces[DataHandler.FenDict[i]] |= (1UL << (36 - shiftCount)) & lower36BitMask;
			}
		}
	}

	public void LeftShift(int shiftAmount) {
		for (int piece = 0; piece < blackPieces.Length; piece++) {
			blackPieces[piece] = (blackPieces[piece] << shiftAmount) & lower36BitMask;
		}
		for (int piece = 0; piece < whitePieces.Length; piece++) {
			whitePieces[piece] = (whitePieces[piece] << shiftAmount) & lower36BitMask;
		}
	}

	public ulong GetBitboard() {
		return whitePieces[5] & lower36BitMask;
	}

	public void RemovePiece(int location, int pieceType) {
		if (pieceType > (DataHandler.FenDict.Count-1)) {
			blackPieces[pieceType % DataHandler.FenDict.Count] &= ~(1UL << location) & lower36BitMask;
		} else {
			whitePieces[pieceType % DataHandler.FenDict.Count] &= ~(1UL << location) & lower36BitMask;
		}
	}

	public void AddPiece(int location, int pieceType) {
		if (pieceType > (DataHandler.FenDict.Count-1)) {
			blackPieces[pieceType % DataHandler.FenDict.Count] |= (1UL << location) & lower36BitMask;
		} else {
			whitePieces[pieceType % DataHandler.FenDict.Count] |= (1UL << location) & lower36BitMask;
		}
	}

	public void MakeMove(DataHandler.Move move, bool isBlackMove) {
		ulong[] fromList = isBlackMove ? blackPieces : whitePieces;
		ulong[] toList = isBlackMove ? whitePieces : blackPieces;

		ulong fromBit = 1UL << move.From;
		ulong toBit = 1UL << move.To;

		for (int i = 0; i < DataHandler.FenDict.Count; i++) {
			toList[i] &= ~toBit;
		}

		for (int i = 0; i < DataHandler.FenDict.Count; i++) {
			if ((fromList[i] & fromBit) != 0) {
				fromList[i] &= ~fromBit;
				fromList[i] |= toBit;
				break; // Es wurde die Figur gefunden, keine weitere Suche nötig
			}
		}
	}

	public List<DataHandler.Move> GenerateMoveSet(bool isBlackMove) {
		ulong[] searchList;
		ulong selfboard, enemyboard;
		List<DataHandler.Move> moveSet = new();
		GeneratePath pathGenerator = new();

		if (isBlackMove) {
			selfboard = GetBlackBitboard();
			enemyboard = GetWhiteBitboard();
			searchList = blackPieces;
		} else {
			selfboard = GetWhiteBitboard();
			enemyboard = GetBlackBitboard();
			searchList = whitePieces;
		}

		for (int pieceType = 0; pieceType < DataHandler.FenDict.Count; pieceType++) {
			for (int i = 0; i < 36; i++) {
				if ((searchList[pieceType] & (1UL << i)) != 0) {
					ulong currentMoves = 0;

					switch ((DataHandler.PieceNames)pieceType) {
						case DataHandler.PieceNames.BISHOP:
							currentMoves = pathGenerator.BishopPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.BISHOPEXCOMM:
							currentMoves = pathGenerator.BishopPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.COMPASSPAWN:
							currentMoves = pathGenerator.CompassPawnPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.FIELD_ICE:
							currentMoves = pathGenerator.FieldEffectPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.KING:
							currentMoves = pathGenerator.KingPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.KINGMIMIC:
							currentMoves = pathGenerator.KingMimicPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.KNIGHT:
							currentMoves = pathGenerator.KnightPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.LIFEAFTERKNIGHT:
							currentMoves = pathGenerator.KnightPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.LIFEAFTERROOK:
							currentMoves = pathGenerator.RookPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.PAWN:
							currentMoves = pathGenerator.PawnPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.PAWNBOW:
							currentMoves = pathGenerator.PawnBowPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.PAWNHUNTER:
							currentMoves = pathGenerator.PawnHunterPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.QUEEN:
							currentMoves = pathGenerator.QueenPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.QUEENCHARM:
							currentMoves = pathGenerator.QueenPath(i, selfboard, enemyboard, isBlackMove);
							break;
						case DataHandler.PieceNames.ROOK:
							currentMoves = pathGenerator.RookPath(i, selfboard, enemyboard, isBlackMove);
							break;
						default:
							break;
					}

					for (int j = 0; j < 36; j++) {
						if ((currentMoves & (1UL << j)) != 0) {
							DataHandler.Move newMove = new(i, j);
							moveSet.Add(newMove);
						}
					}
				}
			}
		}

		return moveSet;
	}
}
