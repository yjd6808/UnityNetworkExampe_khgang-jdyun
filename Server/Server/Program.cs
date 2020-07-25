using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkServer.Get().Start();
            do
            {
                Console.WriteLine("아무키나 입력시 종료됨.");
            } while (Console.ReadLine().Length >= 0);
        }
    }
}
