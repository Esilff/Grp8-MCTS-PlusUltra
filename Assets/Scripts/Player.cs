using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerBehavior
{
    Player, Random, Montecarlo
}
public class Player
{
    public PlayerBehavior Behavior { get; set; }
    public Vector2 Position { get; set; }
}
