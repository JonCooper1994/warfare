using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

public partial class Battle : Node2D
{
  [Export] private PackedScene unitScene;

  [Export] private PackedScene markerScene;

  private Unit selectedUnit;

  public override void _Ready()
  {
    GetNode<Camera2D>("Camera2D").SetZoom(new Vector2(3.0f, 3.0f));
  }


  private void SpawnMarker(Vector2 pos) {
    var marker = markerScene.Instantiate<Node2D>();

    marker.Position = pos;
    CallDeferred("add_child", marker);
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {

    }
  }
}
