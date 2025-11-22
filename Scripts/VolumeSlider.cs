using Godot;
using System;

public partial class VolumeSlider : HSlider
{
	private int bus_index;
	private HSlider volume_slider;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Music: ", DataHandler.volume);
		volume_slider = GetNode<HSlider>("VolumeSlider");
		volume_slider.Value = Mathf.DbToLinear(DataHandler.volume);
		bus_index = AudioServer.GetBusIndex("Master");

		this.ValueChanged += _on_value_changed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GD.Print(DataHandler.volume);

	}


	private void _on_value_changed(double value)
	{
		GD.Print(DataHandler.volume);
		DataHandler.volume_changed = true;
		AudioServer.SetBusVolumeDb(bus_index, Mathf.LinearToDb((float)value));
		DataHandler.volume = (float)value;
	}
}
