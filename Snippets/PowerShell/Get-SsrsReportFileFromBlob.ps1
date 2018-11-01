# Configuration data
$Server = "RsServer";
$Database = "ReportServer"
$Dest = "C:\SsrsRDL\"
$bufferSize = 8192
$Sql = "SELECT [ContentType] , [Content]
          FROM [ReportServer].[dbo].[CatalogItemExtendedContent]
         WHERE itemid = 'ABCDEF12-1234-5678-90AB-CDEF01234567'";

# Open ADO.NET Connection
$con = New-Object Data.SqlClient.SqlConnection;
$con.ConnectionString = "Data Source=$Server;" +
                        "Integrated Security=True;" +
                        "Initial Catalog=$Database";
$con.Open();

# New Command and Reader
$cmd = New-Object Data.SqlClient.SqlCommand $Sql, $con;
$rd = $cmd.ExecuteReader();

# Create a byte array for the stream.
$out = [byte[]]::new($bufferSize)

# Looping through records
While ($rd.Read())
{
    Write-Output ("Exporting: {0}" -f $rd.GetString(0));
    # New BinaryWriter
    $fs = New-Object System.IO.FileStream ($Dest + $rd.GetString(0)), Create, Write;
    $bw = New-Object System.IO.BinaryWriter $fs;

    $start = 0;
    # Read first byte stream
    $received = $rd.GetBytes(1, $start, $out, 0, $bufferSize - 1);
    While ($received -gt 0)
    {
       $bw.Write($out, 0, $received);
       $bw.Flush();
       $start += $received;
       # Read next byte stream
       $received = $rd.GetBytes(1, $start, $out, 0, $bufferSize - 1);
    }

    $bw.Close();
    $bw.Dispose();
    $fs.Close();
    $fs.Dispose();
}

# Closing & Disposing all objects
$fs.Dispose();
$rd.Close();
$cmd.Dispose();
$con.Close();

Write-Output ("Finished");
