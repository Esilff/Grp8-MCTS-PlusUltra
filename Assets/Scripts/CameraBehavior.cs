using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Transform origin;
    // Start is called before the first frame update
    private void Awake()
    {
        gameManager.DataGenerated += UpdateVue;
    }

    private void UpdateVue(object sender, EventArgs e)
    {
        origin.position = new Vector3((float)gameManager.Data.Width / 2, (float)gameManager.Data.Height / 2, -10);
    }
}
