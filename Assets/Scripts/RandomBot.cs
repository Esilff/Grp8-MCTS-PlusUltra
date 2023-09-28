using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBot : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private GameData _data;

    private void Start()
    {
        _data = gameManager.Data;
    }

    private void Update()
    {
        _data.Bombers.ForEach(bomberman =>
        {
            if (bomberman.Type == BombermanType.Random)
            {
                bomberman.BombermanAction = RandomAction();
            }
        });
    }

    private BombermanAction RandomAction()
    {
        var count = Enum.GetValues(typeof(BombermanAction)).Length;
        var rnd = UnityEngine.Random.Range(0, count);
        return Enum.Parse<BombermanAction>(rnd.ToString());
    }
}

