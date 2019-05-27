using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ProjektSCR
{
    class Program
    {
        private static List<Okienko> ListaOkienek;
        private static List<Matka> ListaMatek;
        private static SpinLock Biletomat = new SpinLock();
        static int numerek = 0;
        class Matka
        {
            public enum MozliwyStan { Kolejka, Poczekalnia, Okienko, Wyszla };
            MozliwyStan stan;
            int ID;
            int Numer;
            int Trudnosc;
            public bool MamSprawe;
            Okienko okienko;
            public Matka(int powaga, int iD)
            {
                stan = MozliwyStan.Kolejka;
                Trudnosc = powaga;
                ID = iD;
                MamSprawe = true;
                Console.WriteLine("Utworzona Matka " + ID);
            }
            public void Run()
            {
                while(stan != MozliwyStan.Wyszla)
                {
                    Update();
                    Thread.Sleep(1000);
                }
            }
            void Update()
            {
                switch (stan)
                {
                    case MozliwyStan.Kolejka:
                        Console.WriteLine("Matka " + ID + " jest w kolejce");
                        Pobierz_Bilet();
                        break;
                    case MozliwyStan.Poczekalnia:
                        Console.WriteLine("Matka " + ID + " jest w poczekalni z numerem " + Numer);
                        Podejdz_Do_Okienka();
                        break;
                    case MozliwyStan.Okienko:
                        if (!MamSprawe)
                        {
                            Awansuj();
                        }
                        break;
                    case MozliwyStan.Wyszla:
                        break;
                    default:
                        break;
                }
            }
            void Awansuj()
            {
                if (stan != MozliwyStan.Wyszla)
                {
                    stan++;
                }
            }
            MozliwyStan GdzieJest()
            {
                return stan;
            }
            void Pobierz_Bilet()
            {
                bool lockTaken = false;
                do
                {
                    try
                    {
                        Console.WriteLine("Matka " + ID + " probuje wziac numer");
                        Biletomat.TryEnter(ref lockTaken);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Numer = ++numerek;
                            Console.WriteLine("Matka " + ID + " wziela numer " + Numer);
                            Awansuj();
                            Biletomat.Exit();
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                } while (!lockTaken);
            }
            void Podejdz_Do_Okienka()
            {
                foreach(var okienko in ListaOkienek)
                {
                    if (okienko.JakiNumer() == Numer)
                    {
                        okienko.NowaMatka(ID);
                        Awansuj();
                    }
                }
            }
            public void Rozwiazana_Sprawa()
            {
                MamSprawe = false;
            }
        }

        class Okienko
        {
            Urzednik urzednik;
            int NumerOkienka;
            int WywolanyNumer;
            int AktualnyNumer;
            int AktualnaMatkaID;
            public int JakiNumer()
            {
                return AktualnyNumer;
            }
            public void NowaMatka(int ID)
            {
                AktualnaMatkaID = ID;
            }
            public Okienko(Urzednik urzednik, int NumerOkienka)
            {
                this.urzednik = urzednik;
                this.NumerOkienka = NumerOkienka;
                AktualnyNumer = 0;
            }
            public void Run()
            {
                while (true)
                {
                    Update();
                }
            }
            void Update()
            {
                if (ListaMatek.ElementAt(AktualnaMatkaID).MamSprawe)
                {
                    //Funkcja sprawdzajaca czy urzednik rozwiazal sprawe
                }
                else
                {
                    AktualnyNumer++;
                }
            }
        }
        class Urzednik
        {
            public enum MozliwyStan { Pracuje, Przerwa };
            public int ID;
            int czas_przerwy;
            MozliwyStan stan;
            public Urzednik(int iD, int przerwa)
            {
                stan = MozliwyStan.Pracuje;
                ID = iD;
                czas_przerwy = przerwa;
            }
            public void Przerwa()
            {
                stan = MozliwyStan.Przerwa;
                Thread.Sleep(czas_przerwy);
                stan = MozliwyStan.Pracuje;
            }
            public void Update()
            {

            }
        }
        

        

        static void Main(string[] args)
        {
            ListaOkienek = new List<Okienko>();
            ListaMatek = new List<Matka>();
            Urzednik urzednik1 = new Urzednik(1, 10);
            ListaOkienek.Add(new Okienko(urzednik1, 1));
            foreach(var okienko in ListaOkienek)
            {
                Thread threadOkienko = new Thread(okienko.Run);
            }
            int test = 0;
            for(int i = 0; i <10; i++)
            {
                ListaMatek.Add(new Matka(test, i));
            }
            List<Thread> watki = new List<Thread>();
            foreach(var matka in ListaMatek)
            {
                Thread thread = new Thread(matka.Run);
                watki.Add(thread);
                thread.Start();
            }
            Console.ReadLine();

        }
    }
}
