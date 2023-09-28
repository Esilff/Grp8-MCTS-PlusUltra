using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb
{
    public float Cooldown { get; set; }
    public int Radius { get; set; }
    public float X { get; set; }
    public float Y { get; set; }

    public int Xi => Convert.ToInt32(X - 0.5f);
    public int Yi => Convert.ToInt32(Y - 0.5f);
}
