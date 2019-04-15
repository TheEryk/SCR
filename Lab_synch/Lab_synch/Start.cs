using System;
using System.Threading;
namespace Lab_synch
{
    abstract class Start
    {
        protected abstract void Update();
        private static Mutex mut = new Mutex();
        private static readonly object x = new object();
        private static SpinLock sl = new SpinLock();
        bool gotLock = false;
        bool przerwij = false;

        private static int usingResource = 0;

        private static bool[] Entering = new bool[300];
        private static int[] Number = new int[300];

        public void Piekarnia(int i)
        {

        }


        public virtual void Run()
        {
            while(!przerwij)
            {
                // mutex

                //mut.WaitOne();
                //Update();
                //mut.ReleaseMutex();
                //Thread.Sleep(2000);

                // lock

                //lock(x)
                //{
                //    Update();
                //}
                //Thread.Sleep(2000);

                //
                // SpinLock
                //

                // while

                //gotLock = false;
                //try
                //{
                //    sl.Enter(ref gotLock);
                //    Update();
                //}
                //finally
                //{
                //    if (gotLock) sl.Exit();
                //}
                //Thread.Sleep(2000);

                // normalny cykl

                //gotLock = false;
                //try
                //{
                //    sl.TryEnter(1, ref gotLock);
                //}
                //finally
                //{
                //    if (gotLock)
                //    {
                //        Update();
                //        sl.Exit();
                //    }
                //}
                //Thread.Sleep(2000);

                // anulowanie

                //gotLock = false;
                //try
                //{
                //    sl.TryEnter(1, ref gotLock);
                //}
                //finally
                //{
                //    if (gotLock)
                //    {
                //        Update();
                //        sl.Exit();
                //    }
                //    else
                //    {
                //        przerwij = true;
                //    }
                //}
                //Thread.Sleep(2000);


                // atomic

                //if (0 == Interlocked.Exchange(ref usingResource, 1))
                //{
                //    Update();
                //    Interlocked.Exchange(ref usingResource, 0);
                //}
                //Thread.Sleep(2000);

                // piekarnia

                lock(x)
                {

                }

            }
        }
    }
}