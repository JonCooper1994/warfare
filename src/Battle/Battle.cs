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

  private TileMapLayer tileMap;

  private AStar2D aStar = new AStar2D();

  public override void _Ready()
  {
    SpawnUnit(new Vector2(64+32, 64+32));
    GetNode<Camera2D>("Camera2D").SetZoom(new Vector2(4.0f, 4.0f));
    tileMap = GetNode<TileMapLayer>("TileMapLayer");

    foreach (var usedCell in tileMap.GetUsedCells())
    {
      aStar.AddPoint(GetTileId(usedCell), usedCell, 1.0f);
    }

    foreach (var usedCell in tileMap.GetUsedCells())
    {
      var neighbours = GetOpenNeighbours(usedCell);
      foreach (var neighbour in neighbours)
      {
        aStar.ConnectPoints(GetTileId(usedCell), GetTileId(neighbour));
      }
    }
  }

  public Vector2[] GetPathToPoint(Vector2I Start, Vector2I End) {
    return aStar.GetPointPath(GetTileId(Start), GetTileId(End));
  }

  private List<Vector2I> GetOpenNeighbours(Vector2I tilePosition) {
    var neighbourOffsets = new Array<Vector2I> {
      new (0, 1),
      new (1, 0),
      new (0, -1),
      new (-1, 0),
      new (1, 1),
      new (-1, -1),
      new (-1, 1),
      new (1, -1)
    };

    return neighbourOffsets
      .Select(neighbourOffset => tilePosition + neighbourOffset)
      .Where(neighbourTilePosition => tileMap.GetUsedCells().Contains(neighbourTilePosition))
      .ToList();
  }

  private int GetTileId(Vector2I tilePosition) {
    int rectWidth = (tileMap.GetUsedRect().Size - tileMap.GetUsedRect().Position).X;
    return (tilePosition.X * rectWidth) + tilePosition.Y;
  }

  private void SpawnUnit(Vector2 unitPosition) {
    var newUnit = unitScene.Instantiate<Unit>();

    if (newUnit == null)
    {
      GD.PrintErr("Unit null!");
      return;
    }

    newUnit.Position = unitPosition;
    CallDeferred("add_child", newUnit);

    selectedUnit = newUnit;
  }

  private void SpawnMarker(Vector2 pos) {
    var marker = markerScene.Instantiate<Node2D>();

    marker.Position = pos;
    CallDeferred("add_child", marker);
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {

      Vector2 localMousePosition = tileMap.ToLocal(GetGlobalMousePosition());
      Vector2I tileCoordinates = tileMap.LocalToMap(localMousePosition);

      tileMap.MapToLocal(tileCoordinates);

      GD.Print($"Tile coordinates: {tileCoordinates * 64}");

      //SpawnMarker( (tileCoordinates * 64) + new Vector2(32, 32));

      foreach (var point in GetPathToPoint(new Vector2I(0, 0), tileCoordinates))
      {
        GD.Print($"Point: {point * 64}");

        SpawnMarker( (point * 64) + new Vector2(32, 32));
      }
    }
  }
}
