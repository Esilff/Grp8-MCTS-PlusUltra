using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombermanRenderer : MonoBehaviour
{
    public Bomberman Bomberman { get; set; }
    
    void Update()
    {
        if (Bomberman.IsDead) Destroy(gameObject);
        transform.position = Bomberman.Position + new Vector3(0,0,-2);// + new Vector3(0,0.5f,0);
    }
}
