using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ShipInteriorPathfinder
{
  public class PathNode {
    public bool IsStart = false;

    public PathNode parent;

    public Vector2I gridPos;

    public double f, g, h;
  }

  private static ShipInterior shipInterior;

  public ShipInteriorPathfinder (ShipInterior interior) {
    shipInterior = interior;
  }

  public Vector2I NextTile(Vector2I startPosition, Vector2I destinationPosition) {
    return FindPath(startPosition, destinationPosition).First();
  }

  public List<Vector2I> FindPath(Vector2I startPosition, Vector2I destinationPosition) {
    Dictionary<Vector2I, bool> closedList = new();

    // Initialising the parameters of the starting node
    var startNode = new PathNode {
      f = 0.0,
      h = 0.0,
      g = 0.0,
      IsStart = true,
      gridPos = startPosition,
    };

    var openList = new PriorityQueue<PathNode, double>();

    openList.Enqueue(startNode, startNode.f);

    while (openList.Count > 0) {

      // Get the current cheapest node
      var currentNode = openList.Dequeue();

      if (currentNode.gridPos == destinationPosition) {
        return TracePath(currentNode);
      }

      closedList[currentNode.gridPos] = true;

      var neighbours = GetNeighbours(currentNode.gridPos);

      foreach (var neighbourPosition in neighbours) {

        if (!closedList.ContainsKey(neighbourPosition)) {
          var distance = currentNode.gridPos.DistanceTo(neighbourPosition);
          var gNew = currentNode.g + distance;
          var hNew = CalculateHValue(neighbourPosition.X, neighbourPosition.Y, destinationPosition);
          var fNew = gNew + hNew;

          var neighbourNode = new PathNode {
            f = fNew,
            g = gNew,
            h = hNew,
            gridPos = neighbourPosition,
            parent = currentNode
          };

          openList.Enqueue(neighbourNode, neighbourNode.f);
        }
      }

    }
    GD.Print("Pathfinding failed");
    return new();
  }


  private static List<Vector2I> GetNeighbours(Vector2I start) {
      var neighbours = new List<Vector2I> {
        new (start.X, start.Y - 1),
        new (start.X + 1, start.Y),
        new (start.X, start.Y + 1),
        new (start.X - 1, start.Y),
        new (start.X + 1, start.Y + 1),
        new (start.X + 1, start.Y - 1),
        new (start.X - 1, start.Y + 1),
        new (start.X - 1, start.Y - 1)
      };

      var availableNeighbours = new List<Vector2I>();

      foreach (var gridPos in neighbours) {
        if (shipInterior.TileExists(gridPos)) {
          availableNeighbours.Add(gridPos);
        }
      }

      return availableNeighbours;
    }

    public static List<Vector2I> TracePath(PathNode finalNode) {

      var failsafe = 100;
      var currentNode = finalNode;

      List<Vector2I> path = new();

      while (currentNode.IsStart == false && failsafe > 0) {
        failsafe--;

        path.Add(currentNode.gridPos);
        currentNode = currentNode.parent;
      }

      path.Reverse();
      return path;
    }

    public static double CalculateHValue(int x, int y, Vector2I dest)
    {
        return Math.Sqrt(Math.Pow(x - dest.X, 2) + Math.Pow(y - dest.Y, 2));
    }
}
