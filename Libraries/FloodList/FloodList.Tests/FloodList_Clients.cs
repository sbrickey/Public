using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Libraries.FloodList.Tests
{

    public class FloodList_Provider__String
    {
        public delegate void OnGetResults(FloodList_Results.ProviderResults r);
        private event OnGetResults GetResults;

        // Constructor variables
        iFloodList _list;
        Func<bool> Running;
        Func<bool> Stop;

        // local variables
        int throws = 0;

        public FloodList_Provider__String(iFloodList list, Func<bool> running, Func<bool> stop, OnGetResults results)
        {
            _list = list;
            Running = running;
            Stop = stop;

            GetResults += results;

            Run();
        }

        private void Run()
        {
            while (!Stop.Invoke())
            {
                if (Running.Invoke())
                {
                    _list.Drop(RandomString(3));
                    //_list.Drop(System.IO.Path.GetRandomFileName().Replace(".", ""));
                    //_list.Drop(Guid.NewGuid().ToString("N"));
                    throws++;
                }
            }

            GetResults(new FloodList_Results.ProviderResults() { Thrown = throws });
            //System.Diagnostics.Debug.WriteLine(String.Format("Provider<String> : Throws [{0}]", throws));
        }


        Random RandomString_r = new Random();
        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * RandomString_r.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

    }

    public class FloodList_Provider__Int
    {
        public delegate void OnGetResults(FloodList_Results.ProviderResults r);
        private event OnGetResults GetResults;

        // Constructor variables
        iFloodList _list;
        Func<bool> Running;
        Func<bool> Stop;

        // local variables
        int throws = 0;
        Random r = new Random();

        public FloodList_Provider__Int(iFloodList list, Func<bool> running, Func<bool> stop, OnGetResults results)
        {
            _list = list;
            Running = running;
            Stop = stop;

            GetResults += results;

            Run();
        }

        private void Run()
        {
            while (!Stop.Invoke())
            {
                if (Running.Invoke())
                {
                    _list.Drop(r.Next(1, 255));
                    throws++;
                }
            }

            GetResults(new FloodList_Results.ProviderResults() { Thrown = throws });
            //System.Diagnostics.Debug.WriteLine(String.Format("Provider<Int> : Throws [{0}]", throws));
        }

    }

    public class FloodList_Consumer__String
    {
        public delegate void OnGetResults(FloodList_Results.ConsumerResults r);
        private event OnGetResults GetResults;

        // Constructor variables
        iFloodList _list;
        Func<bool> Running;
        Func<bool> Stop;

        // local variables
        int attempts = 0;
        int catches = 0;

        public FloodList_Consumer__String(iFloodList list, Func<bool> running, Func<bool> stop, OnGetResults results)
        {
            _list = list;
            Running = running;
            Stop = stop;

            GetResults += results;

            Run();
        }

        private void Run()
        {
            String Default_String = default(String);
            while (!Stop.Invoke())
            {
                if (Running.Invoke())
                {
                    attempts++;
                    if (_list.Catch<String>() != Default_String)
                        catches++;
                    //System.Diagnostics.Debug.WriteLine(_list.Catch<String>());
                }
            }

            GetResults(new FloodList_Results.ConsumerResults() { Attempts = attempts, Catches = catches });
            //int CatchesToAttempts = ((catches / attempts) * 100);
            //System.Diagnostics.Debug.WriteLine(String.Format("Consumer<String> : Attempts [{0}]  Catches [{1}]  :  {1}/{0} => {2}", attempts, catches, CatchesToAttempts));
        }
    }

    public class FloodList_Consumer__Int
    {
        public delegate void OnGetResults(FloodList_Results.ConsumerResults r);
        private event OnGetResults GetResults;

        // Constructor variables
        iFloodList _list;
        Func<bool> Running;
        Func<bool> Stop;

        // local variables
        int attempts = 0;
        int catches = 0;

        public FloodList_Consumer__Int(iFloodList list, Func<bool> running, Func<bool> stop, OnGetResults results)
        {
            _list = list;
            Running = running;
            Stop = stop;

            GetResults += results;

            Run();
        }

        private void Run()
        {
            int Default_Int = default(int);
            while (!Stop.Invoke())
            {
                if (Running.Invoke())
                {
                    attempts++;
                    if (_list.Catch<int>() != Default_Int)
                        catches++;
                    //System.Diagnostics.Debug.WriteLine(_list.Catch<int>());
                }
            }

            GetResults(new FloodList_Results.ConsumerResults() { Attempts = attempts, Catches = catches });
            //int CatchesToAttempts = ((catches / attempts) * 100);
            //System.Diagnostics.Debug.WriteLine(String.Format("Consumer<Int> : Attempts [{0}]  Catches [{1}]  :  {1}/{0} => {2}", attempts, catches, CatchesToAttempts));
        }
    }

}

