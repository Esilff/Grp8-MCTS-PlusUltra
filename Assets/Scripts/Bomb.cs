using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb
{
    public int Cooldown { get; private set; }
    public int Radius { get; private set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool ShouldExplode { get; private set; }
    

    public Bomb()
    {
        ShouldExplode = false;
        Radius = 1;
        

    }
}
