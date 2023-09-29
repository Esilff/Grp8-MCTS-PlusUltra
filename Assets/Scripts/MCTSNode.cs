using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MCTSNode
{
    public MCTSNode Parent { get; set; }
    public List<MCTSNode> Children { get; set; } = new();
    public GameData Data { get; set; }
    public BombermanAction SelectedAction { get; set; } = BombermanAction.Null;
    public int Victory { get; set; } = 0;
    public int PartyPlayed { get; set; } = 0;
}
