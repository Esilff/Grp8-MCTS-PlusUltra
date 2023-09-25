
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


public enum Tile
{
    Ground, Wall, Rock
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject groundSprite;
    [SerializeField] private GameObject wallSprite;
    [SerializeField] private GameObject rockSprite;
    [SerializeField] private Transform camera;
    [SerializeField] private GameConfig config;

    private GameObject _playerPrefab;
    public Tile[][] Terrain { get; set; } //[x][y] -> Tile

    public List<Player> Players { get; set; }
    private readonly List<GameObject> _playersGameObjects = new(); //playerId -> position

    public List<Vector2> Bombs { get; set; }
    
    
    private void Awake()
    {
        Assert.AreNotEqual(0, config.terrainDimensions.x % 2);
        Assert.AreNotEqual(0, config.terrainDimensions.y % 2);
        _playerPrefab = Resources.Load<GameObject>("Player");
        Terrain = new Tile[config.terrainDimensions.x][];
        for (var i = 0; i < Terrain.Length; i++)
        {
            Terrain[i] = new Tile[config.terrainDimensions.y];
        }
        
        Players = new List<Player>
        {
            new() { Position = new Vector2(1, 1), Behavior = PlayerBehavior.Player },
            new() { Position = new Vector2(Terrain.Length, 1), Behavior = PlayerBehavior.Random },
            new() { Position = new Vector2(1, Terrain[0].Length), Behavior = PlayerBehavior.Montecarlo },
            new() { Position = new Vector2(Terrain.Length, Terrain[0].Length), Behavior = PlayerBehavior.Montecarlo }
        };
        
        for (var i = 0; i < Terrain.Length; i++)
        {
            for (var j = 0; j < Terrain[i].Length; j++)
            {
                if (i == 0 || i == Terrain.Length - 1 || j == 0 || j == Terrain[0].Length - 1 || (i % 2 == 0 && j % 2 == 0))
                {
                    Terrain[i][j] = Tile.Wall;
                }
                else if (!((i < 3 && j < 3) || (i < 3 && j > Terrain[0].Length - 4) || (i > Terrain.Length - 4 && j < 3)
                         || (i > Terrain.Length - 4 && j > Terrain[0].Length - 4)))
                {
                    float rng = Random.Range(0, 100);
                    Terrain[i][j] = rng > config.rockRate ? Tile.Ground : Tile.Rock;
                }
            }
        }
        InitialRender();
    }
    
    

    // Start is called before the first frame update
    void Start()
    {
        camera.position = new Vector3(Terrain.Length / 2, Terrain[0].Length / 2, -10);
        _playersGameObjects.Add(Instantiate(_playerPrefab, new Vector3(Players[0].Position.x, Players[0].Position.y, 0), Quaternion.identity));
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        for (var i = 0; i < _playersGameObjects.Count; i++)
        {
            _playersGameObjects[i].transform.position = new Vector3(Players[i].Position.x, Players[i].Position.y, 0);
        }
    }

    private void InitialRender()
    {
        for (var i = 0; i < Terrain.Length; i++)
        {
            for (var j = 0; j < Terrain[i].Length; j++)
            {
                var tile = Terrain[i][j] switch
                {
                    Tile.Ground => groundSprite,
                    Tile.Wall => wallSprite,
                    Tile.Rock => rockSprite,
                    _ => groundSprite
                };
                Instantiate(tile, new Vector3(i, j, 0), Quaternion.identity);
            }
        }
    }

    private bool CloseToPlayer(int x, int y, Player player)
    {
        Vector2 distance = new Vector2(player.Position.x - x, player.Position.y - y);
        Debug.Log($"Distance : {distance}");
        return (distance.x < 2 || distance.y < 2);
    }

}
