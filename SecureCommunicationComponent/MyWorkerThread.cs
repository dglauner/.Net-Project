using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E33DGLAUNER
{
    class MyWorkerThread
    {
        //Disposed Flag
        private bool disposed = false;
        //Callback event stuff
        public delegate void CallbackEventHandler(Communicator.mydata data);
        public event CallbackEventHandler Callback;
        //Thread Stuff
        EventWaitHandle _wh = new AutoResetEvent(false);
        Thread _worker;
        Thread _worker2;
        //Locker Objects
        readonly object _locker1 = new object();
        readonly object _locker2 = new object();
        //Random Numbers
        private Random rnd;
        private int MIN_RAND = 1000000;
        private int MAX_RAND = 1000000000;

        public MyWorkerThread()
        {
            //Create Thread one
            _worker = new Thread(DoWork);
            _worker.Priority = ThreadPriority.BelowNormal;
            _worker.Name = "Worker1";
            _worker.Start();
            //Create Thread two
            _worker2 = new Thread(DoWork);
            _worker2.Priority = ThreadPriority.BelowNormal;
            _worker2.Name = "Worker2";
            _worker2.Start();

            rnd = new Random();
        }

        private void DoWork()
        {
            Communicator.mydata job;
            while (true)
            {
                lock (_locker1)
                {
                    if ( > 0)
                    {
                        job = _MyQueue.Dequeue();
                        if (job.Addressee == null)
                        {
                            //Die Nicely
                            return;
                        }
                        else
                        {
                            // simulate work...
                            int q = rnd.Next(MIN_RAND, MAX_RAND);
                            int i;
                            for (i = 0; i < q; i++)
                            {
                                //Lets play Beer Pong while we wait! 
                            }
                            //Thread.Sleep (1000);  
                            job.Message = Thread.CurrentThread.Name;
                            Callback(job);
                        }
                    }
                    else
                    {
                        // No more tasks - wait for a signal from EnqueueTask
                        _wh.WaitOne();
                    }
                }
            }
        }
    }
}
