using Godot;
using System;
using System.Collections.Generic;

public partial class ShipInterior : Node2D {

  [Export] private PackedScene CrewMemberScene;

  private TileMapLayer tileMapLayer;

  private List<CrewMember> CrewMembers = new();

  public ShipInteriorPathfinder Pathfinder;

  private CrewMember SelectedCrewmember;

  public override void _Ready() {
    tileMapLayer = GetNode<TileMapLayer>("TileMapLayer");
    ShipInteriorDebugger.Init();

    for (int i = 0; i < 5; i++) {
      SpawnRandomCrewMember();
    }

    Pathfinder = new ShipInteriorPathfinder(this);
  }

  public override void _Input(InputEvent @event)
  {
    var clickedGridPos = WorldToGrid(GetGlobalMousePosition());

    if(Input.IsMouseButtonPressed(MouseButton.Left)) {
      GD.Print("Clicked Pos:" + clickedGridPos);
    }

    if (Input.IsMouseButtonPressed(MouseButton.Right)) {
      SelectedCrewmember.GoToTile(clickedGridPos);
      ShipInteriorDebugger.Clear();
      ShipInteriorDebugger.CreateNodeOnGrid(this, clickedGridPos);
      var path = Pathfinder.FindPath(SelectedCrewmember.gridPos, clickedGridPos);

      foreach (var pathPos in path)
      {
        ShipInteriorDebugger.CreateNodeOnGrid(this, pathPos);
      }
    }


  }

  private void SpawnRandomCrewMember() {
    SpawnCrewMember(tileMapLayer.GetUsedCells().PickRandom());
  }

  private void SpawnCrewMember(Vector2I gridPos) {
    var newCrewMember = CrewMemberScene.Instantiate<CrewMember>();

    newCrewMember.Position = tileMapLayer.MapToLocal(gridPos);

    CrewMembers.Add(newCrewMember);
    SelectedCrewmember = newCrewMember;
    newCrewMember.gridPos = gridPos;
    newCrewMember.shipInterior = this;

    //newCrewMember.Connect(SignalName, new Callable(this, nameof(OnCrewMemberClicked)));

    CallDeferred("add_child", newCrewMember);
  }

  public void OnCrewMemberClicked(CrewMember crewMember) {
    SelectedCrewmember = crewMember;
  }

  public bool TileExists(Vector2I gridPos) {
    var tileData = tileMapLayer.GetCellTileData(gridPos);

    return tileData != null;
  }

  public Vector2 GridToWorld(Vector2I gridPos) {
    return tileMapLayer.MapToLocal(gridPos);
  }
  public Vector2I WorldToGrid(Vector2 worldPos) {
    return tileMapLayer.LocalToMap(worldPos);
  }


}
