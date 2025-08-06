using Godot;
using System.Collections.Generic;

public enum TileType {
    SpawnZone,
    Island,
    Obstacle,
    PushNorth,
    PushEast,
    PushWest,
    PushSouth,
    BorderNorth,
    BorderEast,
    BorderWest,
    BorderSouth,
    Empty,
}

public static class TileTypeMethods {
    public static bool IsSolid(this TileType t) => t switch {
        TileType.Obstacle => true,
        TileType.Island => true,
        _ => false,
    };

    public static bool IsBorder(this TileType t) => t switch {
      TileType.BorderNorth => true,
      TileType.BorderEast => true,
      TileType.BorderSouth => true,
      TileType.BorderWest => true,
      _ => false,
    };

    // What path does this move cover?
    public static List<Vector2I> PushPath(this TileType m, Vector2I initialPos) => m switch {
        TileType.PushNorth => new List<Vector2I>{initialPos, initialPos + Direction.North.Vector()},
        TileType.PushEast => new List<Vector2I>{initialPos, initialPos + Direction.East.Vector()},
        TileType.PushSouth => new List<Vector2I>{initialPos, initialPos + Direction.South.Vector()},
        TileType.PushWest => new List<Vector2I>{initialPos, initialPos + Direction.West.Vector()},
        TileType.BorderNorth => new List<Vector2I>{initialPos, initialPos + Direction.South.Vector()},
        TileType.BorderEast => new List<Vector2I>{initialPos, initialPos + Direction.West.Vector()},
        TileType.BorderSouth => new List<Vector2I>{initialPos, initialPos + Direction.North.Vector()},
        TileType.BorderWest => new List<Vector2I>{initialPos, initialPos + Direction.East.Vector()},
        _ => new List<Vector2I>{initialPos},
    };
}
