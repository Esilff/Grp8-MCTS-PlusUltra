using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Util;

public enum Tile
{
    Ground, Wall, Rock
}

public enum GameState
{
    Paused, Running, Finished
}

public class GameData
{
    public Tile[,] Terrain;
    public int Width;
    public int Height;
    public List<Bomberman> Bombers;
    public List<Bomb> Bombs;
    public GameState GameState;

    public GameData() {}
    
    public GameData(GameData copy)
    {
        Width = copy.Width;
        Height = copy.Height;
        Bombers = new List<Bomberman>();
        copy.Bombers.ForEach(bomberman => Bombers.Add(bomberman.Copy()));
        Bombs = new List<Bomb>();
        copy.Bombs.ForEach(bomb => Bombs.Add(bomb.Copy()));
        GameState = copy.GameState;
        Terrain = new Tile[Width,Height];
        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                Terrain[i, j] = copy.Terrain[i, j];
            }
        }
    }
}

public class BombDroppedEventArgs : EventArgs
{
    public Bomb Bomb { get; }

    public BombDroppedEventArgs(Bomb bomb)
    {
        Bomb = bomb;
    }
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    
    public GameData Data { get; set; }

    public event EventHandler RockDestroyed;
    public event EventHandler BombDropped;
    public event EventHandler DataGenerated;

    protected virtual void OnBombermanInvoked(EventArgs e)
    {
        DataGenerated?.Invoke(this,e);
    }

    protected virtual void OnBombDropped(BombDroppedEventArgs bomb)
    {
        BombDropped?.Invoke(this,bomb);
    }

    protected virtual void OnRockDestroyed(EventArgs e)
    {
        RockDestroyed?.Invoke(this, e);
    }
    
    //GameRender should Awake before this gameObject
    
    private void Awake()
    {
        Assert.AreNotEqual(0, config.terrainDimensions.x % 2);
        Assert.AreNotEqual(0, config.terrainDimensions.y % 2);
        Assert.IsTrue(config.rockRate <= 1);
        Data = new GameData
        {
            Width = config.terrainDimensions.x,
            Height = config.terrainDimensions.y,
            Terrain = TerrainUtils.DefaultTerrain(config.terrainDimensions.x, config.terrainDimensions.y, config.rockRate),
            Bombers = PlayerGeneration.DefaultSpawn(config.terrainDimensions.x, config.terrainDimensions.y),
            Bombs = new List<Bomb>(),
            GameState = GameState.Running
        };
        OnBombermanInvoked(EventArgs.Empty);
    }

