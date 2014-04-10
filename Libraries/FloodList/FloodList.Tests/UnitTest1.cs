using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SBrickey.Libraries.FloodList.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            bool running = false;   // providing/consuming data
            bool stop = false;      // thread execution

            iFloodList list = new FloodList_Dict_DeleteOnCatch();

            FloodList_Results results = new FloodList_Results(list.GetType().FullName);

            System.Threading.Thread p_string = new System.Threading.Thread(() => new FloodList_Provider__String(
                list, 
                () => running, 
                () => stop,
                (r) => results.AddResults(typeof(String), r)));
            System.Threading.Thread p_int = new System.Threading.Thread(() => new FloodList_Provider__Int(
                list, 
                () => running, 
                () => stop,
                (r) => results.AddResults(typeof(int), r)));
            System.Threading.Thread C_string = new System.Threading.Thread(() => new FloodList_Consumer__String(
                list,
                () => running,
                () => stop,
                (r) => results.AddResults(typeof(String), r)));
            System.Threading.Thread c_int = new System.Threading.Thread(() => new FloodList_Consumer__Int(
                list,
                () => running,
                () => stop,
                (r) => results.AddResults(typeof(int), r)));

            // start the processes... execution loop not stopped... data supply/consume not running
            p_string.Start();
            p_int.Start();
            C_string.Start();
            c_int.Start();

            // start running the data supply/consumption
            running = true;
            // allow some time to pass
            System.Threading.Thread.Sleep(15000);
            // stop the thread
            stop = true;

            // wait for final spindown (and collect results)
            while (p_string.IsAlive ||
                   p_int.IsAlive ||
                   C_string.IsAlive ||
                   c_int.IsAlive)
            {
                System.Threading.Thread.Sleep(100);
            }

            // print results
            results.PrintResults();
            
        }


        [TestMethod]
        public void Generic__lamda()
        {
            bool running = false;   // providing/consuming data
            bool stop = false;      // thread execution

            FloodList_Generic<int> Ints = new FloodList_Generic<int>();
            FloodList_Generic<string> Strings = new FloodList_Generic<string>();

            FloodList_Results results = new FloodList_Results(typeof(FloodList_Generic<int>).FullName);

            System.Threading.Thread p_string = new System.Threading.Thread(delegate()
            {
                new Generic.FloodList_Provider__String(
                    Strings,
                    () => running,
                    () => stop,
                    (r) => results.AddResults(typeof(String), r));
            });
            System.Threading.Thread C_string = new System.Threading.Thread(delegate()
            {
                new Generic.FloodList_Consumer__String(
                    Strings,
                    () => running,
                    () => stop,
                    (r) => results.AddResults(typeof(String), r));
            });
            System.Threading.Thread p_int = new System.Threading.Thread(delegate()
            {
                new Generic.FloodList_Provider__Int(
                    Ints,
                    () => running,
                    () => stop,
                    (r) => results.AddResults(typeof(int), r));
            });
            System.Threading.Thread c_int = new System.Threading.Thread(delegate()
            {
                new Generic.FloodList_Consumer__Int(
                    Ints,
                    () => running,
                    () => stop,
                    (r) => results.AddResults(typeof(int), r));
            });

            //System.Threading.Thread.CurrentThread.set

            // start the processes... execution loop not stopped... data supply/consume not running
            p_string.Start();
            p_int.Start();
            C_string.Start();
            c_int.Start();

            // start running the data supply/consumption
            running = true;
            // allow some time to pass
            System.Threading.Thread.Sleep(15000);
            // stop the thread
            stop = true;

            // wait for final spindown (and collect results)
            while (p_string.IsAlive ||
                   p_int.IsAlive ||
                   C_string.IsAlive ||
                   c_int.IsAlive)
            {
                System.Threading.Thread.Sleep(100);
            }

            // print results
            results.PrintResults();

        }

        [TestMethod]
        public void Generic__delegate()
        {
            bool running = false;   // providing/consuming data
            bool stop = false;      // thread execution

            FloodList_Generic<int> Ints = new FloodList_Generic<int>();
            FloodList_Generic<string> Strings = new FloodList_Generic<string>();

            FloodList_Results results = new FloodList_Results(typeof(FloodList_Generic<int>).FullName);

            System.Threading.Thread p_string = new System.Threading.Thread(delegate()
            {
                new Generic.FloodList_Provider__String(
                    Strings,
                    () => running,
                    () => stop,
                    (r) => results.AddResults(typeof(String), r));
            });
            System.Threading.Thread C_string = new System.Threading.Thread(delegate()
            {
                new Generic.FloodList_Consumer__String(
                    Strings,
                    () => running,
                    () => stop,
                    (r) => results.AddResults(typeof(String), r));
            });
            System.Threading.Thread p_int = new System.Threading.Thread(delegate()
            {
                new Generic.FloodList_Provider__Int(
                    Ints,
                    () => running,
                    () => stop,
                    (r) => results.AddResults(typeof(int), r));
            });
            System.Threading.Thread c_int = new System.Threading.Thread(delegate()
            {
                new Generic.FloodList_Consumer__Int(
                    Ints,
                    () => running,
                    () => stop,
                    (r) => results.AddResults(typeof(int), r));
            });

            //System.Threading.Thread.CurrentThread.set

            // start the processes... execution loop not stopped... data supply/consume not running
            p_string.Start();
            p_int.Start();
            C_string.Start();
            c_int.Start();

            // start running the data supply/consumption
            running = true;
            // allow some time to pass
            System.Threading.Thread.Sleep(15000);
            // stop the thread
            stop = true;

            // wait for final spindown (and collect results)
            while (p_string.IsAlive ||
                   p_int.IsAlive ||
                   C_string.IsAlive ||
                   c_int.IsAlive)
            {
                System.Threading.Thread.Sleep(100);
            }

            // print results
            results.PrintResults();

        }

        [TestMethod]
        public void Generic__delegate__affinity()
        {
            bool running = false;   // providing/consuming data
            bool stop = false;      // thread execution

            iFloodList<Int32> Ints = new FloodList_Generic2<int>();
            iFloodList<string> Strings = new FloodList_Generic2<string>();

            FloodList_Results results = new FloodList_Results(typeof(FloodList_Generic2<int>).FullName);

            System.Threading.Thread p_string = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(2))
                {
                    new Generic.FloodList_Provider__String(
                        Strings,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(String), r));
                }
            });
            System.Threading.Thread C_string = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(3))
                {
                    new Generic.FloodList_Consumer__String(
                        Strings,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(String), r));
                }
            });
            System.Threading.Thread p_int = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(4))
                {
                    new Generic.FloodList_Provider__Int(
                        Ints,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(int), r));
                }
            });
            System.Threading.Thread c_int = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(5))
                {
                    new Generic.FloodList_Consumer__Int(
                        Ints,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(int), r));
                }
            });

            //System.Threading.Thread.CurrentThread.set

            // start the processes... execution loop not stopped... data supply/consume not running
            p_string.Start();
            p_int.Start();
            C_string.Start();
            c_int.Start();

            // allow the threads some time to spin up
            System.Threading.Thread.Sleep(1000);

            // start running the data supply/consumption
            running = true;
            // allow some time to pass
            System.Threading.Thread.Sleep(15000);
            // stop the thread
            stop = true;

            // wait for final spindown (and collect results)
            while (p_string.IsAlive ||
                   p_int.IsAlive ||
                   C_string.IsAlive ||
                   c_int.IsAlive)
            {
                System.Threading.Thread.Sleep(100);
            }

            // print results
            results.PrintResults();

        }

        [TestMethod]
        public void Generic__delegate__affinity__multipleProviders_String()
        {
            bool running = false;   // providing/consuming data
            bool stop = false;      // thread execution

            iFloodList<Int32> Ints = new FloodList_ConcurrentQueue<int>();
            iFloodList<string> Strings = new FloodList_ConcurrentQueue<string>();

            FloodList_Results results = new FloodList_Results(typeof(FloodList_ConcurrentQueue<string>).FullName);

            System.Threading.Thread string_p1 = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(2))
                {
                    new Generic.FloodList_Provider__String(
                        Strings,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(String), r));
                }
            });
            System.Threading.Thread string_p2 = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(4))
                {
                    new Generic.FloodList_Provider__String(
                        Strings,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(String), r));
                }
            });
            System.Threading.Thread string_c1 = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(6))
                {
                    new Generic.FloodList_Consumer__String(
                        Strings,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(String), r));
                }
            });

            //System.Threading.Thread.CurrentThread.set

            // start the processes... execution loop not stopped... data supply/consume not running
            string_p1.Start();
            string_p2.Start();
            string_c1.Start();

            // allow the threads some time to spin up
            System.Threading.Thread.Sleep(1000);

            // start running the data supply/consumption
            running = true;
            // allow some time to pass
            System.Threading.Thread.Sleep(15000);
            // stop the thread
            stop = true;

            // wait for final spindown (and collect results)
            while (string_p1.IsAlive ||
                   string_p2.IsAlive ||
                   string_c1.IsAlive)
            {
                System.Threading.Thread.Sleep(100);
            }

            // print results
            results.PrintResults();

        } // unit test

        [TestMethod]
        public void Generic__delegate__affinity__multipleProviders_Int32()
        {
            bool running = false;   // providing/consuming data
            bool stop = false;      // thread execution

            iFloodList<Int32> Ints = new FloodList_ConcurrentQueue<int>();
            iFloodList<string> Strings = new FloodList_ConcurrentQueue<string>();

            FloodList_Results results = new FloodList_Results(typeof(FloodList_ConcurrentQueue<int>).FullName);

            System.Threading.Thread int_p1 = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(2))
                {
                    new Generic.FloodList_Provider__Int(
                        Ints,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(String), r));
                }
            });
            System.Threading.Thread int_p2 = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(4))
                {
                    new Generic.FloodList_Provider__Int(
                        Ints,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(String), r));
                }
            });
            System.Threading.Thread int_c1 = new System.Threading.Thread(delegate()
            {
                using (ProcessorAffinity.BeginAffinity(6))
                {
                    new Generic.FloodList_Consumer__Int(
                        Ints,
                        () => running,
                        () => stop,
                        (r) => results.AddResults(typeof(String), r));
                }
            });

            //System.Threading.Thread.CurrentThread.set

            // start the processes... execution loop not stopped... data supply/consume not running
            int_p1.Start();
            int_p2.Start();
            int_c1.Start();

            // allow the threads some time to spin up
            System.Threading.Thread.Sleep(1000);

            // start running the data supply/consumption
            running = true;
            // allow some time to pass
            System.Threading.Thread.Sleep(15000);
            // stop the thread
            stop = true;

            // wait for final spindown (and collect results)
            while (int_p1.IsAlive ||
                   int_p2.IsAlive ||
                   int_c1.IsAlive)
            {
                System.Threading.Thread.Sleep(100);
            }

            // print results
            results.PrintResults();

        } // unit test


        [TestMethod]
        public void testProcessorInfo()
        {
            System.Diagnostics.Debug.WriteLine("Physical Processors: {0}", ProcessorInfo.Physical);
            System.Diagnostics.Debug.WriteLine("Processor Cores    : {0}", ProcessorInfo.Cores);
            System.Diagnostics.Debug.WriteLine("Logical Processors : {0}", ProcessorInfo.Logical);
            System.Diagnostics.Debug.WriteLine("HT Enabled         : {0}", ProcessorInfo.HT);
        }
    }
}
