
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


public enum Tile
{
    Ground, Wall, Rock
}

public enum GameState
{
    Paused, Running, Finished
}

public enum GameActions
{
    Up, Down, Left, Right, Bomb
}

public class GameData
{
    public Tile[][] Terrain;
    public List<Bomberman> Bombermans;
    

}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject groundSprite;
    [SerializeField] private GameObject wallSprite;
    [SerializeField] private GameObject rockSprite;
    [SerializeField] private Transform camera;
    [SerializeField] private GameConfig config;

    public GameData Data { get; set; } 
    
    private GameObject _playerPrefab;
    private GameObject _botPrefab;
    private readonly List<GameObject> _playersGameObjects = new(); //playerId -> position

    public List<Vector2> Bombs { get; set; }


    private void Awake()
    {
        Assert.AreNotEqual(0, config.terrainDimensions.x % 2);
        Assert.AreNotEqual(0, config.terrainDimensions.y % 2);
        _playerPrefab = Resources.Load<GameObject>("Player");
        _botPrefab = Resources.Load<GameObject>("Bot");
        Data = new GameData
        {
            Terrain = new Tile[config.terrainDimensions.x][]
        };
        for (var i = 0; i < Data.Terrain.Length; i++)
        {
            Data.Terrain[i] = new Tile[config.terrainDimensions.y];
        }

        Data.Bombermans = new()
        {
            new Bomberman { X = 1, Y = 1, State = BombermanState.Player, BombermanAction = BombermanAction.None },
            new Bomberman
            {
                X = Data.Terrain.Length - 2, Y = 1, State = BombermanState.Random,
                BombermanAction = BombermanAction.None
            },
            new Bomberman
            {
                X = 1, Y = Data.Terrain[0].Length - 2, State = BombermanState.Random,
                BombermanAction = BombermanAction.None
            },
            new Bomberman
            {
                X = Data.Terrain.Length - 2, Y = Data.Terrain[0].Length - 2, State = BombermanState.Random,
                BombermanAction = BombermanAction.None
            }
        };
        GenerateTerrain();
    }
    
    

    // Start is called before the first frame update
    void Start()
    {
        camera.position = new Vector3(Data.Terrain.Length / 2, Data.Terrain[0].Length / 2, -10);
        _playersGameObjects.Add(Instantiate(_playerPrefab, new Vector3(Data.Bombermans[0].X, Data.Bombermans[0].Y, 0), Quaternion.identity));
        for (var i = 1; i < Data.Bombermans.Count; i++)
        {
            _playersGameObjects.Add(Instantiate(_botPrefab, new Vector3(Data.Bombermans[i].X, Data.Bombermans[i].Y, 0), Quaternion.identity));
        }
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        UpdatePlayers();
        RenderPlayers();
    }

    private void GenerateTerrain()
    {
        for (var i = 0; i < Data.Terrain.Length; i++)
        {
            for (var j = 0; j < Data.Terrain[i].Length; j++)
            {
                if (i == 0 || i == Data.Terrain.Length - 1 || j == 0 || j == Data.Terrain[0].Length - 1 || (i % 2 == 0 && j % 2 == 0))
                {
                    Data.Terrain[i][j] = Tile.Wall;
                }
                else if (!((i < 3 && j < 3) || (i < 3 && j > Data.Terrain[0].Length - 4) || (i > Data.Terrain.Length - 4 && j < 3)
                           || (i > Data.Terrain.Length - 4 && j > Data.Terrain[0].Length - 4)))
                {
                    float rng = Random.Range(0, 100);
                    Data.Terrain[i][j] = rng > config.rockRate ? Tile.Ground : Tile.Rock;
                }
            }
        }
        for (var i = 0; i < Data.Terrain.Length; i++)
        {
            for (var j = 0; j < Data.Terrain[i].Length; j++)
            {
                var tile = Data.Terrain[i][j] switch
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

    private void UpdatePlayers()
    {
        Data.Bombermans.ForEach(bomberman =>
        {
            switch (bomberman.BombermanAction)
            {
                
                case BombermanAction.None:
                    break;
                case BombermanAction.Bomb:
                    break;
                case BombermanAction.Up:
                    if (Data.Terrain[(int)(bomberman.X + 0.5f)][(int)(bomberman.Y + 1f)] == Tile.Ground)
                        bomberman.Y += 1 * Time.deltaTime * 2;
                    break;
                case BombermanAction.Down:
                    if (Data.Terrain[(int)(bomberman.X + 0.5f)][(int)bomberman.Y] == Tile.Ground)
                        bomberman.Y -= 1 * Time.deltaTime * 2;
                    break;
                case BombermanAction.Left:
                    if (Data.Terrain[(int)bomberman.X][(int)(bomberman.Y + 0.5f)] == Tile.Ground)
                        bomberman.X -= 1 * Time.deltaTime * 2;
                    break;
                case BombermanAction.Right:
                    if (Data.Terrain[(int)(bomberman.X + 1f)][(int)(bomberman.Y + 0.5f)] == Tile.Ground)
                        bomberman.X += 1 * Time.deltaTime * 2;
                    break;
                default:
                    break;
            }
        });
    }

    private void RenderPlayers()
    {
        for (var i = 0; i < Data.Bombermans.Count; i++)
        {
            _playersGameObjects[i].transform.position = Data.Bombermans[i].Position;
        }
    }

}
