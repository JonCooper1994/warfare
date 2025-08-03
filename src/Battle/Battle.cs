using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Warfare.Battle;

public partial class Battle : Node2D
{
  [Export] private PackedScene tileScene;
  [Export] private PackedScene[] blockerScenes;
  [Export] private PackedScene playerShipScene;

  [Export] private int size;

  private Vector2I hoveredGridPos;
  private Ship playerShip;
  private Hand hand;

  private float zoom = 3.0f;


  private Godot.Collections.Dictionary<Vector2I, Tile> allTiles = new();
  private Godot.Collections.Dictionary<Vector2I, Ship> allShips = new();

  private Godot.Collections.Dictionary<Vector2I, bool> blockedTiles = new();

  bool IsPlayerTurn = true;
  private int PlayerEnergy = 5;

  private LineDrawer lineDrawer = new();

  public override void _Ready()
  {
    GD.Randomize();

    AddChild(lineDrawer);

    GetNode<Camera2D>("Camera2D").SetZoom(new Vector2(zoom, zoom));

    for (int i = 0; i < size; i++) {
      for (int j = 0; j < size; j++) {
        SpawnTile(new Vector2I(i, j));

        if (GD.Randf() < 0.2) {
          blockedTiles.Add(new Vector2I(i, j), true);
          SpawnBlocker(new Vector2I(i, j));
        }
        else {
          blockedTiles.Add(new Vector2I(i, j), false);
        }
      }
    }

    SpawnShip(new Vector2I(5, 5));
  }

  public override void _Process(double delta) {
    foreach (var keyValuePair in allTiles)
    {
      var tile = keyValuePair.Value;
      tile.SetState(TileUIState.Normal);
    }

    foreach (var keyValuePair in blockedTiles) {
      var gridPos = keyValuePair.Key;
      var isBlocked = keyValuePair.Value;

      var tile = allTiles[gridPos];
      if (isBlocked) {
        tile.SetState(TileUIState.Blocked);
      }
    }

    foreach (var keyValuePair in allShips)
    {
        var pawn = keyValuePair.Value;
        var tile = allTiles[pawn.GridPosition];
        if (pawn.Faction == Faction.Player) {
          tile.SetState(TileUIState.PlayerPawn);
        }
        else {
          tile.SetState(TileUIState.EnemyPawn);
        }
    }

    if (allTiles.ContainsKey(hoveredGridPos)) {
      var hoveredTile = allTiles[hoveredGridPos];
    }
  }

  private void SpawnTile(Vector2I gridPos) {
    var newTile = tileScene.Instantiate<Tile>();

    newTile.Position = GridUtils.GridToWorldPos(gridPos);

    allTiles.Add(gridPos, newTile);

    CallDeferred("add_child", newTile);
  }

  private void SpawnBlocker(Vector2I gridPos) {

    var randomIndex = (int)(GD.Randi() % blockerScenes.Length);

    var newTree = blockerScenes[randomIndex].Instantiate<Node2D>();

    newTree.Position = GridUtils.GridToWorldPos(gridPos);

    CallDeferred("add_child", newTree);
  }

  private void SpawnShip(Vector2I gridPos) {
    Ship newShip;
    newShip = playerShipScene.Instantiate<Ship>();

    playerShip = newShip;

    newShip.Position = GridUtils.GridToWorldPos(gridPos);
    newShip.GridPosition = gridPos;
    allShips.Add(gridPos, newShip);

    CallDeferred("add_child", newShip);
  }

  public override void _Input(InputEvent @event)
  {
    var gridPos = GridUtils.WorldToGridPos(GetGlobalMousePosition());

    hoveredGridPos = gridPos;

    if(Input.IsKeyPressed(Key.Left))
    {
      playerShip.ExecuteMove(Move.Left);
    }

    if(Input.IsKeyPressed(Key.Right))
    {
      playerShip.ExecuteMove(Move.Right);
    }

    if (Input.IsKeyPressed(Key.Up)) {
      playerShip.ExecuteMove(Move.Forward);
    }
  }


