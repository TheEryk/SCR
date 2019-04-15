using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_synch
{
    abstract class Agent : Start
    {
        public Bank bank;
        protected float zmiana;
        protected float stan;

        public Agent()
        {
            bank = new Bank();
            zmiana = 10;
        }

        public Agent(float zmiana)
        {
            bank = new Bank();
            this.zmiana = zmiana;
        }

        public Agent(Bank bank, float zmiana)
        {
            this.bank = bank;
            this.zmiana = zmiana;
        }
    }
}