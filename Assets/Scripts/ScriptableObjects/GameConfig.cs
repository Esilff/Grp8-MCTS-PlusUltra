using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game configuration", menuName = "Bomberman/GameConfiguration")]
public class GameConfig : ScriptableObject
{
    public Vector2Int terrainDimensions;
    public int rockRate;
    public float bombCooldown;
    public int bombRadius;
}
