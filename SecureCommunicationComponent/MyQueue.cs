using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E33DGLAUNER;

namespace E33DGLAUNER
{

    public class MyQueue : IDisposable
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
        readonly object _locker = new object();
        readonly object _EnqueueTaskLock = new object();
        //Internal queue
        private Queue<Communicator.mydata> _MyQueue = new Queue<Communicator.mydata>();
        //Random Numbers
        private Random rnd;
        private int MIN_RAND = 10000000;
        private int MAX_RAND = 1000000000;
        //DieNicely flag
        private bool _dieNicely = false;
        
        public MyQueue()
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



        public void DieNicely()
        {
            //Set the Die Nicely flag
            _dieNicely = true;
            //Empty the queue
            while(_MyQueue.Count > 0)
            {
                _MyQueue.Dequeue();
            }
            //Make sure the threads are running 
            //so DoWork(s) can finish and exit.
            _wh.Set();

        }

        public void EnqueueTask(Communicator.mydata _data)
        {
            lock (_EnqueueTaskLock)
            {
                _MyQueue.Enqueue(_data);
                _wh.Set();
            }
        }

        public int QueueCount()
        {
            return _MyQueue.Count();
        }

        public Communicator.mydata DequeueTask()
        {
            return _MyQueue.Dequeue();
        }

        private void DoWork()
            {
                Communicator.mydata job;
                while (true)
                {

                    if (_dieNicely)
                    {
                        //Die Nicely
                        return;
                    }


                    if (_MyQueue.Count > 0)
                    {

                        job = _MyQueue.Dequeue();

                        if (job.Addressee == null)
                        {
                            return;
                        }
                        else
                        {
                            lock (_locker)
                            {
                                // simulate work...
                                int q = rnd.Next(MIN_RAND, MAX_RAND);
                                int i;
                                for (i = 0; i < q; i++)
                                {
                                    //Lets play the waiting game! 
                                }

                                job.Message = Thread.CurrentThread.Name;

                                if (!_dieNicely)
                                {
                                    //Don't try a callback event if the app is closing
                                    Callback(job);
                                }
                            }

                        }
                    }
                    else
                    {
                        // No more tasks - wait for a signal from EnqueueTask
                        _wh.WaitOne();
                    }
               

            }
        }

        public void Dispose()
        {
            DieNicely();
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
                
            try
            {
                if (disposing)
                {
                    
                    Communicator.mydata d = new Communicator.mydata(null, null);
                    EnqueueTask(d);         // Signal DoWork to Die Nicely.
                    d = new Communicator.mydata(null, null);
                    EnqueueTask(d);         // Signal DoWork to Die Nicely.

                    _worker.Join();         // Wait for DoWork's thread to finish.
                    _worker2.Join();        // Wait for DoWork's thread to finish.
                    _wh.Close();            // Release the waithandle.
                    _wh.Dispose();
                    // Free any other managed objects here.
                    //
                }
                // Free any unmanaged objects here.
            }
            finally
            {
                disposed = true;
            }

        }

        ~MyQueue()
        {
            Dispose(false);
        }
    }
}
