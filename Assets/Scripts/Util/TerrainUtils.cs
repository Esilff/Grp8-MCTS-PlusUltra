namespace Util
{
    public class TerrainUtils 
    {
        public static Tile[,] DefaultTerrain(int width, int height, float rockRate)
        {
            var defaultTerrain = new Tile[width, height];
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    if (i == 0 || i == width - 1 || j == 0 || j == height - 1 || (i % 2 == 0 && j % 2 == 0))
                    {
                        defaultTerrain[i, j] = Tile.Wall;
                    } else if (!((i < 3 && j < 3) || (i < 3 && j > height - 4) || (i > width - 4 && j < 3)
                                 || (i > width - 4 && j > height - 4))) // Not in spawning areas
                    {
                        float rng = UnityEngine.Random.Range(0, 1);
                        defaultTerrain[i, j] = rng > rockRate ? Tile.Ground : Tile.Rock;
                    }
                }
            }
            return defaultTerrain;
        }
    }
}