  public Dictionary<Vector2I, PathNode> GetAllPaths(Vector2I start, int maxDistance)
  {
    var openList = new Queue<PathNode>();
    var paths = new Dictionary<Vector2I, PathNode>();
    var visited = new HashSet<Vector2I>();

    // Add starting node
    var startNode = new PathNode(start)
    {
      Cost = 0 // Distance from the start to itself is 0
    };
    openList.Enqueue(startNode);
    visited.Add(start);

    // BFS
    while (openList.Count > 0)
    {
      var currentNode = openList.Dequeue();

      // Stop traversing neighbors if max distance is reached
      if (currentNode.Cost >= maxDistance)
      {
        continue;
      }

      foreach (var neighbor in GetNeighbors(currentNode.GridPosition))
      {
        // Skip if already visited
        if (visited.Contains(neighbor))
        {
          continue;
        }

        // Mark node as visited
        visited.Add(neighbor);

        // Create neighbor node
        var neighborNode = new PathNode(neighbor)
        {
          Parent = currentNode,
          Cost = currentNode.Cost + 1
        };

        // Add to open list and paths
        openList.Enqueue(neighborNode);
        paths[neighbor] = neighborNode;
      }
    }

    return paths; // Return reachable nodes with their paths
  }
  private List<Vector2I> GetNeighbors(Vector2I gridPos) {
    var neighbors = new List<Vector2I>();

    var up = new Vector2I(gridPos.X, gridPos.Y - 1);
    var down = new Vector2I(gridPos.X, gridPos.Y + 1);
    var left = new Vector2I(gridPos.X - 1, gridPos.Y);
    var right = new Vector2I(gridPos.X + 1, gridPos.Y );

    if (IsTileInBounds(up) && IsTileWalkable(up)) {
      neighbors.Add(up);
    }

    if (IsTileInBounds(down) && IsTileWalkable(down)) {
      neighbors.Add(down);
    }

    if (IsTileInBounds(left) && IsTileWalkable(left)) {
      neighbors.Add(left);
    }

    if (IsTileInBounds(right) && IsTileWalkable(right)) {
      neighbors.Add(right);
    }

    return neighbors;
  }

  private bool IsTileInBounds(Vector2I gridPos)
  {
    return gridPos.X >= 0 && gridPos.X < size && gridPos.Y >= 0 && gridPos.Y < size;
  }

  private bool IsTileWalkable(Vector2I gridPos)
  {
    return !blockedTiles[gridPos] && !allShips.ContainsKey(gridPos);
  }

  private List<Vector2I> ReconstructPath(PathNode node)
  {
    var path = new List<Vector2I>();

    while (node != null)
    {
      path.Add(node.GridPosition);
      node = node.Parent;
    }

    path.Reverse(); // Start -> Target
    return path;
  }

  private static float HeuristicCost(Vector2I start, Vector2I target)
  {
    return Mathf.Abs(start.X - target.X) + Mathf.Abs(start.Y - target.Y);
  }

  private bool HasLineOfSight(Vector2I start, Vector2I target)
  {
    // Get all the tiles along the line from `start` to `target`
    var lineTiles = GetLineTiles(start, target);

    // Check if all tiles (except start and target) are walkable
    foreach (var tile in lineTiles)
    {
      if (tile == start || tile == target)
      {
        continue; // Skip start and target tiles
      }

      // Check if the tile is blocked
      if (blockedTiles[tile])
      {
        return false; // Line of sight is blocked
      }
    }

    return true; // No obstacles found, line of sight is clear
  }

  private List<Vector2I> GetLineTiles(Vector2I start, Vector2I target)
  {
    // Bresenham's Line Algorithm for integer grid positions
    var line = new List<Vector2I>();

    int dx = Mathf.Abs(target.X - start.X);
    int dy = Mathf.Abs(target.Y - start.Y);

    int sx = start.X < target.X ? 1 : -1;
    int sy = start.Y < target.Y ? 1 : -1;

    int err = dx - dy;

    int x = start.X;
    int y = start.Y;

    while (true)
    {
      line.Add(new Vector2I(x, y));

      if (x == target.X && y == target.Y)
      {
        break;
      }

      int e2 = 2 * err;

      if (e2 > -dy)
      {
        err -= dy;
        x += sx;
      }

      if (e2 < dx)
      {
        err += dx;
        y += sy;
      }
    }

    return line;
  }
}
