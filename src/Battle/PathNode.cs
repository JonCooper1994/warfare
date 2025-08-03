namespace Warfare.Battle;

using Godot;

public class PathNode {
  public Vector2I GridPosition { get; set; } // Coordinate on your grid
  public PathNode Parent { get; set; } // Parent node (used to reconstruct the path)

  public float Cost { get; set; } // Cost from the start node

  public PathNode(Vector2I gridPosition)
  {
    GridPosition = gridPosition;
  }
}
