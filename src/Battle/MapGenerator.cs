using Godot;
using System.Collections.Generic;

public class MapGenerator(Vector2I size) {

	Dictionary<TileType, string> printableTiles = new() {
		{TileType.Obstacle, "X"},
		{TileType.PushNorth, "N"},
		{TileType.PushEast, "E"},
		{TileType.PushSouth, "S"},
		{TileType.PushWest, "W"},
		{TileType.BorderNorth, "B"},
		{TileType.BorderEast, "B"},
		{TileType.BorderSouth, "B"},
		{TileType.BorderWest, "B"},
		{TileType.Island, "I"},
		{TileType.SpawnZone, "#"},
		{TileType.Empty, "O"},
	};

	Dictionary<int, Direction> windDirections = new() {
		{0, Direction.North},
		{1, Direction.East},
		{2, Direction.South},
		{3, Direction.West}
	};

	Dictionary<Direction, TileType> directionToTile = new() {
		{Direction.North, TileType.PushNorth},
		{Direction.East, TileType.PushEast},
		{Direction.South, TileType.PushSouth},
		{Direction.West, TileType.PushWest}
	};

  Dictionary<Vector2I, TileType> tiles = new();

  public Dictionary<Vector2I, TileType> Generate(int obstacleGroups, int windColumns, int islands) {
 		GD.Randomize();

    tiles.Clear();

    size = new(15, 15);
    GD.Print(size);

		for (int x = 0; x < size.X; x++){
			for (int y = 0; y < size.Y; y++){
        GD.Print("test:" + new Vector2I(x, y));
				tiles.Add(new(x, y), TileType.Empty);
			}
		}

		for (int i = 0; i < obstacleGroups; i++){
			GenerateObstacleGroup(RandomPosition(false), 0.5f, 2);
		}

		for (int i = 0; i < windColumns; i++){
			GenerateWindColumn(RandomPosition(true), 1, 7);
		}

    AddBorder();

		return tiles;
	}

	public void AddBorder() {
		//North
		for (int x = -3; x < size.X + 3; x++) {
			for (int y = -3; y < 0; y++) {
        tiles[new(x, y)] = TileType.BorderNorth;
			}
		}

		//East
		for (int x = size.X; x < size.X + 3; x++) {
			for (int y = -3; y < size.Y + 3; y++) {
        tiles[new(x, y)] = TileType.BorderEast;

			}
		}

		//West
		for (int x = -3; x < 0; x++) {
			for (int y = -3; y < size.Y + 3; y++) {
        tiles[new(x, y)] = TileType.BorderWest;
			}
		}

		//South
		for (int x = -3; x < size.X + 3; x++) {
			for (int y = size.Y; y < size.Y + 3; y++) {
        tiles[new(x, y)] = TileType.BorderSouth;
      }
		}
	}

	public void GenerateObstacleGroup(Vector2I pos, float spreadChance, int spreadCount) {
		tiles[pos] = TileType.Obstacle;

		var adjacentCells = GetAdjacentCells(pos);

		if (spreadCount <= 0) { return; };

		foreach(var cell in adjacentCells) {
      TileType tileType;
      if(tiles.TryGetValue(cell, out tileType)) {
        if(tileType == TileType.Empty) {
          if(GD.Randf() <= spreadChance) {
            GenerateObstacleGroup(cell, spreadChance, spreadCount - 1);
          }
        }
      }
		}
	}

	public void GenerateWindColumn(Vector2I pos, int minDistance, int maxDistance) {
		var direction = windDirections[GD.RandRange(0,3)];

		var length = GD.RandRange(minDistance, maxDistance);

		for (int i = 0; i < length; i++) {
      tiles[pos + (i * direction.Vector())] = directionToTile[direction];
		}
	}

	public List<Vector2I> GetAdjacentCells(Vector2I pos) {
		List<Vector2I> list = new();

		if(pos.X - 1 >= 0) {
			list.Add(new(){X = pos.X - 1, Y = pos.Y});
		}

		if(pos.Y - 1 >= 0) {
			list.Add(new(){X = pos.X, Y = pos.Y - 1});
		}

		if(pos.X + 1 < size.X) {
			list.Add(new(){X = pos.X + 1, Y = pos.Y});
		}

		if(pos.Y + 1 < size.Y) {
			list.Add(new(){X = pos.X, Y = pos.Y + 1});
		}

		return list;
	}

	public Vector2I RandomPosition(bool includeSpawnZone){
		var startX = 0;
		var endX = size.X - 1;

		var startY = 0;
		var endY = size.Y - 1;

		if(!includeSpawnZone) {
			startX = 2;
			endX = size.X - 3;
		}

		return new() {
			X = GD.RandRange(startX, endX),
			Y = GD.RandRange(startY, endY),
		};
	}

}
