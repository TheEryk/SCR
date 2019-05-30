﻿

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
        private static Mutex ListaOkienekDostep;
        private static Mutex ListaSprawDostep;

        static bool SimulationRunning;

        private struct Bilet
        {
            char Kolejka;
            int Numer;
            public Bilet(char Kolejka, int Numer)
            {
                this.Kolejka = Kolejka;
                this.Numer = Numer;
            }
            public int ZwrocNumer()
            {
                return Numer;
            }
            public char ZwrocKolejke()
            {
                return Kolejka;
            }
            public static bool operator ==(Bilet bilet, Bilet bilet2)
            {
                if (bilet.Kolejka == bilet2.Kolejka && bilet.Numer == bilet2.Numer)
                    return true;
                else
                    return false;
            }
            public static bool operator !=(Bilet bilet, Bilet bilet2)
            {
                return !(bilet == bilet2);
            }
            public override string ToString()
            {
                return Kolejka + Numer.ToString();
            }
        }
        static int OstatniNumerek;
        static List<Bilet> ListaSpraw;
        static List<Okienko> ListaOkienek;
        static List<Matka> ListaMatek;
        static List<Biletomat> ListaBiletomaotw;
        static List<Thread> ListaWatkow;
        static List<Urzednik> ListaUrzednikow;
        static Czas czas;
        class Czas
        {
            DateTime time;
            public Czas(DateTime datetime)
            {
                time = datetime;
                minuta = 1000;
            }
            int minuta;
            
            public void Run()
            {
                while(SimulationRunning)
                {
                    Update();
                    Thread.Sleep(minuta);
                }
            }
            public void Run(int okres)
            {
                for(int i=0; i<okres; i++)
                {
                    Update();
                    Thread.Sleep(minuta);
                }
            }
            ///<summary>
            ///Argument[int] określa długość symulowanej minuty w milisekundach.
            ///</summary>
            public void UstawMinute(int minuta)
            {
                this.minuta = minuta;
            }
            void Update()
            {
                time = time.AddMinutes(1);
            }
            ///<summary>
            ///Zwraca aktualny czas symulacji.
            ///</summary>
            public DateTime getTime()
            {
                return time;
            }
            /*  Przykładowe funkcje czasu
            Czas czas = new Czas(new DateTime(2019, 5, 30, 19, 14, 00));

            czas.Run(500);
            #if DEBUG
                    Console.WriteLine(time.ToString());
            #endif
            Console.WriteLine((czas.getTime()- new DateTime(2019, 5, 30, 19, 14, 00)).ToString());
            Console.ReadKey();
            */
        }
        static void Setup()
        {
            SimulationRunning = true;
            czas = new Czas(new DateTime(2019, 5, 30, 19, 14, 00));
            Thread zegarek = new Thread(czas.Run);
            zegarek.Start();
            ListaSpraw = new List<Bilet>();
            ListaOkienek = new List<Okienko>();
            ListaMatek = new List<Matka>();
            ListaBiletomaotw = new List<Biletomat>();
            ListaBiletomaotw.Add(new Biletomat(1, 500));
            ListaBiletomaotw.Add(new Biletomat(2, 500));
            ListaWatkow = new List<Thread>();
            ListaUrzednikow = new List<Urzednik>();
            ListaOkienekDostep = new Mutex();
            ListaSprawDostep = new Mutex();
        }
        ///<summary>
        ///Obiekt Matka.
        ///(int ID, Sprawa sprawa).
        ///</summary>
        class Matka
        {
            ///<summary>
            ///Mozliwe stay matki.
            ///</summary>
            public enum MozliwyStan { Kolejka, Poczekalnia, Okienko, Wyszla };
            public struct Sprawa
            {
                public enum Typ {Undefined, Finanse, Administracja };
                public Typ Typ_Sprawy;
                public int Trudnosc;
                public bool Rozwiazana;
                public Sprawa(int Trudnosc)
                {
                    Typ_Sprawy = Sprawa.Typ.Undefined;
                    this.Trudnosc = Trudnosc;
                    Rozwiazana = false;
                }
                public Sprawa(Sprawa.Typ typ, int Trudnosc)
                {
                    Typ_Sprawy = typ;
                    this.Trudnosc = Trudnosc;
                    Rozwiazana = false;
                }
            }
            ///<summary>
            ///Spis czasu w jakim zdarzyły sie wydarzenia kluczowe.
            ///</summary>
            struct SpisCzasu
            {
                public DateTime CzasPrzyjscia;
                public DateTime CzasOtrzymaniaNumerka;
                public DateTime CzasPodejsciaDoOkienka;
                public DateTime CzasRozwiazaniaSprawy;
            }
            int ID;
            MozliwyStan stan;
            public Sprawa sprawa;
            SpisCzasu spisczasu;
            Bilet bilet;
            public Matka()
            {
                stan = MozliwyStan.Kolejka;
                this.ID = 0;
                this.sprawa = new Sprawa(1);
                spisczasu.CzasPrzyjscia = czas.getTime();
#if DEBUG
                Console.WriteLine("Utworzona Matka " + ID + " o godzinie " + spisczasu.CzasPrzyjscia.ToString("T"));
#endif
            }
            public Matka(int ID)
            {
                stan = MozliwyStan.Kolejka;
                this.ID = ID;
                this.sprawa = new Sprawa(1);
                spisczasu.CzasPrzyjscia = czas.getTime();
#if DEBUG
                Console.WriteLine("Utworzona Matka " + ID + " o godzinie " + spisczasu.CzasPrzyjscia.ToString("T"));
#endif
            }
            public Matka(int ID, Sprawa sprawa)
            {
                stan = MozliwyStan.Kolejka;
                this.ID = ID;
                this.sprawa = sprawa;
                spisczasu.CzasPrzyjscia = czas.getTime();
#if DEBUG
                Console.WriteLine("Utworzona Matka " + ID + " o godzinie " + spisczasu.CzasPrzyjscia.ToString("T"));
#endif
            }
            public void Run()
            {
                while(stan != MozliwyStan.Wyszla)
                {
                    Update();
                    Thread.Sleep(2000);
                }
            }
            public void Update()
            {
                switch (stan)
                {
                    case MozliwyStan.Kolejka:
                        Pobierz_Bilet();
                        break;
                    case MozliwyStan.Poczekalnia:
                        Podejdz_Do_Okienka();
                        break;
                    case MozliwyStan.Okienko:
                        if (!sprawa.Rozwiazana)
                        {
                        }
                        else
                        {
                            Awansuj();
                        }
                        break;

                    case MozliwyStan.Wyszla:
#if DEBUG
                        Console.WriteLine("Wyszłam, mam ID: " + ID + " | Zostałam obsłużona z numerem: " + bilet);
                        Console.WriteLine("Timestamps: ");
                        Console.WriteLine(spisczasu.CzasPrzyjscia);
                        Thread.Sleep(10);
                        Console.WriteLine(spisczasu.CzasOtrzymaniaNumerka);
                        Thread.Sleep(10);
                        Console.WriteLine(spisczasu.CzasPodejsciaDoOkienka);
                        Thread.Sleep(10);
                        Console.WriteLine(spisczasu.CzasRozwiazaniaSprawy);
                        Thread.Sleep(10);
#endif
                        break;

                    default:
                        break;
                }
            }
            ///<summary>
            ///Wprowadza jednostkowy postęp w sprawie.
            ///</summary>
            public void PostepWSprawie()
            {
                sprawa.Trudnosc--;
                if (sprawa.Trudnosc == 0)
                {
                    sprawa.Rozwiazana = true;
                }
            }
            ///<summary>
            ///Wprowadza wiekszy postęp w sprawie.
            ///</summary>
            public void PostepWSprawie(int postep)
            {
                sprawa.Trudnosc = sprawa.Trudnosc - postep;
                if (sprawa.Trudnosc < 0)
                {
                    sprawa.Rozwiazana = true;
                }
            }
            public void PelneInfo()
            {
                Console.WriteLine("Jestem matka z ID " + ID + " w kolejce: " + bilet.ZwrocKolejke() + " z numerem: " + bilet.ZwrocNumer() + " i MamSprawe: " + !sprawa.Rozwiazana);
            }
            void Awansuj()
            {
                switch (stan)
                {
                    case MozliwyStan.Kolejka:
                        stan++;
                        spisczasu.CzasOtrzymaniaNumerka = czas.getTime();
                        break;
                    case MozliwyStan.Poczekalnia:
                        stan++;
                        spisczasu.CzasPodejsciaDoOkienka = czas.getTime();
                        break;
                    case MozliwyStan.Okienko:
                        stan++;
                        spisczasu.CzasRozwiazaniaSprawy = czas.getTime();
                        break;
                    default:
                        break;
                }
            }
            public MozliwyStan GdzieJest()
            {
                return stan;
            }
            void Pobierz_Bilet()
            {
                Console.WriteLine("Matka pobiera bilet " + ID);
                foreach (Biletomat biletomat in ListaBiletomaotw)
                {
                    if (stan == Matka.MozliwyStan.Kolejka)
                    {
                        if (biletomat.ZajmijBiletomat())
                        {
                            Console.WriteLine("Matka: " + ID + " zajela biletomat " + biletomat.ID);
                            bilet = biletomat.ZdobadzNumer(this);
                            PelneInfo();
                            Awansuj();
                        }
                    }
                }
            }
            void Podejdz_Do_Okienka()
            {
                foreach(var okienko in ListaOkienek)
                {
                    if (okienko.ZdobadzWywolanyBilet() == bilet)
                    {
                        okienko.Podejdz();
                        Awansuj();
                    }
                }
            }
        }
        class Okienko
        {
            //Statyczne 
            public int NumerOkienka;
            public enum Status { Pusty, Oczekuje, Zajety};

            //Status
            Status status;
            bool MaUrzednika;
            
            //Przed przyjeciem petenta
            public Bilet WywolanyBilet;

            public Okienko(int NumerOkienka)
            {
                this.NumerOkienka = NumerOkienka;
                MaUrzednika = false;
                status = Status.Pusty;
                Console.WriteLine("Utworzone okienko nr:" + NumerOkienka + " o godzinie " + czas.getTime().ToString("T"));
            }
            public void UrzednikPodszedl()
            {
                MaUrzednika = true;
            } 
            public void UrzednikOdszedl()
            {
                MaUrzednika = false;
            }
            public bool CzyJestUrzednik()
            {
                return MaUrzednika;
            }
            public Status ZwrocStatus()
            {
                return status;
            }
            public void ZwolnijOkienko()
            {
                status = Status.Pusty;
            }
            public Bilet ZdobadzWywolanyBilet()
            {
                return WywolanyBilet;
            }
            public void UstawWywolanyBilet(Bilet bilet)
            {
                WywolanyBilet = bilet;
            }
            public void Podejdz()
            {
                status++;
            }
        }
        class Urzednik
        {
            public enum Kompetencje { Ogólny, Administracja, Finanse};
            int ID;
            Kompetencje kompetencja;
            int czas_przerwy;
            Okienko okienko;

            public Urzednik(int iD, int przerwa)
            {
                ID = iD;
                czas_przerwy = przerwa;
                kompetencja = Kompetencje.Ogólny;
                Console.WriteLine("Utworzony urzednik nr:" + ID + " o godzinie " + czas.getTime().ToString("T"));
            }
            public void Run()
            {
                while (SimulationRunning)
                {
                    Update();
                    Thread.Sleep(3000);
                }
            }
            public void Update()
            {
                if (okienko != null)
                {
                    switch (okienko.ZwrocStatus())
                    {
                        case Okienko.Status.Pusty:
                            Console.WriteLine("Okienko nr:" + okienko.NumerOkienka + " jest " + okienko.ZwrocStatus() + " Przydzielam nowa sprawe, urzednik " + ID);
                            NowaSprawa();
                            break;
                        case Okienko.Status.Oczekuje:
                            Console.WriteLine(okienko.ZwrocStatus() + " Czekam na petenta");
                            break;
                        case Okienko.Status.Zajety:
                            if (ListaMatek.ElementAt(okienko.WywolanyBilet.ZwrocNumer()).GdzieJest()==Matka.MozliwyStan.Wyszla)
                            {
                                Console.WriteLine(okienko.ZwrocStatus() + " Zwalniam okienko");
                                okienko.ZwolnijOkienko();
                            }
                            else
                            {
                                ListaMatek.ElementAt(okienko.WywolanyBilet.ZwrocNumer()).PostepWSprawie();
                                Console.WriteLine(okienko.ZwrocStatus() + " Pracuje z matka");
                            }
                            break;
                        default:
                            break;
                    }
                }
                else ZnajdzOkienko();
            }
            void ZnajdzOkienko()
            {

                Console.WriteLine("Urzednik nr:" + ID + " Szukam okienka");
                ListaOkienekDostep.WaitOne();
                Thread.Sleep(500);
                foreach (var okienko in ListaOkienek)
                {
                    if (this.okienko == null && !okienko.CzyJestUrzednik())
                    {
                        this.okienko = okienko;
                        okienko.UrzednikPodszedl();
                        Console.WriteLine("Urzednik nr:" + ID +  " Znalazlem okienko: " + okienko.NumerOkienka);
                    }
                }
                ListaOkienekDostep.ReleaseMutex();
            }
            public void ZwolnijOkienko()
            {
                Console.WriteLine("Zwalniam okienko");
                okienko.UrzednikOdszedl();
                okienko = null;
            }
            public void NowaSprawa()
            {
                if (ListaSpraw.Any())
                {
                    switch (kompetencja)
                    {
                        case Kompetencje.Administracja:
                            break;
                        case Kompetencje.Finanse:
                            break;
                        default:
                            ListaSprawDostep.WaitOne();
                            okienko.UstawWywolanyBilet(ListaSpraw.ElementAt(0));
                            ListaSpraw.RemoveAt(0);
                            ListaSprawDostep.ReleaseMutex();
                            Console.WriteLine("Okienko nr:" + okienko.NumerOkienka + " jest " + okienko.ZwrocStatus() + " Przydzieliłem nowa sprawe "+ okienko.ZdobadzWywolanyBilet() + ", urzednik " + ID);
                            break;
                    }
                    okienko.Podejdz();
                }
                else Console.WriteLine("Okienko nr:" + okienko.NumerOkienka + " jest " + okienko.ZwrocStatus() +  ", urzednik " + ID + " Brak spraw");
            }
        }
        class Biletomat
        {
            public int ID;
            Mutex zajety;
            int Opoznienie;
            public Biletomat()
            {
                zajety = new Mutex();
                this.Opoznienie = 0;
                ID = 0;
            }
            public Biletomat(int ID, int Opoznienie)
            {
                zajety = new Mutex();
                this.ID = ID;
                this.Opoznienie = Opoznienie;
            }
            public bool ZajmijBiletomat()
            {
                return zajety.WaitOne(10);
            }
            public Bilet ZdobadzNumer(Matka matka)
            {
                Bilet bilet;
                Thread.Sleep(Opoznienie);
                ListaSprawDostep.WaitOne();
                switch (matka.sprawa.Typ_Sprawy)
                {
                    case Matka.Sprawa.Typ.Administracja:
                        bilet = new Bilet('A', OstatniNumerek);
                        ListaSpraw.Add(bilet);
                        break;
                    case Matka.Sprawa.Typ.Finanse:
                        bilet = new Bilet('F', OstatniNumerek);
                        ListaSpraw.Add(bilet);
                        break;
                    default:
                        bilet = new Bilet('U', OstatniNumerek);
                        ListaSpraw.Add(bilet);
                        break;
                }
                OstatniNumerek++;
                ListaMatek.Add(matka);
                ListaSprawDostep.ReleaseMutex();
                Thread.Sleep(Opoznienie);
                zajety.ReleaseMutex();
                return bilet;
            }
        }

        

        static void Main(string[] args)
        {

            Setup();
            ListaUrzednikow.Add(new Urzednik(1,1));
            ListaUrzednikow.Add(new Urzednik(2,1));
            foreach (Urzednik urzednik in ListaUrzednikow)
            {
                Thread thread = new Thread(urzednik.Run);
                thread.Start();
            }
            Console.ReadKey();
            ListaOkienek.Add(new Okienko(1));
            Console.ReadKey();
            for (int i = 0; i < 15; i++)
            {
                Matka matka = (new Matka(i));
                ListaWatkow.Add(new Thread(matka.Run));
            }
            foreach (Thread thread in ListaWatkow)
            {
                thread.Start();
            }
            Console.ReadKey();
            ListaOkienek.Add(new Okienko(2));
            foreach (Thread thread in ListaWatkow)
            {
                thread.Join();
            }
            Console.ReadKey();
            foreach (Matka matka in ListaMatek)
            {
                matka.Update();
            }
            Console.ReadKey();
            //Czas czas = new Czas(new DateTime(2019, 5, 30, 19, 14, 00));

            //czas.Run(500);

            //Console.WriteLine((czas.getTime() - new DateTime(2019, 5, 30, 19, 14, 00)).ToString());
            //Console.ReadKey();
        }
        /*{
            
            ListaSpraw = new List<int>();
            ListaOkienek = new List<Okienko>();
            ListaMatek = new List<Matka>();
            List<Thread> ListaWatkow = new List<Thread>();
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
                ListaWatkow.Add(thread);
                thread.Start();
            }
            foreach(Thread thread in ListaWatkow)
            {
                thread.Join();
            }
            
            Console.ReadLine();
            for (int i = 12; i < 15; i++)
            {
                ListaMatek.Add(new Matka(test, i));
                Thread thread = new Thread(ListaMatek.Last().Run);
                ListaWatkow.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in ListaWatkow)
            {
                thread.Join();
            }
            Finished = true;
            foreach (var matka in ListaMatek)
            {
                matka.Update();
            }
            
            Console.ReadLine();
        }*/
    }
}
