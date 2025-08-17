using Godot;
using System;
using System.Collections.Generic;

public partial class CrewMember : Node2D {

  [Signal]
  public delegate void CrewMemberClickedEventHandler(CrewMember crewMember);

  public Vector2I gridPos;
  public Vector2I targetGridPos;
  public Vector2I nextGridPos;

  public ShipInterior shipInterior;

  public float speed = 50.0f;


  public void GoToTile(Vector2I newTargetGridPos) {
    targetGridPos = newTargetGridPos;
    nextGridPos = shipInterior.Pathfinder.NextTile(gridPos, targetGridPos);
  }

  public override void _Ready() {
    targetGridPos = gridPos;
  }

  public override void _Process(double delta) {
    if (gridPos != targetGridPos) {
      var targetWorldPos = shipInterior.GridToWorld(nextGridPos);

      var direction = (targetWorldPos - Position).Normalized();
      var distance = speed * (float)delta;

      if (Position.DistanceTo(targetWorldPos) < distance) {
        Position = targetWorldPos;
        gridPos = nextGridPos;
        nextGridPos = shipInterior.Pathfinder.NextTile(gridPos, targetGridPos);
      }
      else {
        Position += direction * distance;
      }
    }
  }
}
