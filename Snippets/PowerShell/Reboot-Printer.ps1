# Source : https://www.andreadraghetti.it/remotely-reboot-of-hp-and-other-printers/
$PrinterIP = "10.0.0.20"
$SNMP = New-Object -ComObject olePrn.OleSNMP
$SNMP.Open($PrinterIP, "public")
$SNMP.Set(".1.3.6.1.2.1.43.5.1.1.3.1",4)
$SNMP.Close()
