using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace lab2
{
    class Program
    {
        interface IRunnable
        {
            IEnumerator<float> CoroutineUpdate();
            void Run();
            bool HasFinished
            {
                get; set;
            }
        }

        static List<IRunnable> AgentList = new List<IRunnable>();
        
        static void GenerateRunnables()
        {
            Console.WriteLine("Zaczynam tworzenie agentow");
            for (int i=0; i<10; i++)
            {
                Console.WriteLine("Agent" + i);
                AgentList.Add(new ConstantCountingAgent(i));
            }
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Agent" + (i+10));
                AgentList.Add(new CountingAgent(i+10));
            }
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Agent" + (i+20));
                AgentList.Add(new SineGeneratingAgent(i+20));
            }
        }
        static void RunThreads()
        {
            Console.WriteLine("Zaczynam startowanie watkow");
            List<Thread> ThreadList = new List<Thread>();
            foreach (var IRunnable in AgentList)
            {
                Thread thread = new Thread(IRunnable.Run);
                thread.Start();
                ThreadList.Add(thread);
            }
            Console.WriteLine("Łączę wątki");
            while (!AgentList.All(IRRunable => IRRunable.HasFinished.Equals(true)))
            {
                Console.Write(".");
                Thread.Sleep(100);
            }
        }

        /*static void RunFibres()
        {
            //List<IEnumerator<float>> IEnumList = new List<IEnumerator<float>>();
            foreach (var IRunnable in AgentList)
            {
                IRunnable.CoroutineUpdate().MoveNext();
            }
        }*/
        static void RunFibres()
        {
            foreach (var IRunnable in AgentList)
            {
                while(IRunnable.CoroutineUpdate().MoveNext())
                {
                }
                Console.Write(".");
            }
        }

        abstract class Agent : IRunnable
        {
            public int ID;
            public bool HasFinished
            {
                get;
                set;
            }
            public Agent(int iD)
            {
                ID = iD;
                HasFinished = false;
            }
            
            public abstract void Update();
            public void Run()
            {
                while (!HasFinished)
                {
                    Update();
                    Thread.Sleep(100);
                }
            }
            public IEnumerator<float> CoroutineUpdate()
            {
                while (!HasFinished)
                {
                    Update();
                    yield return 0;
                }
                yield break;
            }
        }

        class ConstantCountingAgent : Agent
        {
            int count;
            public ConstantCountingAgent(int iD): base(iD)
            {
            }
            public override void Update()
            {
                if (count < 10)
                {
                    count++;
                }
                else
                {
                   // Console.WriteLine(ID);
                    HasFinished = true;
                }
            }
        }
        
        class CountingAgent : Agent
        {
            int count;
            public CountingAgent(int iD): base(iD)
            {
            }
            public override void Update()
            {
                if (count < ID)
                {
                    count++;
                }
                else
                {
                   // Console.WriteLine(ID);
                    HasFinished = true;
                }
            }
        }
        
        class SineGeneratingAgent : Agent
        {
            int count;
            public double output;
            public SineGeneratingAgent(int iD): base(iD)
            {
                count = 0;
                output = 0;
            }
            int SinGen()
            {
                return 0;
            }
            public override void Update()
            {
               if(count < 10*(ID % 10))
                {
                    output = Math.Sin(count);
                    //Console.WriteLine(output);
                    count++;
                }
                else
                {
                   // Console.WriteLine(ID);
                    HasFinished = true;
                }
            }
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("Zaczynam");
            GenerateRunnables();
            Console.WriteLine("Wygenerowałem agenty");
            RunFibres();
            Console.WriteLine("Połączyłem wątki");
            Console.Read();
        }
        
        
    }

    
}
