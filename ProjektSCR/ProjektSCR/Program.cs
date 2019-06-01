

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
        private static bool SimulationRunning;
        /// <summary>
        /// Okresla podstawe czasowa symulacji (1 -> minuta trwa 1ms)
        /// </summary>
        private static int TimeBase;
        /// <summary>
        /// Godzina rozpoczecia symulacji
        /// </summary>
        private static int TimeDayStart;
        /// <summary>
        /// Godzina rozpoczecia pracy
        /// </summary>
        private static int TimeWorkStart;
        /// <summary>
        /// Godzina zakonczenia pracy
        /// </summary>
        private static int TimeWorkEnd;
        /// <summary>
        /// Godzina zakonczenia symulacji
        /// </summary>
        private static int TimeDayEnd;

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
        private static int OstatniNumerek;
        private static List<Bilet> ListaSpraw;
        private static List<Okienko> ListaOkienek;
        private static List<Matka> ListaMatek;
        private static List<Biletomat> ListaBiletomatow;
        private static List<Thread> ListaWatkow;
        private static List<Urzednik> ListaUrzednikow;
        private static Czas czas;
        /// <summary>
        /// Funkcja inicjalizująca środowisko
        /// </summary>
        private static void Setup()
        {
            SimulationRunning = true;
            czas = new Czas(new DateTime(2019, 5, 30, TimeDayStart, 00, 00));
            Thread zegarek = new Thread(czas.Run);
            zegarek.Start();
            ListaSpraw = new List<Bilet>();
            ListaOkienek = new List<Okienko>();
            ListaMatek = new List<Matka>();
            ListaBiletomatow = new List<Biletomat>();
            ListaWatkow = new List<Thread>();
            ListaUrzednikow = new List<Urzednik>();
            ListaOkienekDostep = new Mutex();
            ListaSprawDostep = new Mutex();
        }
        /// <summary>
        /// Funkcja ustalająca kluczowe godziny
        /// </summary>
        private static void SetupTime(int timeBase, int timeDayStart, int timeDayEnd, int timeWorkStart, int timeWorkEnd)
        {
            TimeBase = timeBase;
            TimeDayStart = timeDayStart;
            TimeDayEnd = timeDayEnd;
            TimeWorkStart = timeWorkStart;
            TimeWorkEnd = timeWorkEnd;
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
            else
            {
                Name = ListaWatkow.Count.ToString();
            }
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
        /// <summary>
        /// Funkcja generuje j matek o parametrach domyślnych
        /// </summary>
        /// <param name="j"></param>
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
        private static void GenerujOkienko()
        {
            ListaOkienekDostep.WaitOne();
            ListaOkienek.Add(new Okienko(ListaOkienek.Count));
            ListaOkienekDostep.ReleaseMutex();
        }
        /// <summary>
        /// Generuje biletomat z opóźnieniem w minutach
        /// </summary>
        /// <param name="opoznienie"></param>
        private static void GenerujBiletomat(int opoznienie)
        {
            ListaBiletomatow.Add(new Biletomat(ListaBiletomatow.Count, opoznienie));
        }
        class Czas
        {
            DateTime time;
            public Czas(DateTime datetime)
            {
                time = datetime;
                minuta = TimeBase;
            }
            int minuta;
            
            public void Run()
            {
                while (czas.getTime().Hour < TimeDayEnd) 
                {
                    Update();
                    Thread.Sleep(minuta);
                    if (getTime().Hour >= TimeWorkEnd)
                        SimulationRunning = false;
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
                if (time.Hour != time.AddMinutes(1).Hour)
                    Console.WriteLine(time.AddMinutes(1));
                time = time.AddMinutes(1);
            }
            ///<summary>
            ///Zwraca aktualny czas symulacji.
            ///</summary>
            public DateTime getTime()
            {
                return time;
            }
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
                Console.WriteLine("Utworzona Matka " + this.Name + " o godzinie " + spisczasu.CzasPrzyjscia.ToString("T") + " ze sprawa " + this.sprawa.Typ_Sprawy + " o trudnosci " + this.sprawa.Trudnosc);
            }
            public void Run()
            {
                while (stan != MozliwyStan.Wyszla)
                {
                    Update();
                    Thread.Sleep(TimeBase);
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
                        Console.WriteLine("Wyszłam, mam ID: " + Name + " | Zostałam obsłużona z numerem: " + bilet);
                        Console.WriteLine("Timestamps: ");
                        Console.WriteLine("Przyszlam o " + spisczasu.CzasPrzyjscia);
                        if (spisczasu.CzasOtrzymaniaNumerka.Year != 1)
                        {
                            Console.WriteLine("Otrzymalam numerek o " + spisczasu.CzasOtrzymaniaNumerka);
                            if (spisczasu.CzasPodejsciaDoOkienka.Year != 1)
                            {
                                Console.WriteLine("Podeszłam do okienka o " + spisczasu.CzasPodejsciaDoOkienka);
                                if (spisczasu.CzasRozwiazaniaSprawy.Year != 1)
                                {
                                    Console.WriteLine("Rozwiązałam sprawe o " + spisczasu.CzasRozwiazaniaSprawy);
                                }
                                else Console.WriteLine("Nie rozwiązałam sprawy");
                            }
                            else Console.WriteLine("Nie podeszłam do okienka");
                        }
                        else Console.WriteLine("Nie otrzymałam numerka");
                        break;

                    default:
                        break;
                }
                if (!SimulationRunning) {
                    if (stan == MozliwyStan.Kolejka)
                    {
                        ListaSprawDostep.WaitOne();
                        ListaMatek.Add(this);
                        ListaSprawDostep.ReleaseMutex();
                        stan = MozliwyStan.Wyszla;
                    }
                    else if (stan == MozliwyStan.Poczekalnia)
                        stan = MozliwyStan.Wyszla;
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
            /// <summary>
            /// Czas przerwy w minutach
            /// </summary>
            int czas_przerwy;
            int Wydajnosc;
            bool KoniecPracy;
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
                KoniecPracy = false;
                ID = iD;
                czas_przerwy = przerwa;
                this.kompetencja = kompetencja;
                this.Wydajnosc = Wydajnosc;
                Console.WriteLine("Utworzony urzednik nr:" + ID + " o godzinie " + czas.getTime().ToString("T") + " posiada kompetencje " + this.kompetencja + " i robi przerwe na " + czas_przerwy);
            }
            public void Run()
            {
                while (!KoniecPracy)
                {
                    Update();
                    Thread.Sleep(2*TimeBase);
                }
            }
            public void Update()
            {
                if (okienko == null)
                {
                    if (SimulationRunning)
                    {
                        ZnajdzOkienko();
                    }
                    else KoniecPracy = true;
                }
                if (okienko != null)
                {
                    switch (okienko.ZwrocStatus())
                    {
                        case Okienko.Status.Pusty:
                            if (SimulationRunning)
                            {
                                if (SprawdzPrzerwe(60))
                                {
                                    NowaSprawa();
                                }
                            }
                            else
                            {
                                KoniecPracy = true;
                                ZwolnijOkienko();
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
                            if (ListaMatek.ElementAt(okienko.WywolanyBilet.ZwrocNumer()).GdzieJest() == Matka.MozliwyStan.Wyszla)
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
            }
            void ZnajdzOkienko()
            {
                ListaOkienekDostep.WaitOne();
                Thread.Sleep(500);
                int index = ListaOkienek.FindIndex(x => !x.CzyJestUrzednik());
                if (index != -1)
                {
                    okienko = ListaOkienek.ElementAt(index);
                    okienko.UrzednikPodszedl();
                    Console.WriteLine("Urzednik " + ID + " podszedl do okienka " + okienko.NumerOkienka + " o godzinie " + czas.getTime());
                    ostatniaPrzerwa = czas.getTime();
                }
                ListaOkienekDostep.ReleaseMutex();
            }
            bool SprawdzPrzerwe(int okres)
            {
                if(czas_przerwy > 2)
                {
                    if (czas.getTime().Hour < 8)
                        ostatniaPrzerwa = czas.getTime();
                    TimeSpan span = czas.getTime().Subtract(ostatniaPrzerwa);
                    if (span.TotalMinutes > okres)
                    {
                        ZwolnijOkienko();
                        Thread.Sleep((czas_przerwy-2) * TimeBase);
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
                bool Flaga = false;
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
                            Flaga = true;
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
                            Flaga = true;
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
                            Flaga = true;
                        }
                            break;
                }
                ListaSprawDostep.ReleaseMutex();
                if (Flaga)
                    SprawdzPrzerwe(15);
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
                if (czas.getTime().Hour >= TimeWorkStart && czas.getTime().Hour <TimeWorkEnd)
                {
                    return zajety.WaitOne(10);
                }
                else return false;
            }
            public Bilet ZdobadzNumer(Matka matka)
            {
                Bilet bilet;
                Thread.Sleep(Opoznienie*TimeBase);
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
                zajety.ReleaseMutex();
                return bilet;
            }
        }
        static void Main(string[] args)
        {
            SetupTime(100, 7, 12, 8, 11);
            Setup();
            GenerujBiletomat(100);
            //GenerujUrzednika(0, 60, 2);
            //GenerujUrzednika(1, 10, 2);
            //Generacja urzednikow i matek w trakcie symulacji
            while (SimulationRunning)
            {
                ConsoleKeyInfo cki;
                cki = Console.ReadKey();
                switch (cki.Key.ToString())
                {
                    case "A":
                        GenerujMatke(ListaWatkow.Count, 10);
                        break;
                    case "U":
                        GenerujUrzednika(ListaUrzednikow.Count, 0);
                        break;
                    default:
                        break;

                }
            }
            //Polaczenie watkow
            foreach(Thread watek in ListaWatkow)
            {
                watek.Join();
            }
            //Spis końcowy
            Console.Clear();
            foreach (Matka matka in ListaMatek)
            {
                matka.Update();
            }
            
            Console.ReadKey();
        }
    }
}
