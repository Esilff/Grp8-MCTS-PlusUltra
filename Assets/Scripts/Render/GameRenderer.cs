using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameRenderer : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [SerializeField]private   Sprite GroundColor;
    [SerializeField] private Sprite RockColor;
    [SerializeField] private Sprite WallColor;
    
    private GameObject _bombermanPrefab;
    private GameObject _botPrefab;
    private GameObject _bombPrefab;
    private GameObject _tilePrefab;

    private GameData _data;
    private SpriteRenderer[,] _terrain;

    private void Awake()
    {
        _bombermanPrefab = Resources.Load<GameObject>("Player");
        _botPrefab = Resources.Load<GameObject>("Bot");
        _bombPrefab = Resources.Load<GameObject>("Bomb");
        _tilePrefab = Resources.Load<GameObject>("Tile");
        gameManager.DataGenerated += Generate;
        gameManager.BombDropped += BombDropped;
        gameManager.RockDestroyed += RockDestroyed;
    }

    private void Generate(object sender, EventArgs e)
    {
        _data = gameManager.Data;
        _terrain = new SpriteRenderer[_data.Width, _data.Height];
        GenerateTerrain();
        RenderTerrain();
        InstantiateBombers();
    }

    private void BombDropped(object sender, EventArgs e)
    {
        var bomb = (BombDroppedEventArgs)e;
        Instantiate(_bombPrefab, new Vector3(bomb.Bomb.X, bomb.Bomb.Y, 0), Quaternion.identity)
            .GetComponent<BombRender>().bomb = bomb.Bomb;
    }

    private void RockDestroyed(object sender, EventArgs e)
    {
        RenderTerrain();
    }

    private void GenerateTerrain()
    {
        for (var i = 0; i < _data.Width; i++)
        {
            for (var j = 0; j < _data.Height; j++)
            {
                var tile = Instantiate(_tilePrefab, new Vector3(i,j,0), Quaternion.identity);
                _terrain[i, j] = tile.GetComponentInChildren<SpriteRenderer>();
            }
        }
    }
    
    private void RenderTerrain()
    {
        for (var i = 0; i < _data.Width; i++)
        {
            for (var j = 0; j < _data.Height; j++)
            {
                _terrain[i, j].sprite = _data.Terrain[i, j] switch
                {
                    Tile.Ground => GroundColor,
                    Tile.Rock => RockColor,
                    Tile.Wall => WallColor,
                    _ => WallColor
                };
            }
        }
    }
    
    private void InstantiateBombers()
    {
        foreach (var bomberman in _data.Bombers)
        {
            (Instantiate(bomberman.Type == BombermanType.Player ? _bombermanPrefab : _botPrefab)).
                GetComponent<BombermanRenderer>().Bomberman = bomberman;
        }
    }
}
