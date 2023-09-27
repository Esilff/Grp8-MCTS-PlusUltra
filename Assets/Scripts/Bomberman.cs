using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BombermanState
{
    Player, Random, Mont
}

public enum BombermanAction
{
    None, Bomb, Up, Down, Left, Right
}

public class Bomberman
{
    public float X { get; set; }
    public float Y { get; set; }
    public BombermanAction BombermanAction { get; set; }
    public BombermanState State { get; set; }

    public float BombCooldown { get; set; } = 2;
    public bool CanBomb { get; set; } = true;

    public Vector3 Position => new Vector3(X, Y,0);
}
