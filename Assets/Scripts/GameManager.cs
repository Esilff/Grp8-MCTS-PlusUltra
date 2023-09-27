
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
    public List<Bomb> bombs;

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
    private GameObject _bombPrefab;
    private readonly List<GameObject> _playersGameObjects = new(); //playerId -> position
    private SpriteRenderer[,] _map;
    
    public List<Vector2> Bombs { get; set; }
    public bool HasEnded { get; set; } = false;

    private void Awake()
    {
        Assert.AreNotEqual(0, config.terrainDimensions.x % 2);
        Assert.AreNotEqual(0, config.terrainDimensions.y % 2);
        _playerPrefab = Resources.Load<GameObject>("Player");
        _botPrefab = Resources.Load<GameObject>("Bot");
        _bombPrefab = Resources.Load<GameObject>("Bomb");
        _map = new SpriteRenderer[config.terrainDimensions.x, config.terrainDimensions.y];
        Data = new GameData
        {
            Terrain = new Tile[config.terrainDimensions.x][],
            bombs = new List<Bomb>()
        };
        for (var i = 0; i < Data.Terrain.Length; i++)
        {
            Data.Terrain[i] = new Tile[config.terrainDimensions.y];
        }

        Data.Bombermans = new()
        {
            new Bomberman { X = 1, Y = 1, State = BombermanState.Player, BombermanAction = BombermanAction.None, CanBomb = true},
            new Bomberman
            {
                X = Data.Terrain.Length - 2, Y = 1, State = BombermanState.Random,
                BombermanAction = BombermanAction.None, CanBomb = true
            },
            new Bomberman
            {
                X = 1, Y = Data.Terrain[0].Length - 2, State = BombermanState.Random,
                BombermanAction = BombermanAction.None, CanBomb = true
            },
            new Bomberman
            {
                X = Data.Terrain.Length - 2, Y = Data.Terrain[0].Length - 2, State = BombermanState.Random,
                BombermanAction = BombermanAction.None, CanBomb = true
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
        if (HasEnded)
        {
            Debug.Log("Game has ended");
            return;
        }
        UpdatePlayers();
        RenderPlayers();
        UpdateBombs();
        UpdateTerrain();
        CheckEnd();
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
                    float rng = UnityEngine.Random.Range(0, 100);
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
                var iTile = Instantiate(tile, new Vector3(i, j, 0), Quaternion.identity);
                _map[i, j] = iTile.GetComponentInChildren<SpriteRenderer>();
            }
        }
    }

    //Dans MCTS passer reference de la copie de la data
    private void UpdatePlayers()
    {
        Data.Bombermans.ForEach(bomberman =>
        {
            switch (bomberman.BombermanAction)
            {
                
                case BombermanAction.None:
                    break;
                case BombermanAction.Bomb:
                    // if (bomberman.BombCooldown < 0)
                    // {
                    //     bomberman.CanBomb = true;
                    // }
                    // if (!bomberman.CanBomb)
                    // {
                    //     bomberman.BombCooldown -= Time.deltaTime;
                    //     break;
                    // }
                    var hBomb = new Bomb
                    {
                        X = (int)bomberman.X, Y = (int)bomberman.Y, Radius = config.bombRadius,
                        Cooldown = config.bombCooldown
                    };
                    Data.bombs.Add(hBomb);
                    (Instantiate(_bombPrefab, new Vector3((int)(bomberman.X + 0.5f), (int)(bomberman.Y + 0.5f), 0), Quaternion.identity)).
                        GetComponent<BombRender>().bomb = hBomb;
                    bomberman.CanBomb = false;
                    bomberman.BombCooldown = 2;
                    break;
                case BombermanAction.Up:
                    if (Data.Terrain[(int)(bomberman.X + 0.5f)][(int)(bomberman.Y + 1f)] == Tile.Ground)
                        bomberman.Y += 1 * Time.deltaTime * 3;
                    break;
                case BombermanAction.Down:
                    if (Data.Terrain[(int)(bomberman.X + 0.5f)][(int)bomberman.Y] == Tile.Ground)
                        bomberman.Y -= 1 * Time.deltaTime * 3;
                    break;
                case BombermanAction.Left:
                    if (Data.Terrain[(int)bomberman.X][(int)(bomberman.Y + 0.5f)] == Tile.Ground)
                        bomberman.X -= 1 * Time.deltaTime * 3;
                    break;
                case BombermanAction.Right:
                    if (Data.Terrain[(int)(bomberman.X + 1f)][(int)(bomberman.Y + 0.5f)] == Tile.Ground)
                        bomberman.X += 1 * Time.deltaTime * 3;
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

    private void UpdateTerrain()
    {
        for (int i = 0; i < config.terrainDimensions.x; i++)
        {
            for (var j = 0; j < config.terrainDimensions.y; j++)
            {
                _map[i, j].color = Data.Terrain[i][j] switch
                {
                    Tile.Ground => new Color(0.6792453f,0.6792453f,0.6792453f),
                    Tile.Wall => new Color(0.1792453f,0.1748487f,0.1748487f),
                    Tile.Rock => new Color(0.3396226f,0.3008213f,0.2351726f)
                };
            }
        }
    }
    //Passer une ref de la copie de la data dans le MCTS
    private void UpdateBombs()
    {
        Data.bombs.ForEach(bomb =>
        {
            bomb.Cooldown -= Time.deltaTime;
            if (bomb.Cooldown < 0)
            {
                for (var i = -bomb.Radius; i <= bomb.Radius; i++)
                {
                    if (i == 0) continue;
                    if (!IsOffLimit(bomb.X + i, bomb.Y) && Data.Terrain[bomb.X + i][bomb.Y] == Tile.Rock)
                    {
                        Data.Terrain[bomb.X +i ][bomb.Y] = Tile.Ground;
                    }
                    if (!IsOffLimit(bomb.X, bomb.Y + i) && Data.Terrain[bomb.X][bomb.Y + i] == Tile.Rock)
                    {
                        Data.Terrain[bomb.X][bomb.Y + i] = Tile.Ground;
                    }
                    Data.Bombermans.RemoveAll(bomberman => Vector3.Distance(bomberman.Position,new Vector3(bomb.X + i, bomb.Y, 0)) < bomb.Radius);
                    Data.Bombermans.RemoveAll(bomberman => Vector3.Distance(bomberman.Position, new Vector3(bomb.X, bomb.Y + i, 0)) <bomb.Radius);

                }
            }
        });
        Data.bombs.RemoveAll(bomb => bomb.Cooldown < 0);
    }

    private bool IsOffLimit(int x, int y)
    {
        return (x < 0 || x >= Data.Terrain.Length - 1) || (y < 0 || y >= Data.Terrain[0].Length - 1);
    }

    private void CheckEnd()
    {
        if (Data.Bombermans.Count <= 1) HasEnded = true;
    }

}
