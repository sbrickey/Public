using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Libraries.FloodList.Tests
{
    public class FloodList_Results
    {
        public struct ProviderResults
        {
            public int Thrown;

            public static ProviderResults operator +(ProviderResults obj1, ProviderResults obj2)
            {
                return new ProviderResults() { Thrown = obj1.Thrown + obj2.Thrown };
            }
        }
        public struct ConsumerResults
        {
            public int Attempts;
            public int Catches;

            public decimal PercentOfAttemptsWereCaught { get { return (decimal)Catches / (decimal)Attempts * 100; } }

            public static ConsumerResults operator +(ConsumerResults obj1, ConsumerResults obj2)
            {
                return new ConsumerResults()
                {
                    Attempts = obj1.Attempts + obj2.Attempts,
                    Catches = obj1.Catches + obj2.Catches
                };
            }
        }
        public struct TypeResults
        {
            public ProviderResults Provider;
            public ConsumerResults Consumer;

            public decimal PercentOfThrownWereCaught { get { return (decimal)Consumer.Catches / (decimal)Provider.Thrown * 100; } }

            public static TypeResults operator +(TypeResults obj1, TypeResults obj2)
            {
                return new TypeResults()
                {
                    Provider = obj1.Provider + obj2.Provider,
                    Consumer = obj1.Consumer + obj2.Consumer
                };
            }
        }

        private string Subject;
        private Dictionary<Type, TypeResults> data = new Dictionary<Type, TypeResults>();

        public FloodList_Results(string subject)
        {
            Subject = subject;
        }

        public void AddResults(Type t, ProviderResults results)
        {
            lock (this)
            {
                if (!data.ContainsKey(t))
                    data.Add(t, new TypeResults() { Provider = results });
                else
                    data[t] = new TypeResults() { Provider = results + data[t].Provider, Consumer = data[t].Consumer };
            }
        }

        public void AddResults(Type t, ConsumerResults results)
        {
            lock (this)
            {
                if (!data.ContainsKey(t))
                    data.Add(t, new TypeResults() { Consumer = results });
                else
                    data[t] = new TypeResults() { Consumer = results + data[t].Consumer, Provider = data[t].Provider };
            }
        }

        public void PrintResults()
        {
            System.Diagnostics.Debug.WriteLine(String.Format("== {0,-10} ==", Subject));
            foreach (Type t in data.Keys)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Provider<{0}> : Throws   [{1,10}]", t.Name, data[t].Provider.Thrown));
                System.Diagnostics.Debug.WriteLine(String.Format("Consumer<{0}> : Attempts [{1,10}]", t.Name, data[t].Consumer.Attempts));
                System.Diagnostics.Debug.WriteLine(String.Format("Consumer<{0}> : Catches  [{1,10}]", t.Name, data[t].Consumer.Catches));
                System.Diagnostics.Debug.WriteLine(String.Format("         {0}    %        [{1}]", new String(' ', t.Name.Length), data[t].Consumer.PercentOfAttemptsWereCaught));
                System.Diagnostics.Debug.WriteLine(String.Format("%        {0}             [{1}]", new String(' ', t.Name.Length), data[t].PercentOfThrownWereCaught));
                //System.Diagnostics.Debug.WriteLine(String.Format(""));
                System.Diagnostics.Debug.WriteLine("");
            }
            //System.Diagnostics.Debug.WriteLine(String.Format("Provider<String> : Throws [{0}]", throws));
        }
    }
}
