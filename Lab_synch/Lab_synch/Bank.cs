using System;
using System.Threading;

namespace Lab_synch
{
    class Bank : Start
    {
        public float StanKonta { set; get; }
        public String Nazwa { set; get; }

        public Bank()
        {
            StanKonta = 100;
            Nazwa = "PKO";
        }

        public Bank(string N, float S)
        {
            StanKonta = S;
            Nazwa = N;
        }

        protected override void Update()
        {
            Console.WriteLine(Nazwa + ": " + StanKonta);
        }

        public void Pokaz()
        {
            Console.WriteLine(Nazwa + ": " + StanKonta);
            Thread.Sleep(2000);
        }

    }
}