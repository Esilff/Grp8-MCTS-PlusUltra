using System.Collections.Generic;

namespace Util
{
    public class PlayerGeneration
    {
        public static List<Bomberman> DefaultSpawn(int width, int height, BombermanType botType = BombermanType.Random, int playersCount = 1)
        {
            List<Bomberman> bombers = new()
            {
                new Bomberman { X = 1.5f, Y = 1.5f, BombermanAction = BombermanAction.None, CanBomb = true},
                new Bomberman { X = width - 1.5f, Y = 1.5f, BombermanAction = BombermanAction.None, CanBomb = true},
                new Bomberman { X = 1.5f, Y = height - 1.5f, BombermanAction = BombermanAction.None, CanBomb = true},
                new Bomberman { X = width - 1.5f, Y = height - 1.5f, BombermanAction = BombermanAction.None, CanBomb = true}
            };
            for (var i = 0; i < bombers.Count; i++)
            {
                bombers[i].Type = i < playersCount ? BombermanType.Player : botType;
            }
            return bombers;
        }
    }
}
