using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _03_18
{
    class Program
    {
        static Random random = new Random();
        static void GenerateList(List<int> lista, int size)
        {

            for (int j = 0; j < size; j++)
            {
                lista.Add(1);
            }
        }
        static void GenerateRunnables(List<int> lista, List<int> zwrot, int size, List<Agent> agenty)
        {
            int i = 0;
            bool divided = false;
            do
            {
                if (i < (lista.Count - size))
                {
                    agenty.Add(new CountingAgent(lista.GetRange(i, size),zwrot));
                }
                else
                {
                    agenty.Add(new CountingAgent(lista.GetRange(i, lista.Count - i),zwrot));
                    divided = true;
                }
                i += size;

            } while (!divided);
        }
        static void RunRunnables(List<Thread> ThreadList, List<Agent> agenty)
        {
            
            foreach (var agent in agenty)
            {
                Thread thread = new Thread(agent.Run);
                ThreadList.Add(thread);
                thread.Start();
            }
        }

        static void JoinRunnables(List<Thread> ThreadList)
        {
            foreach (var thread in ThreadList)
            {
                thread.Join();
            }            
        }
        
        abstract class Agent
        {
            public bool HasFinished;
            public abstract void Run();
        }
        class CountingAgent : Agent
        {
            List<int> liczby;
            List<int> liczbyOut;
            public CountingAgent(List<int> Liczby, List<int> ListaOut)
            {
                liczby = Liczby;
                HasFinished = false;
                liczbyOut = ListaOut;
            }
            public override void Run()
            {
                int temp = 0;
                foreach(var i in liczby)
                {
                    temp += i;
                    //Thread.Sleep(1);
                }
                liczbyOut.Add(temp);
                HasFinished = true;
                
            }
        }
        class MasterAgent : Agent
        {
            public MasterAgent()
            {
                HasFinished = false;
            }
            public override void Run()
            {

            }
        }
        
        static void Main(string[] args)
        {
            List<int> lista = new List<int>();
            GenerateList(lista, 1023);
            Console.WriteLine("Wygenerowalem liste");
            while (lista.Count!=1)
            {
                List<Thread> ThreadList = new List<Thread>();
                List<int> zwrot = new List<int>();
                List<Agent> agenty = new List<Agent>();
                Console.WriteLine("aktualna lista:");
                foreach(var i in lista)
                {
                    Console.Write(i + " ");
                }
                GenerateRunnables(lista, zwrot, 9, agenty);
                Console.WriteLine("Utworzylem agenty" + agenty.Count);
                RunRunnables(ThreadList, agenty);
                Console.WriteLine("Wystartowalem agenty");
                JoinRunnables(ThreadList);
                Console.WriteLine("Agenty policzyły");
                lista = zwrot;
                Console.WriteLine("Lista po połączeniu");
                foreach (var i in lista)
                {
                    Console.Write(i + " ");
                }
            }
        }
    }
}
