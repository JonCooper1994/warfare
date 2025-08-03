using Godot;
using System.Collections;
using System.Collections.Generic;

public enum LocalDirection
{
    Forward,
    Left,
    Right,
    Back
}

public static class LocalDirectionsMethods{

    public static LocalDirection Next(this LocalDirection d) => d switch {
        LocalDirection.Forward => LocalDirection.Left,
        LocalDirection.Left => LocalDirection.Right,
        LocalDirection.Right => LocalDirection.Back,
        LocalDirection.Back => LocalDirection.Forward,
        _ => LocalDirection.Forward,// Stops compiler complaining
    };

    public static LocalDirection Previous(this LocalDirection d) => d switch {
        LocalDirection.Forward => LocalDirection.Back,
        LocalDirection.Back => LocalDirection.Right,
        LocalDirection.Right => LocalDirection.Left,
        LocalDirection.Left => LocalDirection.Forward,
        _ => LocalDirection.Forward,// Stops compiler complaining
    };

    public static LocalDirection Opposite(this LocalDirection d) => d switch {
        LocalDirection.Forward => LocalDirection.Right,
        LocalDirection.Left => LocalDirection.Back,
        LocalDirection.Right => LocalDirection.Forward,
        LocalDirection.Back => LocalDirection.Left,
        _ => LocalDirection.Forward,// Stops compiler complaining
    };
}