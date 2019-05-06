using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Kowalska_Miskiewicz04_01
{
    class Program
    {
        private static int millisecondsTimeout = 100;
        private static Mutex mut = new Mutex();
        static private readonly object x = new object();
        private static SpinLock sl = new SpinLock();
        private static int usingResource = 0;
        abstract class Agent
        {
            public int ID;
            public bool HasFinished;
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
                    bool lockTaken = false;
                    do
                    {
                        try
                        {
                            Console.WriteLine("Proba wejscia");
                            sl.TryEnter(ref lockTaken);
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                Update();
                                sl.Exit();
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                    } while (!lockTaken);
                    Thread.Sleep(500);
                    //Mutex

                    /*
                    mut.WaitOne();
                    Update();
                    mut.ReleaseMutex();
                    */

                    //Lock

                    /*
                    lock (x)
                    {
                        Update();
                    }
                    */

                    //SpinLock co wywołanie

                    /*
                    bool lockTaken = false;
                    try
                    {
                        sl.TryEnter(ref lockTaken);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Update();
                            sl.Exit();
                        }
                    }
                   
                    //SpinLock na zawsze

                    /*
                    bool lockTaken = false;
                    try
                    {
                        sl.TryEnter(-1, ref lockTaken);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Update();
                            sl.Exit();
                        }
                    }
                    */

                    //Atomic Ops

                    /*
                    if (0 == Interlocked.Exchange(ref usingResource, 1))
                    {
                        Update();
                        Interlocked.Exchange(ref usingResource, 0);
                    }
                    */

                }
            }
        }

        class Bank
        {
            public int stanKonta;
            public void setStanKonta(int newStanKonta)
            {
                stanKonta = newStanKonta;
            }
            public Bank(int iD) 
            {
                stanKonta = 0;
            }
            public void Run()
            {
                while (true)
                {
                    Update();
                }
            }
            public void Update()
            {
                Console.WriteLine("Stan konta: " + stanKonta);
                Thread.Sleep(2000);
            }
            

        }

        abstract class Kasjer : Agent
        {
            public Kasjer(int iD, Bank bank) : base(iD)
            {
                
            }
        }
        
        class KasjerAd : Kasjer
        {
            int JD;
            int tempStanKonta;
            Bank tempBank;
            public KasjerAd(int iD, Bank bank) : base(iD, bank)
            {
                Console.WriteLine("Utworzony AgentDodajacy, ID:" + iD);
                tempBank = bank;
                JD = iD;
            }
            public override void Update()
            {
                tempStanKonta = tempBank.stanKonta;
                tempStanKonta += 200;
                Console.WriteLine("Czekam 10000ms :" + JD);
                Thread.Sleep(500);
                tempBank.setStanKonta(tempStanKonta);
                Console.WriteLine("Dodałem 200, moj ID:" + JD + " stan banku:" + tempBank.stanKonta);
                
            }
        }

        class KasjerSub : Kasjer
        {
            int JD;
            int tempStanKonta;
            Bank tempBank;
            public KasjerSub(int iD, Bank bank) : base(iD, bank)
            {
                Console.WriteLine("Utworzony AgentOdejmujacy, ID:" + iD);
                tempBank = bank;
                JD = iD;
            }
            public override void Update()
            {

                tempStanKonta = tempBank.stanKonta;
                tempStanKonta -= 100;
                Console.WriteLine("Czekam 500ms :" + JD);
                Thread.Sleep(500);
                tempBank.setStanKonta(tempStanKonta);
                Console.WriteLine("Zabrałem 100, moj ID:" + JD + " stan banku:" + tempBank.stanKonta);
                
            }
        }


        static void Main(string[] args)
        {
            Bank bank = new Bank(1);
            List<Kasjer> kasjerzy = new List<Kasjer>();
            for(int i=0; i<2; i++)
            {
                if (i % 2 == 0)
                {
                    kasjerzy.Add(new KasjerAd(i, bank));
                }
                else
                {
                    kasjerzy.Add(new KasjerSub(i, bank));
                }
            }
            List<Thread> watki = new List<Thread>();
            foreach (var kasjer in kasjerzy)
            {
                Thread thread = new Thread(kasjer.Run);
                watki.Add(thread);
                thread.Start();
                
            }
            
            Thread thread2 = new Thread(bank.Run);
            thread2.Start();
        }
    }
}
