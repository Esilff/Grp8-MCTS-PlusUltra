using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MCTS : MonoBehaviour
{
    [SerializeField] private GameManager manager;

    private List<MCTSNode> _nodes;
    private MCTSNode _root;
    private MCTSNode _leaf;

    private static int ActionRange = 5;
    public int SimulationCount { get; set; } = 20;

    public int Iterations { get; set; } = 100;

    private List<BombermanAction> _actions;
    // Update is called once per frame
    void Update()
    {
        for (var i = 0; i < 4; i++)
        {
            _actions[i] = RandomAction();
        }
        manager.Data.Bombers.ForEach(bomberman =>
        {
            if (bomberman.Type is BombermanType.Mont) bomberman.BombermanAction = RunMcts(bomberman);
        });

        
        
    }

    private BombermanAction RunMcts(Bomberman bomber)
    {
        int bomberman = manager.Data.Bombers.IndexOf(bomber);
        _nodes = new List<MCTSNode>() {new()
        {
            Parent = null, Data = new GameData(manager.Data)
        }};
        for (var i = 0; i < Iterations; i++)
        {
            Selection();
            Expansion();
            Simulation(bomberman);
            Backpropagation();
        }
        return _nodes.OrderByDescending(node => node.Victory).FirstOrDefault()!.SelectedAction;
    }

    private void Selection()
    {
        _leaf = RandomSelection();
    }

    private void Expansion()
    {
        BombermanAction action = RandomAction();

        var node = new MCTSNode { 
            Parent = _leaf, 
            Data = new GameData(_leaf.Data),
            SelectedAction = action
        };
        _leaf = node;
        for (var i = 0; i < _leaf.Data.Bombers.Count; i++)
        {
            _leaf.Data.Bombers[i].BombermanAction = _actions[i];
        }
        manager.UpdateBombers(_leaf.Data);
        manager.UpdateTimers(_leaf.Data);
        manager.CheckEndOfGame(_leaf.Data);
        
        if (node.Data.GameState != GameState.Finished) _nodes.Add(node);
    }

    private void Simulation(int bomber)
    {
        //Rajouter des simultaions via for
        for (var i = 0; i < SimulationCount; i++)
        {
            var simulationData = new GameData(_leaf.Data);
            while (simulationData.GameState != GameState.Finished)
            {
                simulationData.Bombers.ForEach(bomberman => bomberman.BombermanAction = RandomAction());
                manager.UpdateBombers(simulationData);
                manager.UpdateTimers(simulationData);
                manager.CheckEndOfGame(simulationData);
            }
            _leaf.PartyPlayed += 1;
            if (!_leaf.Data.Bombers[bomber].IsDead) _leaf.Victory += 1;
        }
    }

    private void Backpropagation()
    {
        var currentNode = _leaf;
        while (currentNode.Parent != null)
        {
            currentNode.Parent.Victory += _leaf.Victory;
            currentNode.PartyPlayed += _leaf.PartyPlayed;
            currentNode = currentNode.Parent;
        }
    }

    private MCTSNode RandomSelection()
    {
        return _nodes[Random.Range(0, _nodes.Count)];
    }
    
    private BombermanAction RandomAction()
    {
        var count = Enum.GetValues(typeof(BombermanAction)).Length;
        var rnd = UnityEngine.Random.Range(0, count);
        return Enum.Parse<BombermanAction>(rnd.ToString());
    }
}
