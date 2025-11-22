using Godot;
using System;
using System.Collections.Generic;

public partial class Slot : ColorRect
{
	private ColorRect filter_path;
	public int slot_ID = -1;
	public delegate void SlotClickedDelegate(Slot slot);
	public event SlotClickedDelegate SlotClicked;
	public DataHandler.SlotStates state = DataHandler.SlotStates.NONE;


	public override void _Ready()
	{
		filter_path = GetNode<ColorRect>("Filter");
	}

	public override void _Process(double delta)
	{
		// Replace with function body.
	}

	public void SetBackground(Color c)
	{
		Color = c;
	}

	public void SetSize(Vector2 size)
	{
		CustomMinimumSize = size;
	}

	public void SetFilter(DataHandler.SlotStates color = DataHandler.SlotStates.NONE)
	{
		state = color;
		switch (color)
		{
			case DataHandler.SlotStates.NONE:
				filter_path.Color = new Color(0, 0, 0, 0); // Nichts
				break;
			case DataHandler.SlotStates.FREE:
				filter_path.Color = new Color(0, 1, 0, 0.6f); // Grün
				break;
			case DataHandler.SlotStates.SELF:
				filter_path.Color = new Color(1, 1, 0, 0.6f); // Gelb
				break;
			case DataHandler.SlotStates.LAST_FROM:
				filter_path.Color = new Color(0, 0, 1, 0.6f);; // Blau
				break;
			case DataHandler.SlotStates.LAST_TO:
				filter_path.Color = new Color(0, 0, 1, 0.6f);; // Blau
				break;
		}
	}


	public void _on_filter_gui_input(InputEvent @event)
	{
		if (@event.IsActionPressed("mouse_left"))
		{
			SlotClicked?.Invoke(this);
		}
	}
}


