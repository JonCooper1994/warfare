using Godot;
using System.Collections;

public enum Direction
{
    North,
    East,
    South,
    West
}

public static class DirectionsMethods{
    public static Direction Next(this Direction d) => d switch {
        Direction.North => Direction.East,
        Direction.East => Direction.South,
        Direction.South => Direction.West,
        Direction.West => Direction.North,
        _ => Direction.North,// Stops compiler complaining
    };

    public static Direction Previous(this Direction d) => d switch {
        Direction.North => Direction.West,
        Direction.West => Direction.South,
        Direction.South => Direction.East,
        Direction.East => Direction.North,
        _ => Direction.North,// Stops compiler complaining
    };

    public static Direction Opposite(this Direction d) => d switch {
        Direction.North => Direction.South,
        Direction.East => Direction.West,
        Direction.South => Direction.North,
        Direction.West => Direction.East,
        _ => Direction.North,// Stops compiler complaining
    };

    public static int RotationDegrees(this Direction d) => d switch {
        Direction.North => 0,
        Direction.East => 90,
        Direction.South => 180,
        Direction.West => 270,
        _ => 270, // Stops compiler complaining
    };

    public static Vector2I Vector(this Direction d) => d switch {
        Direction.North => new Vector2I(0, -1),
        Direction.East => new Vector2I(1, 0),
        Direction.South => new Vector2I(0, 1),
        Direction.West => new Vector2I(-1, 0),
        _ => new Vector2I(0, -1), // Stops compiler complaining
    };

    public static Direction AddLocalDirection(this Direction d, LocalDirection ld) => ld switch {
        LocalDirection.Forward => d,
        LocalDirection.Right => d.Next(),
        LocalDirection.Back => d.Opposite(),
        LocalDirection.Left => d.Previous(),
        _ => d, // Stops compiler complaining
    };
}
