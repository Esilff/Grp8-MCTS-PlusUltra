using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRender : MonoBehaviour
{
    public Bomb bomb { get; set; }

    // Update is called once per frame
    void Update()
    {
        if (bomb.Cooldown < 0)
        {
            Destroy(gameObject);
        }
    }
}
