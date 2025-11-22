using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;

public partial class Piece : Node2D
{
	public delegate void PieceSelectedDelegate(Piece piece);
	public event PieceSelectedDelegate PieceSelected;
	public TextureRect icon_path;
	public int slot_ID = -1;
	public int type;

	DataHandler.PieceNames_secondary[] rarity0 = {};
	DataHandler.PieceNames_secondary[] rarity1 = 
	{
		DataHandler.PieceNames_secondary.BLACK_COMPASSPAWN,
		DataHandler.PieceNames_secondary.WHITE_COMPASSPAWN,
		DataHandler.PieceNames_secondary.BLACK_PAWNBOW,
		DataHandler.PieceNames_secondary.WHITE_PAWNBOW
	};
	DataHandler.PieceNames_secondary[] rarity2 = {
		DataHandler.PieceNames_secondary.BLACK_LIFEAFTERKNIGHT,
		DataHandler.PieceNames_secondary.WHITE_LIFEAFTERKNIGHT,
		DataHandler.PieceNames_secondary.BLACK_LIFEAFTERROOK,
		DataHandler.PieceNames_secondary.WHITE_LIFEAFTERROOK,
		DataHandler.PieceNames_secondary.BLACK_PAWNHUNTER,
		DataHandler.PieceNames_secondary.WHITE_PAWNHUNTER
	};
	DataHandler.PieceNames_secondary[] rarity3 = {
		DataHandler.PieceNames_secondary.BLACK_BISHOPEXCOMM,
		DataHandler.PieceNames_secondary.WHITE_BISHOPEXCOMM
	};
	DataHandler.PieceNames_secondary[] rarity4 = {};
	DataHandler.PieceNames_secondary[] rarity5 = {
		DataHandler.PieceNames_secondary.BLACK_QUEENCHARM,
		DataHandler.PieceNames_secondary.WHITE_QUEENCHARM
	};
	DataHandler.PieceNames_secondary[] rarity6 = {
		DataHandler.PieceNames_secondary.BLACK_KINGMIMIC,
		DataHandler.PieceNames_secondary.WHITE_KINGMIMIC
	};

	private const String shaderPath = "Shader\\ColorOutline.gdshader";

	public override void _Ready()
	{
		icon_path = GetNode<TextureRect>("Icon");
	}

	public override void _Process(double delta)
	{
		// Replace with function body.
	}

	public void LoadIcon(int piece_name, Vector2 icon_size, string[] assets)
	{
		GD.Print("Piece name ist " + piece_name);
		icon_path.Texture = (Texture2D)ResourceLoader.Load<Texture>(assets[piece_name]);
		icon_path.Material = createShaderMaterial(assignRarity(piece_name));
		icon_path.SetSize(icon_size);
	}

	public int assignRarity(int piece_name) 
	{
		for(int i = 0; i < 7; i++) 
		{
			switch(i) 
			{
				case 0:
				if(checkContent(rarity0,piece_name)) return 0;
					break;
				case 1:
				if(checkContent(rarity1,piece_name)) return 1;
					break;
				case 2:
				if(checkContent(rarity2,piece_name)) return 2;
					break;
				case 3:
				if(checkContent(rarity3,piece_name)) return 3;
					break;
				case 4:
				if(checkContent(rarity4,piece_name)) return 4;
					break;
				case 5:
				if(checkContent(rarity5,piece_name)) return 5;
					break;
				case 6:
				if(checkContent(rarity6,piece_name)) return 6;
					break;
				default:
				return 0;
			}
		}
		return 0;
	}

	public bool checkContent(DataHandler.PieceNames_secondary[] pieces, int piece_name) 
	{
		foreach(DataHandler.PieceNames_secondary piece in pieces) 
		{
			if((int)piece == piece_name) 
			{
				return true;
			}
		}
		return false;
	}

	private Material createShaderMaterial(int rarity) 
	{
		ShaderMaterial material = new ShaderMaterial();
		Shader outlineShader = (Shader)GD.Load(shaderPath);
		if(outlineShader == null) 
		{
			GD.PrintErr("No Shader Loaded");
			return new Material();
		}
		material.Shader = outlineShader;
		material.SetShaderParameter("rarity",rarity);
		GD.Print("Added Shader Material");
		return material;
	}
	public void _on_icon_gui_input(InputEvent @event)
	{
		if (@event.IsActionPressed("mouse_left"))
		{
			PieceSelected?.Invoke(this);
		}
	}
}
