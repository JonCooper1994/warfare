using System.Collections.Generic;
using Godot;

public enum Move
{
    Forward,
    Left,
    Right,
    Skip,
}

public static class MoveMethods{
    public static Move Next(this Move d) => d switch {
        Move.Forward => Move.Left,
        Move.Left => Move.Right,
        Move.Right => Move.Skip,
        Move.Skip => Move.Forward,
        _ => Move.Forward, // Stops compiler complaining
    };

    public static Move Previous(this Move d) => d switch {
        Move.Left => Move.Forward,
        Move.Right => Move.Left,
        Move.Skip => Move.Right,
        Move.Forward => Move.Skip,
        _ => Move.Forward, // Stops compiler complaining
    };

    // What path does this move cover?
    public static List<Vector2I> pathTaken(this Move m, Direction direction, Vector2I initialPos) => m switch {
        Move.Forward => new List<Vector2I>{initialPos, initialPos + direction.Vector()},
        Move.Left => new List<Vector2I>{initialPos, initialPos + direction.Vector(), initialPos + direction.Vector() + direction.Previous().Vector()},
        Move.Right => new List<Vector2I>{initialPos, initialPos + direction.Vector(), initialPos + direction.Vector() + direction.Next().Vector()},
        Move.Skip => new List<Vector2I>{initialPos},
        _ => new List<Vector2I>{initialPos},
    };

    // What does this move do to the ships position?
    public static Vector2I positionChange(this Move m, Direction direction, Vector2I initialPos) => m switch {
        Move.Forward => initialPos + direction.Vector(),
        Move.Left => initialPos + direction.Vector() + direction.Previous().Vector(),
        Move.Right => initialPos + direction.Vector() + direction.Next().Vector(),
        Move.Skip => initialPos,
        _ => initialPos,
    };

    // What does this move do to the ships direction?
    public static Direction directionChange(this Move m, Direction initialDirection) => m switch {
        Move.Forward => initialDirection,
        Move.Left => initialDirection.Previous(),
        Move.Right => initialDirection.Next(),
        Move.Skip => initialDirection,
        _ => initialDirection,
    };
}
