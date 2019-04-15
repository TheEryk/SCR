using System;
using System.Threading;
namespace Lab_synch
{
    internal class AgentOdejmowanie : Agent
    {
        public AgentOdejmowanie() : base() { }
        public AgentOdejmowanie(float zmiana) : base(zmiana) { }
        public AgentOdejmowanie(Bank bank, float zmiana) : base(bank, zmiana) { }

        protected override void Update()
        {
            // symulacja dlugiej pracy na zmiennej
            stan = bank.StanKonta;
            Thread.Sleep(500);
            stan -= zmiana;
            bank.StanKonta = stan;
            Console.WriteLine("odejmowanie: " + bank.StanKonta);
        }
    }
}