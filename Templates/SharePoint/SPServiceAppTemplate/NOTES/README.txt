** Projects / Components **

TODO comments are used throughout to designate implementations
Additional tasks:
  - Update the service.svc, web.config and client.config files for the WCF implementation
    NOTE: the public key token will probably need to be adjusted one way or another - I never checked it
  - Add a CREATE.ASPX page in the ADMIN mapped folder, to be used when new service applications are created

SBrickey.SPServiceAppTemplate.Common
  Defined constants used for several purposes:
  - variables used by multiple projects (such as file paths, service names, etc)
  - consolidation of strings used (similar to resource file - such as SharePoint service names/descriptions/etc)

SBrickey.SPServiceAppTemplate.WinService
  Sample windows service application
  - includes install and uninstall methods
  - post-build script:
    - make directory in SOLUTION folder called "_BuildEvent_WorkingFolder_"
	- copy EXE into folder

SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp
  Implementation of SharePoint Service Application Framework, including:

  - CONTRACT \ iServiceAppContract             : WCF contract to be fulfilled by the service application

  - Consumer \ Client						   : Wrapper to the SPWebServiceApplicationProxy for simplified calling
                                                 THIS SHOULD BE THE PRIMARY CONSUMPTION POINT!
												 Constructors exist for the SPServiceContext (default) and for the SPSite (simplified).

  - Consumer \ SPWebServiceApplicationProxy    : Client to the SPWebServiceApplication (within or across farms)
												 client configuration for WCF endpoints/bindings/etc

  - HELPER \ mySPIisWebServiceApplicationProxy : helper for the proxy to make WCF code across the SP service app framework
                                                 (identify endpoints from loadbalancer, configure claims authentication, etc)

  - Provider \ SPWebService                    : registration of a web service.
                                                 Provides briding between
													SPWebServiceApplication entity/code
													SPWebServiceInstance to designate which servers are hosting the service application

  - Provider \ SPWebServiceInstance			   : registration of the web service running on a specific SPServer

  - Consumer \ SPWebServiceProxy               : Client proxy / consumer to the SPWebService
												 Provides bridging between the SPServiceApplicationProxy and the SPWebService

  - Provider \ SPWebServiceApplication         : registration of a service application
                                                 Provides the actual (contract) implementation code.
                                                 Use of the service app framework provides load balancing and failover.

  - Provider \ ServiceHostFactory			   : WCF service factory for the Service Application endpoints
												 configures the endpoint for CLAIMS based auth, since that's what SP uses internally

SBrickey.SPServiceAppTemplate.SharePoint
  Primary SharePoint solution (WSP) project

  - pre-build script:
    - copy Windows Service EXE from "_BuildEvent_WorkingFolder_" directory under SOLUTION
	this ensures that the latest built EXE is always packaged in the WSP

  - Event receivers to install SPWindowsService, SPWebService, SPWebServiceApplication, proxies, etc

  - EXE file for Windows Service in the {hive}\BIN mapped folder

  - WinService \ SPWindowsService          : registration of the Windows Service within SharePoint.
                                             Allows control over credentials and status to be handled by SharePoint.

  - WinService \ SPWindowsServiceInstance  : registration of the web service running on a specific SPServer.
                                             control of the service running status on the specific SPServer

  - service application proxy registration files:
    WebClients
		client.config : WCF client configuration (endpoints, bindings, etc)
	WebServices
	    service.svc   : WCF service definition for implementation class and service host factory class
		web.config    : WCF host configuration (endpoints, bindings, etc)
