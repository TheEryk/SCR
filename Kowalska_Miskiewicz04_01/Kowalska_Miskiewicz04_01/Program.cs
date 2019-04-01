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
                    Update();
                }
            }
        }

        class Bank : Agent
        {
            public int stanKonta;
            public void setStanKonta(int newStanKonta)
            {
                stanKonta = newStanKonta;
            }
            public Bank(int iD) : base(iD)
            {
                stanKonta = 0;
            }
            public override void Update()
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
            int tempStanKonta;
            Bank tempBank;
            public KasjerAd(int iD, Bank bank) : base(iD, bank)
            {
                tempBank = bank;
            }
            public override void Update()
            {
                tempStanKonta = tempBank.stanKonta;
                tempStanKonta += 100;
                Thread.Sleep(100);
                tempBank.setStanKonta(tempStanKonta);
            }
        }

        class KasjerSub : Kasjer
        {
            int tempStanKonta;
            Bank tempBank;
            public KasjerSub(int iD, Bank bank) : base(iD, bank)
            {
                tempBank = bank;
            }
            public override void Update()
            {
                tempStanKonta = tempBank.stanKonta;
                tempStanKonta -= 100;
                Thread.Sleep(100);
                tempBank.setStanKonta(tempStanKonta);
            }
        }


        static void Main(string[] args)
        {
            Bank bank = new Bank(1);
            List<Kasjer> kasjerzy = new List<Kasjer>();
            for(int i=0; i<10; i++)
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
                watki.Add(new Thread(kasjer.Run));
            }
            bank.Run();
            //Thread thread1 = new Thread(kasa1.Run);
            //bank.Run();
        }
    }
}
