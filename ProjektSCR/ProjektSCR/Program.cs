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
        private static bool Finished = false;
        private static List<int> ListaSpraw;
        private static List<Okienko> ListaOkienek;
        private static List<Matka> ListaMatek;
        private static SpinLock Biletomat = new SpinLock();
        static int numerek = 0;
        class Matka
        {
            public enum MozliwyStan { Kolejka, Poczekalnia, Okienko, Wyszla };
            public MozliwyStan stan;
            int ID;
            int Numer;
            public int Trudnosc;
            public bool MamSprawe;
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
            public void PostepWSprawie()
            {
                Trudnosc--;
                if(Trudnosc == 0)
                {
                    MamSprawe = false;
                }
            }
            public void Update()
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
                        if (MamSprawe)
                        {
                            Console.WriteLine("Matka " + ID + " jest przy okienku");
                        }
                        else
                        {
                            Awansuj();
                        }
                        break;

                    case MozliwyStan.Wyszla:
                        Console.WriteLine("Wyszłam, mam ID: " + ID + " | Zostałam obsłużona z numerem: " + Numer);
                        break;

                    default:
                        break;
                }
            }
            public void PelneInfo()
            {
                Console.WriteLine("Jestem matka z ID " + ID + " numer " + Numer + " i MamSprawe: " + MamSprawe);
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
                            Numer = numerek++;
                            ListaSpraw.Add(Numer);
                            Biletomat.Exit();
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                } while (!lockTaken);
                Console.WriteLine("Matka " + ID + " wziela numer " + Numer);
                Awansuj();
            }
            void Podejdz_Do_Okienka()
            {
                foreach(var okienko in ListaOkienek)
                {
                    if (okienko.ZdobadzWywolanyNumer() == Numer)
                    {
                        okienko.NowaMatka(ID);
                        Awansuj();
                    }
                }
            }
        }
        class Okienko
        {
            //Statyczne 
            int NumerOkienka;
            public enum Status { Pusty, Oczekuje, Zajety };

            //Status
            Status status = Status.Pusty;

            //Przed przyjeciem petenta
            public int WywolanyNumer;

            //Po przyjeciu klienta
            int AktualnyNumer;
            int AktualnaMatkaID;

            public int ZdobadzWywolanyNumer()
            {
                return WywolanyNumer;
            }
            public int ZdobadzAktualnyNumer()
            {
                return AktualnyNumer;
            }
            public int ZdobadzIDMatki()
            {
                return AktualnaMatkaID;
            }

            public Status ZwrocStatus()
            {
                return status;
            }
            public void ZwolnijOkienko()
            {
                status = Status.Pusty;
            }
            public void NowaMatka(int ID)
            {
                AktualnyNumer = WywolanyNumer;
                AktualnaMatkaID = ID;
                status++;
            }
            public void UstawNowyNumer(int NowyNumer)
            {
                WywolanyNumer = NowyNumer;
                status++;
            }
            public Okienko(int NumerOkienka)
            {
                this.NumerOkienka = NumerOkienka;
            }
        }
        class Urzednik
        {
            int ID;
            int czas_przerwy;
            Okienko okienko;
            public Urzednik(int iD, int przerwa, Okienko okienko)
            {
                ID = iD;
                czas_przerwy = przerwa;
                this.okienko = okienko;
            }
            public void Run()
            {
                while (!Finished)
                {
                    Update();
                    Thread.Sleep(400);
                }
            }
            public void Update()
            {
                switch (okienko.ZwrocStatus())
                {
                    case Okienko.Status.Pusty:
                        Console.WriteLine(okienko.ZwrocStatus() + " Przydzielam nowa sprawe");
                        NowaSprawa();
                        break;
                    case Okienko.Status.Oczekuje:
                        Console.WriteLine(okienko.ZwrocStatus() + " Oczekuje na klienta " + okienko.ZdobadzWywolanyNumer());
                        break;
                    case Okienko.Status.Zajety:
                        if (ListaMatek.ElementAt(okienko.ZdobadzAktualnyNumer()).MamSprawe)
                        {
                            ListaMatek.ElementAt(okienko.ZdobadzAktualnyNumer()).PostepWSprawie();
                            Console.WriteLine("Pracuje z klientem numer: " + okienko.ZdobadzAktualnyNumer() + " | O ID " + okienko.ZdobadzIDMatki() + " | Pozostało " + ListaMatek.ElementAt(okienko.ZdobadzAktualnyNumer()).Trudnosc);
                        }
                        else
                            okienko.ZwolnijOkienko();
                        break;
                    default:
                        break;
                }
                foreach (int zmienna in ListaSpraw)
                {
                    Console.Write(" " + zmienna + " ");
                }
                Console.Write("\n");
            }
            public void NowaSprawa()
            {
                if (ListaSpraw.Any())
                {
                    bool lockTaken = false;
                    do
                    {
                        try
                        {
                            Biletomat.TryEnter(ref lockTaken);
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                okienko.UstawNowyNumer(ListaSpraw.ElementAt(0));
                                ListaSpraw.RemoveAt(0);
                                Biletomat.Exit();
                            }
                            else
                            {
                            }
                        }
                    } while (!lockTaken);
                    Console.WriteLine("Ustawiono nowy numer " + okienko.WywolanyNumer);
                }
                else Console.WriteLine("Brak spraw");
            }
        }
        

        

        static void Main(string[] args)
        {
            
            ListaSpraw = new List<int>();
            ListaOkienek = new List<Okienko>();
            ListaMatek = new List<Matka>();
            List<Thread> watki = new List<Thread>();
            ListaOkienek.Add(new Okienko(1));
            Urzednik urzednik = new Urzednik(1, 1, ListaOkienek.Last());
            Thread watek = new Thread(urzednik.Run);
            watek.Start();
            Console.ReadLine();
            int test = 5;
            for(int i = 5; i <16; i++)
            {
                ListaMatek.Add(new Matka(test, i));
            }
            foreach(var matka in ListaMatek)
            {
                Thread thread = new Thread(matka.Run);
                watki.Add(thread);
                thread.Start();
            }
            foreach(Thread thread in watki)
            {
                thread.Join();
            }
            
            Console.ReadLine();
            for (int i = 12; i < 15; i++)
            {
                ListaMatek.Add(new Matka(test, i));
                Thread thread = new Thread(ListaMatek.Last().Run);
                watki.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in watki)
            {
                thread.Join();
            }
            Finished = true;
            foreach (var matka in ListaMatek)
            {
                matka.Update();
            }
            
            Console.ReadLine();
        }
    }
}
