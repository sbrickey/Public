$wsProxy = New-WebServiceProxy -Uri "http://server/_vti_bin/lists.asmx" -UseDefaultCredential
$wsProxy.PreAuthenticate = $true;

$xmlDoc = New-Object System.Xml.XmlDocument
$ndQuery        = $xmlDoc.CreateNode( ( [System.Xml.XmlNodeType]::Element ) , "Query" , "" )
$ndViewFields   = $xmlDoc.CreateNode( ( [System.Xml.XmlNodeType]::Element ) , "ViewFields" , "" )
$ndQueryOptions = $xmlDoc.CreateNode( ( [System.Xml.XmlNodeType]::Element ) , "QueryOptions" , "" )


$items = $wsProxy.GetListItems("Products", $null, $ndQuery, $ndViewFields, $null, $ndQueryOptions, $null)

$items.data.row