    private void Update()
    {
        if (Data.GameState == GameState.Finished)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }
        UpdateBombers(Data, true);
        UpdateTimers(Data, true);
        CheckEndOfGame(Data);
    }

    public void UpdateBombers(GameData data, bool events = false)
    {
        foreach (var bomberman in Data.Bombers)
        {
            switch (bomberman.BombermanAction)
            {
                default:
                case BombermanAction.None:
                    break;
                case BombermanAction.Bomb:
                    if (bomberman.CanBomb)
                    {
                        bomberman.CanBomb = false;
                        bomberman.BombCooldown = 2;
                        var bomb = new Bomb
                        {
                            Cooldown = config.bombCooldown, Radius = config.bombRadius,
                            X = bomberman.X, Y = bomberman.Y
                        };
                        data.Bombs.Add(bomb);
                        if (events) OnBombDropped(new BombDroppedEventArgs(bomb));
                    }
                    break;
                case BombermanAction.Up:
                    if (data.Terrain[bomberman.Xi, bomberman.Yi] == Tile.Ground)
                    {
                        bomberman.Y += 1 * 0.003f;
                    }
                    break;
                case BombermanAction.Down:
                    if (data.Terrain[bomberman.Xi, bomberman.Yi - 1] == Tile.Ground)
                    {
                        bomberman.Y -= 1 * 0.003f;
                    }
                    break;
                case BombermanAction.Left:
                    if (data.Terrain[bomberman.Xi - 1, bomberman.Yi] == Tile.Ground)
                    {
                        bomberman.X -= 1 * 0.003f;
                    }
                    break;
                case BombermanAction.Right:
                    if (data.Terrain[bomberman.Xi, bomberman.Yi] == Tile.Ground)
                    {
                        bomberman.X += 1 * 0.003f;
                    }
                    break;
            }
        }
    }

    public void UpdateTimers(GameData data, bool events = false)
    {
        foreach (var bomberman in data.Bombers)
        {
            if (bomberman.BombCooldown > 0)
            {
                bomberman.BombCooldown -= Time.deltaTime;
            }
            else
            {
                bomberman.CanBomb = true;
            }
        }
        foreach (var bomb in data.Bombs)
        {
            if (bomb.Cooldown > 0)
            {
                bomb.Cooldown -= Time.deltaTime;
            }
            if (bomb.Cooldown < 0) //Si c'est un else la bombe sera détruite avant même le call de la méthode Explode
            {
                Explode(bomb, data, events);
            }
            
        }
        data.Bombs.RemoveAll(bomb => bomb.Cooldown <= 0);
    }

    private void Explode(Bomb bomb, GameData data, bool events = false)
    {
        var rockDestroyed = 0;
        for (var t = 1; t <= bomb.Radius; t++)
        {
            if (IsOffLimit(bomb.Xi, bomb.Yi + t, data) || data.Terrain[bomb.Xi, bomb.Yi + t] == Tile.Wall)
            {
                t = bomb.Radius;
            }
            else if (data.Terrain[bomb.Xi, bomb.Yi + t] == Tile.Rock)
            {
                data.Terrain[bomb.Xi, bomb.Yi + t] = Tile.Ground;
                rockDestroyed++;
            } else if (data.Terrain[bomb.Xi, bomb.Yi + t] == Tile.Ground)
            {
                data.Bombers.ForEach(bomberman =>
                {
                    if (Vector3.Distance(bomberman.Position, new Vector3(bomb.X, bomb.Y + t, 0)) < 0.5f)
                    {
                        bomberman.IsDead = true;
                    }
                });
            }
        }
        
        for (var r = 1; r <= bomb.Radius; r++)
        {
            if (IsOffLimit(bomb.Xi + r, bomb.Yi, data) || data.Terrain[bomb.Xi + r, bomb.Yi] == Tile.Wall)
            {
                r = bomb.Radius;
            }
            else if (data.Terrain[bomb.Xi + r, bomb.Yi] == Tile.Rock)
            {
                data.Terrain[bomb.Xi + r, bomb.Yi] = Tile.Ground;
                rockDestroyed++;
            } 
            else if (data.Terrain[bomb.Xi + r, bomb.Yi] == Tile.Ground)
            {
                data.Bombers.ForEach(bomberman =>
                {
                    if (Vector3.Distance(bomberman.Position, new Vector3(bomb.X + r, bomb.Y, 0)) < 0.5f)
                    {
                        bomberman.IsDead = true;
                    }
                });
            }
        }
        for (var l = 1; l <= bomb.Radius; l++)
        {
            if (IsOffLimit(bomb.Xi - l, bomb.Yi, data) || data.Terrain[bomb.Xi - l, bomb.Yi] == Tile.Wall)
            {
                l = bomb.Radius;
            }
            else if (data.Terrain[bomb.Xi - l, bomb.Yi] == Tile.Rock)
            {
                data.Terrain[bomb.Xi - l, bomb.Yi] = Tile.Ground;
                rockDestroyed++;
            }
            else if (data.Terrain[bomb.Xi - l, bomb.Yi] == Tile.Ground)
            {
                data.Bombers.ForEach(bomberman =>
                {
                    if (Vector3.Distance(bomberman.Position, new Vector3(bomb.X - l, bomb.Y, 0)) < 0.5f)
                    {
                        bomberman.IsDead = true;
                    }
                });
            }
        }
        for (var b = 1; b <= bomb.Radius; b++)
        {
            if (IsOffLimit(bomb.Xi, bomb.Yi - b, data) || data.Terrain[bomb.Xi, bomb.Yi - b] == Tile.Wall)
            {
                b = bomb.Radius;
            }
            else if (data.Terrain[bomb.Xi, bomb.Yi - b] == Tile.Rock)
            {
                data.Terrain[bomb.Xi, bomb.Yi - b] = Tile.Ground;
                rockDestroyed++;
            } else if (data.Terrain[bomb.Xi, bomb.Yi - b] == Tile.Ground)
            {
                data.Bombers.ForEach(bomberman =>
                {
                    if (Vector3.Distance(bomberman.Position, new Vector3(bomb.X, bomb.Y - b, 0)) < 0.5f)
                    {
                        bomberman.IsDead = true;
                    }
                });
            }
        }
        data.Bombers.ForEach(bomberman =>
        {
            if (Vector3.Distance(bomberman.Position, new Vector3(bomb.X, bomb.Y, 0)) < 0.5f)
            {
                bomberman.IsDead = true;
            }
        });
        if (rockDestroyed > 0)
        {
            if (events) OnRockDestroyed(EventArgs.Empty);
        }
    }
    
    private bool IsOffLimit(int x, int y, GameData data)
    {
        return (x < 0 || x >= data.Width - 1) || (y < 0 || y >= data.Height - 1);
    }

    public void CheckEndOfGame(GameData data)
    {
        if (data.Bombers.Count(bomberman => !bomberman.IsDead) <= 1)
        {
            data.Bombs.ForEach(bomb => bomb.Cooldown = -1);
            data.GameState = GameState.Finished;
        }
    }
    
}
