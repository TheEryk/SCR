using System;
using System.Threading;
namespace Lab_synch
{
    internal class AgentZmniejszOProcent : Agent
    {
        public AgentZmniejszOProcent() : base() { }
        public AgentZmniejszOProcent(float zmiana) : base(zmiana) { }
        public AgentZmniejszOProcent(Bank bank, float zmiana) : base(bank, zmiana) { }

        protected override void Update()
        {
            // symulacja dlugiej pracy na zmiennej
            stan = bank.StanKonta;
            Thread.Sleep(500);
            stan = stan * (100 - zmiana) / 100;
            bank.StanKonta = stan;
            Console.WriteLine("zmniejszanie o procent: " + bank.StanKonta);
        }
    }
}