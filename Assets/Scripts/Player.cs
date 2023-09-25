using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerInput input;
    [SerializeField] private GameManager manager;
    private Bomberman _player;
    private void Start()
    {
        _player = manager.Data.Bombermans.Find(bomberman => bomberman.State == BombermanState.Player);
    }

    private void Update()
    {
        DefineAction();
    }

    private void DefineAction()
    {
        var movement = input.actions["movement"].ReadValue<Vector2>();
        if (movement.x != 0 && movement.y != 0)
        {
            _player.BombermanAction = BombermanAction.None;
            return;
        }

        if (input.actions["Bomb"].IsPressed() && _player.CanBomb)
        {
            _player.BombermanAction = BombermanAction.Bomb;
            return;
        }
        _player.BombermanAction = movement.x != 0 ? movement.x switch
            {
                > 0 => BombermanAction.Right,
                < 0 => BombermanAction.Left,
                _ => BombermanAction.None
            } :
            movement.y != 0 ? movement.y switch
            {
                > 0 => BombermanAction.Up,
                < 0 => BombermanAction.Down,
                _ => BombermanAction.None
            } : BombermanAction.None;
    }
}
