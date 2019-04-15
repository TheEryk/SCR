using System;
using System.Threading;
namespace Lab_synch
{
    internal class AgentDodawanie : Agent
    {
        public AgentDodawanie() : base() { }
        public AgentDodawanie(float zmiana): base(zmiana) { }
        public AgentDodawanie(Bank bank, float zmiana) : base(bank, zmiana) { }

        protected override void Update()
        {
            // symulacja dlugiej pracy na zmiennej
            stan = bank.StanKonta;
            Thread.Sleep(500);
            stan += zmiana;
            bank.StanKonta = stan;
            Console.WriteLine("dodawanie: " + bank.StanKonta);
        }
    }
}