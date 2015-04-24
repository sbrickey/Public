using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;

namespace SDS.SPConnectors.DirectoryServices
{
    public class DomainControllers
    {
        public struct DomainController
        {
            public string HostName;
            public string IPAddress;
            public string Site;
        }

        public static DomainController[] GetAll()
        {
            return Domain.GetComputerDomain()
                         .DomainControllers.Cast<System.DirectoryServices.ActiveDirectory.DomainController>()
                         .Select(dc => new DomainController() { 
                             HostName = dc.Name,
                             IPAddress = dc.IPAddress,
                             Site = dc.SiteName
                         })
                         .ToArray();
        } // IEnum<DomainController> GetAll()

        public static DomainController GetByName(string HostName)
        {
            return Domain.GetComputerDomain()
                         .DomainControllers.Cast<System.DirectoryServices.ActiveDirectory.DomainController>()
                         .Select(dc => new DomainController()
                         {
                             HostName = dc.Name,
                             IPAddress = dc.IPAddress,
                             Site = dc.SiteName
                         })
                         .Single(dc => dc.HostName == HostName);
        } // DomainController GetByName(...)

    } // class
} // namespace
