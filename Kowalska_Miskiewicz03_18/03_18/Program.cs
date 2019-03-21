using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _03_18
{
    class Program
    {
        static List<int> lista = new List<int>();
        static Random random = new Random();
        static List<Agent> agenty = new List<Agent>();
        stat
        static void GenerateList()
        {

            for (int j = 0; j < 1000; j++)
            {
                lista.Add(random.Next(0, 100));
            }
        }
        static void GenerateRunnables()
        {
            int i = 0;
            bool divided = false;
            do
            {
                if (i < (lista.Count - 1))
                {
                    agenty.Add(new CountingAgent(lista.GetRange(i, i + 249)));
                }
                else
                {
                    agenty.Add(new CountingAgent(lista.GetRange(i, lista.Count - 1)));
                    divided = true;
                }
                i += 250;

            } while (!divided);
        }
        static void RunRunnables()
        {
            foreach(var agent in agenty)
            {
                agent.Run();
            }

        }
        abstract class Agent
        {
            public bool HasFinished;
            public abstract void Run();
        }
        class CountingAgent : Agent
        {
            public int wynik;
            List<int> liczby;
            List<int> liczbyOut;
            public CountingAgent(List<int> Liczby, List<int> ListaOut)
            {
                liczby = Liczby;
                HasFinished = false;
                wynik = 0;
                liczbyOut = ListaOut;
            }
            public override void Run()
            {
                int temp = 0;
                foreach(var i in liczby)
                {
                    temp += i;
                }
                liczbyOut.Add(temp);
                HasFinished = true;
                
            }
        }

        
        static void Main(string[] args)
        {
            
        }
    }
}