namespace SBrickey.Libraries.FloodList.Tests.Generic
{
    public class FloodList_Provider__String
    {
        public delegate void OnGetResults(FloodList_Results.ProviderResults r);
        private event OnGetResults GetResults;

        // Constructor variables
        iFloodList<String> _list;
        Func<bool> Running;
        Func<bool> Stop;

        // local variables
        int throws = 0;

        public FloodList_Provider__String(iFloodList<String> list, Func<bool> running, Func<bool> stop, OnGetResults results)
        {
            _list = list;
            Running = running;
            Stop = stop;

            GetResults += results;

            Run();
        }

        private void Run()
        {
            while (!Stop.Invoke())
            {
                if (Running.Invoke())
                {
                    _list.Drop(RandomString(3));
                    //_list.Drop(System.IO.Path.GetRandomFileName().Replace(".", ""));
                    //_list.Drop(Guid.NewGuid().ToString("N"));
                    throws++;
                }
            }

            GetResults(new FloodList_Results.ProviderResults() { Thrown = throws });
            //System.Diagnostics.Debug.WriteLine(String.Format("Provider<String> : Throws [{0}]", throws));
        }


        Random RandomString_r = new Random();
        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * RandomString_r.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

    }

    public class FloodList_Provider__Int
    {
        public delegate void OnGetResults(FloodList_Results.ProviderResults r);
        private event OnGetResults GetResults;

        // Constructor variables
        iFloodList<int> _list;
        Func<bool> Running;
        Func<bool> Stop;

        // local variables
        int throws = 0;
        Random r = new Random();

        public FloodList_Provider__Int(iFloodList<int> list, Func<bool> running, Func<bool> stop, OnGetResults results)
        {
            _list = list;
            Running = running;
            Stop = stop;

            GetResults += results;

            Run();
        }

        private void Run()
        {
            while (!Stop.Invoke())
            {
                if (Running.Invoke())
                {
                    _list.Drop(r.Next(1, 255));
                    throws++;

                    //for (int i = 0; i < 100; i++) { }
                }
            }

            GetResults(new FloodList_Results.ProviderResults() { Thrown = throws });
            //System.Diagnostics.Debug.WriteLine(String.Format("Provider<Int> : Throws [{0}]", throws));
        }

    }

    public class FloodList_Consumer__String
    {
        public delegate void OnGetResults(FloodList_Results.ConsumerResults r);
        private event OnGetResults GetResults;

        // Constructor variables
        iFloodList<String> _list;
        Func<bool> Running;
        Func<bool> Stop;

        // local variables
        int attempts = 0;
        int catches = 0;

        public FloodList_Consumer__String(iFloodList<String> list, Func<bool> running, Func<bool> stop, OnGetResults results)
        {
            _list = list;
            Running = running;
            Stop = stop;

            GetResults += results;

            Run();
        }

        String Default_String = default(String);
        private void Run()
        {
            while (!Stop.Invoke())
            {
                if (Running.Invoke())
                {
                    attempts++;
                    if (_list.Catch() != Default_String)
                        catches++;
                    //System.Diagnostics.Debug.WriteLine(_list.Catch<String>());
                }
            }

            GetResults(new FloodList_Results.ConsumerResults() { Attempts = attempts, Catches = catches });
            //int CatchesToAttempts = ((catches / attempts) * 100);
            //System.Diagnostics.Debug.WriteLine(String.Format("Consumer<String> : Attempts [{0}]  Catches [{1}]  :  {1}/{0} => {2}", attempts, catches, CatchesToAttempts));
        }
    }

    public class FloodList_Consumer__Int
    {
        public delegate void OnGetResults(FloodList_Results.ConsumerResults r);
        private event OnGetResults GetResults;

        // Constructor variables
        iFloodList<int> _list;
        Func<bool> Running;
        Func<bool> Stop;

        // local variables
        int attempts = 0;
        int catches = 0;

        public FloodList_Consumer__Int(iFloodList<int> list, Func<bool> running, Func<bool> stop, OnGetResults results)
        {
            _list = list;
            Running = running;
            Stop = stop;

            GetResults += results;

            Run();
        }

        int Default_Int = default(int);
        private void Run()
        {
            while (!Stop.Invoke())
            {
                if (Running.Invoke())
                {
                    attempts++;
                    if (_list.Catch() != Default_Int)
                        catches++;
                    //System.Diagnostics.Debug.WriteLine(_list.Catch<int>());
                }
            }

            GetResults(new FloodList_Results.ConsumerResults() { Attempts = attempts, Catches = catches });
            //int CatchesToAttempts = ((catches / attempts) * 100);
            //System.Diagnostics.Debug.WriteLine(String.Format("Consumer<Int> : Attempts [{0}]  Catches [{1}]  :  {1}/{0} => {2}", attempts, catches, CatchesToAttempts));
        }
    }

}
