using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerInput input;
    [SerializeField] private GameManager manager;
    
    private Bomberman _player;
    private Vector2 _movement;
    private void Start()
    {
        _player = manager.Data.Bombers.Find(bomberman => bomberman.Type == BombermanType.Player);
    }

    private void Update()
    {
        _movement = input.actions["movement"].ReadValue<Vector2>();
        if (_movement.x != 0 && _movement.y != 0)
        {
            _player.BombermanAction = BombermanAction.None;
        }
        if (input.actions["Bomb"].IsPressed())
        {
            _player.BombermanAction = BombermanAction.Bomb;
            return;
        }
        _player.BombermanAction = _movement.x != 0 ? _movement.x switch
            {
                > 0 => BombermanAction.Right,
                < 0 => BombermanAction.Left,
                _ => BombermanAction.None
            } :
            _movement.y != 0 ? _movement.y switch
            {
                > 0 => BombermanAction.Up,
                < 0 => BombermanAction.Down,
                _ => BombermanAction.None
            } : BombermanAction.None;
    }
}
