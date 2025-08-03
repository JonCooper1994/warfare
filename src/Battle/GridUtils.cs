namespace Warfare.Battle;

using Godot;


public enum Direction {
  Up,
  Down,
  Left,
  Right
}



public class GridUtils {
  public static int tileWidth = 64;
  public static int tileHeight = 32;

  public static int halfHeight = tileHeight / 2;


  public static Vector2 GridToWorldPos(Vector2 gridPos) {
    var isoX = (gridPos.X - gridPos.Y) * (tileWidth / 2.0f);
    var isoY = (gridPos.X + gridPos.Y) * (tileHeight / 2.0f);

    return new Vector2(isoX, isoY);
  }

  public static Vector2I WorldToGridPos(Vector2 worldPos) {

    // Perform inverse isometric transformation
    var gridX = (worldPos.X / (tileWidth / 2.0f) + (worldPos.Y + halfHeight) / (tileHeight / 2.0f)) / 2;
    var gridY = ((worldPos.Y + halfHeight) / (tileHeight / 2.0f) - worldPos.X / (tileWidth / 2.0f)) / 2;


    // Convert to integers (grid coordinates must be whole numbers)
    var gridPosition = new Vector2I(Mathf.FloorToInt(gridX), Mathf.FloorToInt(gridY));

    return gridPosition;
  }

}
