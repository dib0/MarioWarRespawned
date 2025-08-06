using System;

namespace MarioWarRespawned
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new MarioWarGame();
            game.Run();
        }
    }
}