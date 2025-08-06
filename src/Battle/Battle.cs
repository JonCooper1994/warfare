using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Warfare.Battle;

public partial class Battle : Node2D
{
  // TILES
  [Export] private PackedScene tileScene;
  [Export] private PackedScene borderTileScene;
  [Export] private PackedScene[] obstacleScenes;
  [Export] private Godot.Collections.Dictionary<TileType, PackedScene> windScenes;




  [Export] private PackedScene playerShipScene;

  private Vector2I hoveredGridPos;
  private Ship playerShip;
  private Hand hand;

  public List<Ship> ships = new();
  public int CurrentOrderIndex;
  public int MAX_ORDERS = 4;

  private float zoom = 2.0f;

  [Export]
  private Vector2I size;

  private Dictionary<Vector2I, TileType> tileTypes = new();
  private Dictionary<Vector2I, Tile> tiles = new();
  private Dictionary<Vector2I, Ship> allShips = new();

  bool IsPlayerTurn = true;
  private int PlayerEnergy = 5;

  private bool IsRunning = false;

  private State state = State.BetweenRounds;

  enum State {
    BetweenRounds,
    CalculateMoves,
    ExecuteMoves,
    CalculateEnvironmentMoves,
    ExecuteEnvironmentMoves,
    CalculateShooting,
    ExecuteShooting,
    NextOrder
  }

  public override void _Ready()
  {
    GD.Randomize();
    GetNode<Camera2D>("Camera2D").SetZoom(new Vector2(zoom, zoom));

    tileTypes = new MapGenerator(size).Generate(15, 3, 0);
    foreach (var keyValuePair in tileTypes)
    {
      var gridPos = keyValuePair.Key;
      var tileType = keyValuePair.Value;

      SpawnTile(gridPos, tileType);
    }

    SpawnShip(new Vector2I(5, 5));
  }

  public override void _Process(double delta) {
    // CALCULATE MOVES
    if (state == State.CalculateMoves) {
      CalculateMoves();
      state = State.ExecuteMoves;
      return;
    }

    // EXECUTE MOVES
    if (state == State.ExecuteMoves) {
      var done = ExecuteMoves((float)delta);
      if (done) {
        state = State.CalculateEnvironmentMoves;
      }

      return;
    }

    // CALCULATE ENVIRONMENT MOVES
    if (state == State.CalculateEnvironmentMoves) {
      CalculateEnvironmentMoves();
      state = State.ExecuteEnvironmentMoves;
      return;
    }

    // EXECUTE ENVIRONMENT MOVES
    if (state == State.ExecuteEnvironmentMoves) {
      var done = ExecuteMoves((float)delta);
      if (done) {
        state = State.NextOrder;
      }

      return;
    }

    // NEXT ORDER
    if (state == State.NextOrder) {
      CurrentOrderIndex++;
      if (CurrentOrderIndex < MAX_ORDERS) {
        state = State.CalculateMoves;
      }
      else {
        ResetAll();
        state = State.BetweenRounds;
      }
      return;
    }
  }

  public void ResetAll() {
    GD.Print("RESET");
    CurrentOrderIndex = 0;
    foreach (var ship in ships) {
      ship.ResetRound();
    }
  }

  public void CalculateMoves() {
    foreach(var ship in ships) {
      ship.ResetTurn();

      var order = ship.MoveOrders[CurrentOrderIndex];

      ship.CurrentRotationPath = new() {
        ship.CurrentRotation,
        order.directionChange(ship.Direction).RotationDegrees(),
      };

      ship.FinalGridPath = new(
        ship.Direction,
        order.pathTaken(ship.Direction, ship.GridPosition)
      );

      ship.Direction = order.directionChange(ship.Direction);
    }

    ProcessCollisions();
  }

  public void CalculateEnvironmentMoves() {
    foreach(var ship in ships) {
      ship.ResetTurn();

      var tile = tileTypes[ship.GridPosition];

      ship.FinalGridPath = new(ship.Direction, tile.PushPath(ship.GridPosition));
    }

    ProcessCollisions();
  }

  public bool ExecuteMoves(float delta) {
    var allShipsDone = true;

    foreach(var ship in ships) {
      var done = ship.AnimateMove(delta);
      if(!done) {
        allShipsDone = false;
      }
    }

    return allShipsDone;
  }

