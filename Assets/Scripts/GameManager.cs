using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
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
            return;
        }
        UpdateBombers(Data);
        UpdateTimers(Data);
    }

    private void UpdateBombers(GameData data)
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
                        OnBombDropped(new BombDroppedEventArgs(bomb));
                    }
                    break;
                case BombermanAction.Up:
                    if (data.Terrain[bomberman.Xi, bomberman.Yi] == Tile.Ground)
                    {
                        bomberman.Y += 1 * 0.001f;
                    }
                    break;
                case BombermanAction.Down:
                    if (data.Terrain[bomberman.Xi, bomberman.Yi - 1] == Tile.Ground)
                    {
                        bomberman.Y -= 1 * 0.001f;
                    }
                    break;
                case BombermanAction.Left:
                    if (data.Terrain[bomberman.Xi - 1, bomberman.Yi] == Tile.Ground)
                    {
                        bomberman.X -= 1 * 0.001f;
                    }
                    break;
                case BombermanAction.Right:
                    if (data.Terrain[bomberman.Xi, bomberman.Yi] == Tile.Ground)
                    {
                        bomberman.X += 1 * 0.001f;
                    }
                    break;
            }
        }
    }

    private void UpdateTimers(GameData data)
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
            else
            {
                Explode(bomb, data);
            }
            
        }
        data.Bombs.RemoveAll(bomb => bomb.Cooldown <= 0);
    }

    private void Explode(Bomb bomb, GameData data)
    {
        var rockDestroyed = 0;
        for (int t = 1, b = 1, l = 1, r = 1;
             t < bomb.Radius && b < bomb.Radius && l < bomb.Radius && r < bomb.Radius;
             t++, b++, l++, r++)
        {
            if (!IsOffLimit(bomb.Xi, bomb.Yi + t, data) || data.Terrain[bomb.Xi, bomb.Yi + t] == Tile.Wall)
            {
                t = bomb.Radius;
                Debug.Log("Radius locked");
            }
            else if (data.Terrain[bomb.Xi, bomb.Yi + t] == Tile.Rock)
            {
                Debug.Log("Destroying somfing");
                data.Terrain[bomb.Xi, bomb.Yi + t] = Tile.Ground;
                rockDestroyed++;
            }
        }

        if (rockDestroyed > 0)
        {
            OnRockDestroyed(EventArgs.Empty);
        }
    }
    
    private bool IsOffLimit(int x, int y, GameData data)
    {
        return (x < 0 || x >= data.Width - 1) || (y < 0 || y >= data.Height - 1);
    }
    
}
