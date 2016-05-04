using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace E33DGLAUNER
{
    public class Communicator : IDisposable
    {
        public delegate void CallbackEventHandler(Communicator.mydata data);
        public event CallbackEventHandler Callback;

        public struct mydata
        {
            public string Addressee;
            public string Message;

            public mydata(string addr, string msg)
            {
                Addressee = addr;
                Message = msg;
            }
        }


        // My Custom Queue
        private MyQueue myData;
        

        bool disposed = false;
        //private MutexPool MyPool = new MutexPool();
        //private MutexWrapper MyWrapper;

        public Communicator()
        {
            //MyWrapper = MutexPool.getObject();
            myData = new MyQueue();
            myData.Callback += MyData_Callback;

        }
        //Callback event to raise MyQueue's event up to the form
        private void MyData_Callback(Communicator.mydata data)
        {
            Callback(data);
        }

        public Boolean SendAlert(string Addressee, string Message)
        {
            if (!string.IsNullOrEmpty(Addressee))
            {
                string firstChar = Addressee.Substring(0, 1);
                firstChar = firstChar.ToUpper();

                int one = 1;
                int zero = 0;
                int test;

                if (firstChar == "A")
                {
                    throw new E33DGLAUNER.SecureCommunicationException("The first character cannot be A");
                }
                else if (firstChar == "B")
                {
                    test = one / zero;
                }
                else if (firstChar == "C")
                {
                    try
                    {
                        test = one / zero;
                    }
                    catch (DivideByZeroException e)
                    {
                        throw new E33DGLAUNER.SecureCommunicationException(Message, e, Addressee);
                    }
                }
            }

            //Testing, hitting the call button calls getObjectFromPool
            //getObjectFromPool();
            //Testing
            try
            {
                myData.EnqueueTask(new E33DGLAUNER.Communicator.mydata(Addressee, Message));
                
                //testing
                //for(int i = 0; i < 10; i++)
                //{
                //    myData.EnqueueTask(new E33DGLAUNER.Communicator.mydata(i.ToString(), Message));
                //}

                return true;
            }
            catch
            {
                return false;
            }

        }

        //private void getObjectFromPool()
        //{
        //    if (disposed)
        //    {
        //        throw new InvalidOperationException("Communicator object has been disposed.");
        //    }

        //    try
        //    {
        //        try
        //        {
        //            MutexWrapper tempObj;
        //            tempObj = MutexPool.getObject();
        //            tempObj = null;
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            System.GC.Collect();
        //            MutexWrapper tempObj;
        //            tempObj = MutexPool.getObject();
        //            tempObj = null;
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //}

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            CleanUp(disposing);
        }

        private void CleanUp(bool userCall)
        {
            if (disposed)
            {
                return;
            }

            try
            {
                if (userCall)
                {
                    //Free managed objects.
                    //MutexPool.returnObject(MyWrapper);
                    //MyWrapper = null;
                    //MyPool.Dispose();
                    //
                    myData.DieNicely();
                    myData.Dispose();
                }
                //Free unmanaged objects.
            }
            finally
            {
                disposed = true;
            }
        }

        ~Communicator()
        {
            Dispose(false);
        }

    }


}
