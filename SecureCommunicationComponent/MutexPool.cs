using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace E33DGLAUNER
{
    internal class MutexPool : IDisposable
    {
        private static Queue<MutexWrapper> myObjects;
        bool disposed = false;

        public MutexPool()
        {
            myObjects = new Queue<MutexWrapper>();
            //Create 5 wrappers initally
            while (myObjects.Count() < 5)
            {
                myObjects.Enqueue(new MutexWrapper());
            }
        }

        public static MutexWrapper getObject()
        {
            //Remove after testing
            //MessageBox.Show("Used Count = " + MutexWrapper.usedCount.ToString() + ", Queue Count = " + myObjects.Count.ToString());

            if (myObjects.Count() > 0)
            {

                return myObjects.Dequeue();
            }
            else if (myObjects.Count() == 0 && MutexWrapper.usedCount < 10)
            {

                return new MutexWrapper();
            }
            else
            {

                if (MutexWrapper.usedCount < 10)
                {
                    return new MutexWrapper();
                }
                else
                {
                    throw new InvalidOperationException("All objects in use!");
                }

            }

        }

        public static void returnObject(MutexWrapper myObj)
        {
            myObjects.Enqueue(myObj);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
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
                    MutexWrapper myObj;
                    while (myObjects.Count() > 0)
                    {
                        myObj = myObjects.Dequeue();
                        myObj.Dispose();
                    }
                    myObj = null;
                    // Free any other managed objects here.
                    //
                }
            }
            finally
            {
                // Free any unmanaged objects here.
                //
                disposed = true;
            }

        }

        ~MutexPool()
        {
            Dispose(false);
        }

    }

    internal class MutexWrapper : IDisposable
    {
        bool disposed = false;
        public static int usedCount { get; set; }
        private System.Threading.Mutex _mutex;
        public MutexWrapper()
        {
            _mutex = new System.Threading.Mutex();
            
            usedCount++;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }



            // Free any unmanaged objects here.
            try
            {
                if (disposing)
                {
                    // Free any other managed objects here.
                    //
                }
                _mutex.Dispose();
            }
            finally
            {
                usedCount--;
                //
                disposed = true;
            }
            
        }

        ~MutexWrapper()
        {
            Dispose(false);
        }

    }
}
