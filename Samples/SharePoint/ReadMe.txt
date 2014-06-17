Folders and Files
#################

API - Web Services
	Lists ASMX to query AdventureWorks Products.ps1
		PowerShell script which creates a Web Service proxy to the Lists.ASMX web service, which is then used to query the AdventureWorks' Products (external) list.
		Generic enough that it can be used with almost any list (native or external)


Business Connectivity Services
	AdventureWorks.Production.Product.bdcm
		BDC/BCS model for the AdventureWorks.Production.Product table.
		Includes the following accessors:
			Finder			- SELECT *
			SpecificFinder	- SELECT * WHERE ID
			IDEnumerator	- SELECT ID
			StreamAccessor	- SELECT FILE
		Instructions:
			Import into BCS service application
			modify external system instance properties (server name, authentication option)
			if using windows/sql account impersonation, create application in secure store
			create profile page (for search and file stream)
