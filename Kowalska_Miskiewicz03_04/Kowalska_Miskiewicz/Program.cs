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
            void Run();
            bool HasFinished
            {
                get; set;
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
                    Console.WriteLine(ID);
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
                    Console.WriteLine(ID);
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
                    Console.WriteLine(output);
                    count++;
                }
                else
                {
                    Console.WriteLine(ID);
                    HasFinished = true;
                }
            }
        }
        
        static void Main(string[] args)
        {
            SineGeneratingAgent test = new SineGeneratingAgent(15);
            
            test.Run();
            
            Console.Read();
        }
        
        
    }

    
}
