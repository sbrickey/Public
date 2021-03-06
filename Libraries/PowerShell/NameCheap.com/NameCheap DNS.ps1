# Ref: http://blogs.msdn.com/b/knom/archive/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks.aspx
function Get-NetworkAddress([System.Net.IPAddress] $address, [System.Net.IPAddress] $subnetMask)
{
    $ipAdressBytes = $address.GetAddressBytes()
    $subnetMaskBytes = $subnetMask.GetAddressBytes()

    if ($ipAdressBytes.Length -ne $subnetMaskBytes.Length)
    { throw New-Object ArgumentException("Lengths of IP address and subnet mask do not match.") }

    $broadcastAddress = New-Object byte[] $ipAdressBytes.Length
    for ($i = 0; $i -lt $broadcastAddress.Length; $i++)
    {
        $broadcastAddress[$i] = [byte]( $ipAdressBytes[$i] -band ( $subnetMaskBytes[$i] ) )
    }
    return New-Object System.Net.IPAddress ( , ( [byte[]]$broadcastAddress ) )
}


function Get-IPAddressFor ( $dest ) {
    $nextHop = $ping.Send( $dest, 100, ( [System.Text.Encoding]::ASCII.GetBytes("aaa") ), ( New-Object System.Net.NetworkInformation.PingOptions(1, $true) ) )
    $nextHop_IP = $nextHop.Address.IPAddressToString

    if ($nextHop.Status -eq [System.Net.NetworkInformation.IPStatus]::Success)
    { # destination address reached without router

        # destiation address is LOCAL address
        $nic = [System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() | ? { $_.GetIPProperties().UnicastAddresses | ? { $_.Address -eq $nextHop.Address } }

        if ($nic -eq $null)
        { # validate with broadcast/subnet calculation
            $nic = [System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() `
                 | ? { $_.GetIPProperties().UnicastAddresses `
                     | ? { $_.IPv4Mask -ne $null } `
                     | ? { $_.Address.AddressFamily -eq [System.Net.Sockets.AddressFamily]::InterNetwork } `
                     | ? { ( Get-NetworkAddress -address $_.Address -subnetMask $_.IPv4Mask ).Address -eq ( Get-NetworkAddress -address ( [System.Net.IPAddress]::Parse($dest) ) -subnetMask $_.IPv4Mask ).Address } `
                     }
        }
    }
    
    if ($nextHop.Status -eq [System.Net.NetworkInformation.IPStatus]::TtlExpired)
    { # destination address reached via router
        $nic = [System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() | ? { $_.GetIPProperties().GatewayAddresses | ? { $_.Address -eq $nextHop.Address } }
    }
    
    return $nic.GetIPProperties().UnicastAddresses[0].Address
}

function Set-DnsHost ( [parameter(Position=0, Mandatory=$true)] $apiUser
                     , [parameter(Position=1, Mandatory=$true)] $apiKey
                     , [parameter(Position=2, Mandatory=$true)] $userName
                     , [parameter(Position=3, Mandatory=$true)] $TLD
                     , [parameter(Position=4, Mandatory=$true)] $SLD
                     , [parameter(Position=5, Mandatory=$true)] $HostName
                     , [parameter(ParameterSetName="TypeA"     ) ] [switch] $A
                     , [parameter(ParameterSetName="TypeAAAA"  ) ] [switch] $AAAA
                     , [parameter(ParameterSetName="TypeCNAME" ) ] [switch] $CNAME
                     , [parameter(ParameterSetName="TypeMX"    ) ] [switch] $MX
                     , [parameter(ParameterSetName="TypeMXE"   ) ] [switch] $MXE
                     , [parameter(ParameterSetName="TypeTXT"   ) ] [switch] $TXT
                     , [parameter(ParameterSetName="TypeURL"   ) ] [switch] $URL
                     , [parameter(ParameterSetName="TypeURL301") ] [switch] $URL301
                     , [parameter(ParameterSetName="TypeFRAME" ) ] [switch] $FRAME
                     , [parameter(Mandatory=$true)]                                          [string]$Address
                     , [parameter(ParameterSetName="TypeMX"    ) ]                           [int]   $MXPref
                     ,                                             [ValidateRange(60,60000)] [int]   $TTL = 1800
                     , [switch] $Sandbox
                     ) {
    $Url = "https://api." `
         + ( &{ if ($Sandbox) { "sandbox." } } ) `
         + "namecheap.com/xml.response" `
         + "?apiuser=" + $apiUser `
         + "&apikey=" + $apiKey `
         + "&username=" + $userName `
         + "&Command=namecheap.domains.dns.setHosts" `
         + "&ClientIp=" + ( ( Get-IPAddressFor("4.2.2.1") ).IPAddressToString) `
         + "&TLD=" + $TLD `
         + "&SLD=" + $SLD `
         + "&HostName1=" + $HostName `
         + "&RecordType1=" + ( &{     if ($A)     { "A"     } elseif ($AAAA)   { "AAAA"    } `
                                  elseif ($CNAME) { "CNAME" } elseif ($MX)     { "MX"      } `
                                  elseif ($MXE)   { "MXE"   } elseif ($TXT)    { "TXT"     } `
                                  elseif ($URL)   { "URL"   } elseif ($URL301) { "URL301"  } `
                                  elseif ($FRAME) { "FRAME" } `
                             }  ) `
         + "&Address1=" + $Address `
         + ( &{ if ($MX) { "&MXPref=" + $MXPref } } ) `
         + "&TTL1=" + $TLD
    $wc = New-Object System.Net.WebClient
    $resp = $wc.DownloadString($Url)

	# TODO: expand on this
    return ([xml]$resp).ApiResponse.Status
}

Set-DnsHost -apiUser "abc" -apiKey "def" -userName "bob" -TLD "com" -SLD "scott" -HostName "test" -A  -Address "abc" -TTL 60
Set-DnsHost -apiUser "abc" -apiKey "def" -userName "bob" -TLD "com" -SLD "scott" -HostName "test" -A  -Address "abc" -TTL 60 -Sandbox
Set-DnsHost -apiUser "abc" -apiKey "def" -userName "bob" -TLD "com" -SLD "scott" -HostName "test" -MX -Address "abc" -TTL 60 -MXPref 5
