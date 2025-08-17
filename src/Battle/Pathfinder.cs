using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Pathfinder
{

    static Position defaultPos = new (-1, -1, Direction.North);

    // A structure to hold the necessary parameters
    public struct Cell
    {
        // Row and Column index of its parent
        // Note that 0 <= i <= ROW-1 & 0 <= j <= COL-1
        public Position parent;
        // f = g + h
        public double f, g, h;

        // The move that got us to this cell
        public Move move;
    }

    public struct Position {
        public Vector2I pos;
        public Direction dir;

        public Position(int x, int y, Direction dir) {
            pos = new(x,y);
            this.dir = dir;
        }
    }

    public static Direction intToDir(int i) {
        var dir = Direction.North;

        switch(i) {
            case 0:
                dir = Direction.North;
            break;
            case 1:
                dir = Direction.East;
            break;
            case 2:
                dir = Direction.South;
            break;
            case 3:
                dir = Direction.West;
            break;
        }

        return dir;
    }

    public static Move intToMove(int i) {
        var move = Move.Forward;

        switch(i) {
            case 0:
                move = Move.Forward;
            break;
            case 1:
                move = Move.Left;
            break;
            case 2:
                move = Move.Right;
            break;
        }

        return move;
    }

    // A Function to find the shortest path between
    // a given source cell to a destination cell according
    // to A* Search Algorithm
    public static List<Move> AStar(Battle battle, Vector2I src, Direction srcDirection, Vector2I dest)
    {
        GD.Print("Pathfinding: start");

        var sourcePosition = new Position(src.X, src.Y, srcDirection);

        // // If the source or destination is out of range
        // if (!IsValid(src.first, src.second, ROW, COL) || !IsValid(dest.first, dest.second, ROW, COL))
        // {
        //     Console.WriteLine("Source or destination is invalid");
        //     return;
        // }

        // // Either the source or the destination is blocked
        // if (!IsUnBlocked(grid, src.first, src.second) || !IsUnBlocked(grid, dest.first, dest.second))
        // {
        //     Console.WriteLine("Source or the destination is blocked");
        //     return;
        // }

        // // If the destination cell is the same as the source cell
        // if (src.first == dest.first && src.second == dest.second)
        // {
        //     Console.WriteLine("We are already at the destination");
        //     return;
        // }

        // Create a closed list and initialise it to false which
        // means that no cell has been included yet. This closed
        // list is implemented as a boolean 2D array
        Dictionary<Position, bool> closedList = new();

        // Declare a 2D array of structure to hold the details
        // of that cell
        Dictionary<Position, Cell> allCells = new();

        for (int x = 0; x < battle.Size.X; x++)
        {
            for (int y = 0; y < battle.Size.Y; y++)
            {
                for (int d = 0; d < 4; d++)
                {
                    Cell c = new() {
                        f = double.MaxValue,
                        g = double.MaxValue,
                        h = double.MaxValue,
                        parent = defaultPos,
                    };

                    allCells[new Position(x, y, intToDir(d))] = c;

                    closedList[new Position(x, y, intToDir(d))] = false;
                }
            }
        }

        // Initialising the parameters of the starting node
        Position currentNode = sourcePosition;
        var cell = allCells[currentNode];
        cell.f = 0.0;
        cell.h = 0.0;
        cell.g = 0.0;
        cell.parent = defaultPos;

        allCells[currentNode] = cell;

        /*
            Create an open list having information as-
            <f, <i, j>>
            where f = g + h,
            and i, j are the row and column index of that cell
            Note that 0 <= i <= ROW-1 & 0 <= j <= COL-1
            This open list is implemented as a SortedSet of tuple (f, (i, j)).
            We use a custom comparer to compare tuples based on their f values.
        */
        SortedSet<(double, Position)> openList = new SortedSet<(double, Position)>(
            Comparer<(double, Position)>.Create((a, b) => a.Item1.CompareTo(b.Item1))
        );

        // Put the starting cell on the open list and set its
        // 'f' as 0
        openList.Add((0.0, currentNode));

        // We set this boolean value as false as initially
        // the destination is not reached.
        bool foundDest = false;

        while (openList.Count > 0)
        {
            (double f, Position pos) node = openList.Min;
            openList.Remove(node);

            // Add this vertex to the closed list
            closedList[node.pos] = true;

            // Generate possible next steps for ship
            for (int i = 0; i < 3; i++) {
                Move move = intToMove(i);

                var newPos = battle.PredictedPosition(move, node.pos.dir, node.pos.pos);
                var newDir = battle.PredictedDirection(move, node.pos.dir, node.pos.pos);

                Position newPosition = new(newPos.X, newPos.Y, newDir);

                if(IsInBounds(newPos.X, newPos.Y, battle.Size.X, battle.Size.Y)) {
                    if(IsDestination(newPos.X, newPos.Y, dest)) {
                        var c = allCells[newPosition];
                        c.parent = node.pos;
                        c.move = move;
                        allCells[newPosition] = c;

                        foundDest = true;

                        return TracePath(battle, allCells, sourcePosition, newPosition);
                    }

                    // TODO: check if path crosses any blocked tiles
                    if(!closedList[newPosition]) {
                        double gNew = allCells[node.pos].g + 1.0;
                        double hNew = CalculateHValue(newPos.X, newPos.Y, dest);
                        double fNew = gNew + hNew;

                        // If it isn’t on the open list, add it to
                        // the open list. Make the current square
                        // the parent of this square. Record the
                        // f, g, and h costs of the square cell
                        if(allCells[newPosition].f == double.MaxValue || allCells[newPosition].f > fNew) {
                            openList.Add((fNew, newPosition));

                            // Update the details of this cell
                            var c = allCells[newPosition];
                            c.f = fNew;
                            c.g = gNew;
                            c.h = hNew;
                            c.move = move;
                            c.parent = node.pos;

                            allCells[newPosition] = c;
                        }
                    }
                }
            }

        }

        GD.Print("Pathfinding: Ain't found shit");

        // When the destination cell is not found and the open
        // list is empty, then we conclude that we failed to
        // reach the destination cell. This may happen when the
        // there is no way to destination cell (due to
        // blockages)
        if (!foundDest)
            GD.Print("Failed to find the Destination Cell");

        return new();
    }

    public static List<Move> TracePath(Battle battle, Dictionary<Position, Cell> cells, Position startPos, Position endPos) {

        var moves = new List<Move>();

        var pos = endPos;

        var done = false;

        while(!done) {
            //Debugger.CreateNodeOnGrid(battle, map, pos.pos);
            pos = cells[pos].parent;

            if(pos.Equals(startPos)) {
                done = true;
            } else {
                moves.Add(cells[pos].move);
            }
        }

        moves.Reverse();
        return moves;
    }

    public static bool IsInBounds(int x, int y, int width, int height)
    {
        return (x >= 0) && (x < width) && (y >= 0) && (y < height);
    }

    public static bool IsDestination(int x, int y, Vector2I dest)
    {
        return x == dest.X && y == dest.Y;
    }

    public static double CalculateHValue(int x, int y, Vector2I dest)
    {
        return Math.Sqrt(Math.Pow(x - dest.X, 2) + Math.Pow(y - dest.Y, 2));
    }
}
