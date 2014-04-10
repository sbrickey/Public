using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Libraries.FloodList.Tests
{
    public static class ProcessorInfo
    {
        private static void getPhysicalLogical()
        {
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
            {
                _physical = int.Parse(item["NumberOfProcessors"].ToString());
                //_logical = int.Parse(item["NumberOfLogicalProcessors"].ToString());
            }
        }
        private static void getCores()
        {
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            _cores = coreCount;
        }

        private static int? _physical;
        public static int Physical
        {
            get
            {
                if (!_physical.HasValue)
                    getPhysicalLogical();
                return _physical.Value;
            }
        }

        private static int? _logical;
        public static int Logical
        {
            get
            {
                if (!_logical.HasValue)
                    _logical = Environment.ProcessorCount;
                    //getPhysicalLogical();
                return _logical.Value;
            }
        }

        private static int? _cores;
        public static int Cores
        {
            get
            {
                if (!_cores.HasValue)
                    getCores();
                return _cores.Value;
            }
        }

        public static bool HT { get { return Logical == Cores * 2; } }
    }
}
