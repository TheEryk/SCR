

using System;
using System.Collections;
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
        static List<Biletomat> ListaBiletomatow;
        static List<Thread> ListaWatkow;
        static List<Urzednik> ListaUrzednikow;
        static Czas czas;
        static Matka.Sprawa defaultSprawa = new Matka.Sprawa(Matka.Sprawa.Typ.Undefined, 5);
        static void Setup()
        {
            SimulationRunning = true;
            czas = new Czas(new DateTime(2019, 5, 30, 19, 14, 00));
            Thread zegarek = new Thread(czas.Run);
            zegarek.Start();
            ListaSpraw = new List<Bilet>();
            ListaOkienek = new List<Okienko>();
            ListaMatek = new List<Matka>();
            ListaBiletomatow = new List<Biletomat>();
            ListaBiletomatow.Add(new Biletomat(1, 1500));
            ListaWatkow = new List<Thread>();
            ListaUrzednikow = new List<Urzednik>();
            ListaOkienekDostep = new Mutex();
            ListaSprawDostep = new Mutex();
        }
        /// <summary>
        /// Funkcja generuje Matke przyjmuje:
        /// Name(String/Int)
        /// Sprawa(Matka.Sprawa)
        /// Trudnosc(Int)
        /// Typ(Matka.Sprawa.Typ)
        /// </summary>
        /// <param name="args"></param>
        private static void GenerujMatke(params object[] args)
        {
            bool znajdzInt(object p)
            {
                return p is int;
            }
            bool znajdzString(object p)
            {
                return p is string;
            }
            bool znajdzTyp(object p)
            {
                return p is Matka.Sprawa.Typ;
            }
            bool znajdzSprawe(object p)
            {
                return p is Matka.Sprawa;
            }
            string Name;
            Matka.Sprawa sprawa;
            if (Array.Exists(args, znajdzString))
            {
                Name = (string)Array.Find(args, znajdzString);
            }
            else if (Array.Exists(args, znajdzInt))
            {
                int index = Array.FindIndex(args, znajdzInt);
                Name = ((int)args[index]).ToString();
                args = args.Where((val, idx) => idx != index).ToArray();
            }
            else Name = "Undefined";
            if (Array.Exists(args, znajdzSprawe))
            {
                sprawa = (Matka.Sprawa)Array.Find(args, znajdzSprawe);
            }
            else
            {
                Matka.Sprawa.Typ typ;
                int Trudnosc;
                if (Array.Exists(args, znajdzTyp))
                {
                    typ = (Matka.Sprawa.Typ)Array.Find(args, znajdzTyp);
                }
                else typ = Matka.Sprawa.Typ.Undefined;
                if (Array.Exists(args, znajdzInt))
                {
                    Trudnosc = (int)Array.Find(args, znajdzInt);
                }
                else Trudnosc = 10;
                sprawa = new Matka.Sprawa(typ, Trudnosc);
            }
            ListaWatkow.Add(new Thread(new Matka(Name, sprawa).Run));
            ListaWatkow.Last().Start();
        }
        private static void GenerujMatki(int j)
        {
            int PierwszyWatek = ListaWatkow.Count;
            for (int i = 0; i < j; i++)
            {
                ListaWatkow.Add(new Thread(new Matka((PierwszyWatek + i).ToString(), new Matka.Sprawa(Matka.Sprawa.Typ.Undefined, 5)).Run));
            }
            for (int i = 0; i < j; i++)
            {
                ListaWatkow.ElementAt(PierwszyWatek + i).Start();
            }
        }
        private static void GenerujOkienko(int Numer)
        {
            ListaOkienekDostep.WaitOne();
            ListaOkienek.Add(new Okienko(Numer));
            ListaOkienekDostep.ReleaseMutex();
        }
        /// <summary>
        /// Funkcja generuje Urzednika przyjmuje:
        /// ID(Int)
        /// Kompetencje(Urzednik.Kompetencje)
        /// Czas_Przerwy(Int)
        /// Wydajnosc(Int)
        /// </summary>
        /// <param name="args"></param>
        private static void GenerujUrzednika(params object[] args)
        {
            bool znajdzInt(object p)
            {
                return p is int;
            }
            bool znajdzKompetencje(object p)
            {
                return p is Urzednik.Kompetencje;
            }
            int iD;
            Urzednik.Kompetencje kompetencja;
            int przerwa;
            int Wydajnosc;
            if (Array.Exists(args, znajdzInt))
            {
                int index = Array.FindIndex(args, znajdzInt);
                iD = (int)args[index];
                args = args.Where((val, idx) => idx != index).ToArray();
            }
            else iD = -1;
            if (Array.Exists(args, znajdzKompetencje))
            {
                kompetencja = (Urzednik.Kompetencje)Array.Find(args, znajdzKompetencje);
            }
            else kompetencja = Urzednik.Kompetencje.Ogólny;
            if (Array.Exists(args, znajdzInt))
            {
                int index = Array.FindIndex(args, znajdzInt);
                przerwa = (int)args[index];
                args = args.Where((val, idx) => idx != index).ToArray();
            }
            else przerwa = 0;
            if (Array.Exists(args, znajdzInt))
            {
                int index = Array.FindIndex(args, znajdzInt);
                Wydajnosc = (int)args[index];
                args = args.Where((val, idx) => idx != index).ToArray();
            }
            else Wydajnosc = 1;
            ListaUrzednikow.Add(new Urzednik(iD, kompetencja, przerwa, Wydajnosc));
            new Thread(ListaUrzednikow.Last().Run).Start();
        }
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
        class Matka
        {
            ///<summary>
            ///Mozliwe stay matki.
            ///</summary>
            public enum MozliwyStan { Kolejka, Poczekalnia, Okienko, Wyszla };
            public struct Sprawa
            {
                public enum Typ { Undefined, Finanse, Administracja };
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
            string Name;
            MozliwyStan stan;
            Sprawa sprawa;
            SpisCzasu spisczasu;
            Bilet bilet;
            /// <summary>
            /// Konstruktor matki
            /// </summary>
            /// <param name="Name"></param>
            /// <param name="sprawa"></param>
            public Matka(string Name, Sprawa sprawa)
            {
                stan = MozliwyStan.Kolejka;
                this.Name = Name;
                this.sprawa = sprawa;
                spisczasu.CzasPrzyjscia = czas.getTime();
#if DEBUG
                Console.WriteLine("Utworzona Matka " + this.Name + " o godzinie " + spisczasu.CzasPrzyjscia.ToString("T") + " ze sprawa " + this.sprawa.Typ_Sprawy + " o trudnosci " + this.sprawa.Trudnosc);
#endif
            }
            public void Run()
            {
                while (stan != MozliwyStan.Wyszla)
                {
                    Update();
                    Thread.Sleep(1000);
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
                            Console.WriteLine("Matka " + Name + " ma rozwiazana sprawe");
                            Awansuj();
                        }
                        break;

                    case MozliwyStan.Wyszla:
#if DEBUG
                        Console.WriteLine("Wyszłam, mam ID: " + Name + " | Zostałam obsłużona z numerem: " + bilet);
                        Console.WriteLine("Timestamps: ");
                        Console.WriteLine("Przyszlam o " + spisczasu.CzasPrzyjscia);
                        Thread.Sleep(10);
                        Console.WriteLine("Otrzymalam numerek o " + spisczasu.CzasOtrzymaniaNumerka);
                        Thread.Sleep(10);
                        Console.WriteLine("Podeszlam do okienka o " + spisczasu.CzasPodejsciaDoOkienka);
                        Thread.Sleep(10);
                        Console.WriteLine("Rozwiazalam sprawe o " + spisczasu.CzasRozwiazaniaSprawy);
                        Thread.Sleep(10);
#endif
                        break;

                    default:
                        break;
                }
            }
            ///<summary>
            ///Wprowadza wiekszy postęp w sprawie.
            ///</summary>
            public void PostepWSprawie(int postep = 1)
            {
                sprawa.Trudnosc = sprawa.Trudnosc - postep;
                if (sprawa.Trudnosc <= 0)
                {
                    sprawa.Rozwiazana = true;
                }
                else
                    Console.WriteLine("Matka " + Name + " ma jeszcze " + sprawa.Trudnosc + " sprawy do rozwiazania");
            }
            public Matka.Sprawa.Typ ZwrocTyp()
            {
                return sprawa.Typ_Sprawy;
            }
            
            public MozliwyStan GdzieJest()
            {
                return stan;
            }
            void Pobierz_Bilet()
            {
                int BiletomatIndex = ListaBiletomatow.FindIndex(x => x.ZajmijBiletomat());
                if(BiletomatIndex != -1)
                {
                    bilet = ListaBiletomatow.ElementAt(BiletomatIndex).ZdobadzNumer(this);
                    Console.WriteLine("Matka " + Name + " zdobyla bilet " + bilet + " w biletomacie " + ListaBiletomatow.ElementAt(BiletomatIndex).ID);
                    Awansuj();
                }
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
            void Podejdz_Do_Okienka()
            {
                int index = ListaOkienek.FindIndex(x => x.ZdobadzWywolanyBilet() == bilet);
                if (index != -1)
                {
                    ListaOkienek.ElementAt(index).Podejdz();
                    Console.WriteLine("Matka " + Name + " podeszla do okienka " + ListaOkienek.ElementAt(index).NumerOkienka + " z biletem " + bilet);
                    Awansuj();
                }
            }
        }
        class Okienko
        {
            //Statyczne 
            public enum Status { Pusty, Oczekuje, Zajety};
            public int NumerOkienka;

            //Status
            Status status;
            bool MaUrzednika;
            
            //Przed przyjeciem petenta
            public Bilet WywolanyBilet;
            int Timeout;

            public Okienko(int NumerOkienka)
            {
                this.NumerOkienka = NumerOkienka;
                MaUrzednika = false;
                ZwolnijOkienko();
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
                Timeout = 10;
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
            public bool Oczekuj()
            {
                Timeout--;
                Console.WriteLine("Okienko numer: " + NumerOkienka + " zmieni za: " + Timeout);
                if (Timeout == 0)
                    return true;
                else
                    return false;
            }
        }
        class Urzednik
        {
            public enum Kompetencje { Ogólny, Administracja, Finanse};
            int ID;
            Kompetencje kompetencja;
            int czas_przerwy;
            int Wydajnosc;
            Okienko okienko;
            DateTime ostatniaPrzerwa;
            /// <summary>
            /// Konstruktor urzednika
            /// </summary>
            /// <param name="iD"></param>
            /// <param name="kompetencja"></param>
            /// <param name="przerwa"></param>
            public Urzednik(int iD, Kompetencje kompetencja, int przerwa, int Wydajnosc)
            {
                ID = iD;
                czas_przerwy = przerwa;
                this.kompetencja = kompetencja;
                this.Wydajnosc = Wydajnosc;
                Console.WriteLine("Utworzony urzednik nr:" + ID + " o godzinie " + czas.getTime().ToString("T") + " posiada kompetencje " + this.kompetencja + " i robi przerwe na " + czas_przerwy);
            }
            public void Run()
            {
                while (SimulationRunning)
                {
                    Update();
                    Thread.Sleep(500);
                }
            }
            public void Update()
            {
                if (okienko != null)
                {
                    switch (okienko.ZwrocStatus())
                    {
                        case Okienko.Status.Pusty:
                            if (SprawdzPrzerwe(60))
                            {
                                NowaSprawa();
                            }
                            break;
                        case Okienko.Status.Oczekuje:
                            if (okienko.Oczekuj())
                            {
                                okienko.ZwolnijOkienko();
                                NowaSprawa();
                            }
                            break;
                        case Okienko.Status.Zajety:
                            if (ListaMatek.ElementAt(okienko.WywolanyBilet.ZwrocNumer()).GdzieJest()==Matka.MozliwyStan.Wyszla)
                            {
                                okienko.ZwolnijOkienko();
                            }
                            else
                            {
                                ListaMatek.ElementAt(okienko.WywolanyBilet.ZwrocNumer()).PostepWSprawie(Wydajnosc);
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
                ListaOkienekDostep.WaitOne();
                Thread.Sleep(500);
                foreach (var okienko in ListaOkienek)
                {
                    if (this.okienko == null && !okienko.CzyJestUrzednik())
                    {
                        this.okienko = okienko;
                        okienko.UrzednikPodszedl();
                        Console.WriteLine("Urzednik nr:" + ID +  " znalazlem okienko: " + okienko.NumerOkienka);
                    }
                }
                ostatniaPrzerwa = czas.getTime();
                ListaOkienekDostep.ReleaseMutex();
            }
            bool SprawdzPrzerwe(int okres)
            {
                if(czas_przerwy != 0)
                {
                    TimeSpan span = czas.getTime().Subtract(ostatniaPrzerwa);
                    if (span.TotalMinutes > okres)
                    {
                        ZwolnijOkienko();
                        Thread.Sleep(czas_przerwy * 1000);
                        return false;
                    }
                }
                return true;
            }
            void ZwolnijOkienko()
            {
                Console.WriteLine("Urzednik o ID "+ ID + " idzie na przerwe o " + czas.getTime());
                okienko.UrzednikOdszedl();
                okienko = null;
            }
            void NowaSprawa()
            {
                ListaSprawDostep.WaitOne();
                int index;
                switch (kompetencja)
                {
                    case Kompetencje.Administracja:
                        index = ListaSpraw.FindIndex(x => x.ZwrocKolejke() == 'A');
                        if (index != -1)
                        {
                            okienko.UstawWywolanyBilet(ListaSpraw.ElementAt(index));
                            ListaSpraw.RemoveAt(index);
                            okienko.Podejdz();
                            Console.WriteLine("Urzednik nr "+ID+": Przydzielam sprawe nr " + okienko.ZdobadzWywolanyBilet() + " w okienku " + okienko.NumerOkienka);

                        }
                        else
                        {
                            SprawdzPrzerwe(15);
                        }
                        break;
                    case Kompetencje.Finanse:
                        index = ListaSpraw.FindIndex(x => x.ZwrocKolejke() == 'F');
                        if (index != -1)
                        {
                            okienko.UstawWywolanyBilet(ListaSpraw.ElementAt(index));
                            ListaSpraw.RemoveAt(index);
                            okienko.Podejdz();
                            Console.WriteLine("Urzednik nr " + ID + ": Przydzielam sprawe nr " + okienko.ZdobadzWywolanyBilet() + " w okienku " + okienko.NumerOkienka);

                        }
                        else
                        {
                            SprawdzPrzerwe(15);
                        }
                            break;
                    default:
                        index = ListaSpraw.FindIndex(x => true);
                        if (index != -1)
                        {
                            okienko.UstawWywolanyBilet(ListaSpraw.ElementAt(index));
                            ListaSpraw.RemoveAt(index);
                            okienko.Podejdz();
                            Console.WriteLine("Urzednik nr " + ID + ": Przydzielam sprawe nr " + okienko.ZdobadzWywolanyBilet() + " w okienku " + okienko.NumerOkienka);

                        }
                        else
                        {
                            SprawdzPrzerwe(15);
                        }
                            break;
                }
                ListaSprawDostep.ReleaseMutex();

            }
        }
        class Biletomat
        {
            public int ID;
            Mutex zajety;
            int Opoznienie;
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
                switch (matka.ZwrocTyp())
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
            GenerujOkienko(1);
            GenerujOkienko(2);
            GenerujUrzednika(1, 0, 2);
            GenerujUrzednika(2, Urzednik.Kompetencje.Finanse);
            GenerujMatke("Anna", new Matka.Sprawa(Matka.Sprawa.Typ.Administracja, 5));
            Thread.Sleep(1000);
            GenerujMatke("Beata", new Matka.Sprawa(Matka.Sprawa.Typ.Finanse, 6));
            Thread.Sleep(1000);
            GenerujMatke("Celina", 15);
            foreach (Thread thread in ListaWatkow)
            {
                thread.Join();
            }
            foreach (Matka matkam in ListaMatek)
            {
                matkam.Update();
            }
            SimulationRunning = false;

            Console.ReadKey();
        }
        
    }
}
