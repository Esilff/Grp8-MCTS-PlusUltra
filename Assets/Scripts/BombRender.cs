using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRender : MonoBehaviour
{
    public Bomb bomb { get; set; }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(bomb.X, bomb.Y, -2);
        if (bomb.Cooldown < 0)
        {
            Destroy(gameObject);
        }
    }
}
