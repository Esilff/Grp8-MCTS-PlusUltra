using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BombermanType
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
    public BombermanType Type { get; set; }

    public bool IsDead { get; set; } = false;

    public float BombCooldown { get; set; } = 2;
    public bool CanBomb { get; set; } = true;

    //Util properties
    public Vector3 Position => new Vector3(X, Y,0);
    public int Xi => Convert.ToInt32(X);
    public int Yi => Convert.ToInt32(Y);
    
}