  public void ProcessCollisions() {

    foreach (var ship in ships) {
      ship.FullGridPath = new();

      // Set the full grid path before collisions
      foreach (var x in ship.FinalGridPath.path) {
        ship.FullGridPath.Add(x);
      }
    }

    // Step 0 is the ships original position so ignore it
    for (int step = 1; step <= 2; step++) {
      foreach (var ship in ships) {
        var gridPos = ship.FinalGridPath.PosAtStep(step);
        var previousPosition = ship.FinalGridPath.PosAtStep(step - 1);

        //Environment collisions
        if (ship.FinalGridPath.HasCollided()) { continue; }

        if (tileTypes[gridPos].IsSolid()) {
          ship.FinalGridPath.Collide(step);
        }

        //Other ship collisions
        foreach (var otherShip in ships) {
          if (ship == otherShip) { continue; } // Skip if its the same ship

          if (ship.FinalGridPath.HasCollided()) { continue; } // Ship has already collided so skip it

          var positionOther = otherShip.FinalGridPath.PosAtStep(step);
          var previousPositionOther = otherShip.FinalGridPath.PosAtStep(step - 1);

          if (gridPos == positionOther) {
            ship.FinalGridPath.Collide(step);
          }

          // Check if ships are swapping position - this is also a collision
          if (gridPos == previousPositionOther && positionOther == previousPosition) {
            ship.FinalGridPath.Collide(step);
          }
        }
      }
    }

    foreach(var ship in ships){
      if (ship.FullGridPath.Count > 0) {
        ship.GridPosition = ship.FinalGridPath.path.Last();
      }

      // Figure out how far through the movement a collision ocurred
      if(ship.FinalGridPath.hasCollided) {
        ship.CollisionPoint = (float)ship.FinalGridPath.path.Count / (float)ship.FullGridPath.Count;
      } else {
        ship.CollisionPoint = 99.0f; // NO COLLISION
      }
    }
  }

  private void SpawnTile(Vector2I gridPos, TileType tileType) {

    var tileSceneToSpawn = tileType.IsBorder() ? borderTileScene : tileScene;

    var newTile = tileSceneToSpawn.Instantiate<Tile>();
    newTile.Position = GridUtils.GridToWorldPos(gridPos);
    tiles.Add(gridPos, newTile);
    CallDeferred("add_child", newTile);

    if (tileType == TileType.Obstacle) {
      var randomIndex = (int)(GD.Randi() % obstacleScenes.Length);
      var newObstacle = obstacleScenes[randomIndex].Instantiate<Node2D>();
      newObstacle.Position = GridUtils.GridToWorldPos(gridPos);
      CallDeferred("add_child", newObstacle);
    }

    if (tileType == TileType.PushNorth || tileType == TileType.PushWest || tileType == TileType.PushSouth || tileType == TileType.PushEast) {
      var newWind = windScenes[tileType].Instantiate<Node2D>();
      newWind.Position = GridUtils.GridToWorldPos(gridPos);
      CallDeferred("add_child", newWind);
    }
  }

  private void SpawnShip(Vector2I gridPos) {
    Ship newShip;
    newShip = playerShipScene.Instantiate<Ship>();

    playerShip = newShip;
    ships.Add(newShip);

    newShip.Position = GridUtils.GridToWorldPos(gridPos);
    newShip.GridPosition = gridPos;
    allShips.Add(gridPos, newShip);

    CallDeferred("add_child", newShip);
  }

  public override void _Input(InputEvent @event)
  {
    var gridPos = GridUtils.WorldToGridPos(GetGlobalMousePosition());

    hoveredGridPos = gridPos;

    if(Input.IsKeyPressed(Key.Left)) {
      playerShip.MoveOrders.Add(Move.Left);
    }

    if(Input.IsKeyPressed(Key.Right))
    {
      playerShip.MoveOrders.Add(Move.Right);
    }

    if (Input.IsKeyPressed(Key.Up)) {
      playerShip.MoveOrders.Add(Move.Forward);
    }

    if (playerShip.MoveOrders.Count == 4 && state == State.BetweenRounds) {
      state = State.CalculateMoves;
    }
  }

  private bool IsTileInBounds(Vector2I gridPos)
  {
    return gridPos.X >= 0 && gridPos.X < size.X&& gridPos.Y >= 0 && gridPos.Y < size.Y;
  }

}
