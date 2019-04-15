using System;
using System.Threading;

namespace Lab_synch
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Start[] agenci = new Start[300];
            Bank PKO = new Bank();
            for(int i = 0; i<100;i++)
            {
                agenci[i] = new AgentDodawanie(PKO, i);
            }
            for (int i = 100; i < 200; i++)
            {
                agenci[i] = new AgentOdejmowanie(PKO, i-100);
            }
            for (int i = 200; i < 300; i++)
            {
                agenci[i] = new AgentZmniejszOProcent(PKO, i/100);
            }



            Thread[] threads = new Thread[301];
            threads[300] = new Thread(PKO.Run);
            threads[300].Start();
            for(int i = 0; i < 300; i++)
            {
                threads[i] = new Thread(agenci[i].Run);
                threads[i].Start();
            }
            

            Console.ReadLine();
        }
    }
}