$src_SQL = "PersistSecurityInfo = true;Server=SqlSrc;Database=DbSrc;Integrated Security=True"
$dst_SQL = "PersistSecurityInfo = true;Server=SqlDst;Database=DbDst;Integrated Security=True"

$src_Conn = New-Object System.Data.SqlClient.SqlConnection($src_SQL)
$dst_Conn = New-Object System.Data.SqlClient.SqlConnection($dst_SQL)

$src_Cmd = New-Object System.Data.SqlClient.SqlCommand( "SELECT * FROM DbSrc.dbo.SrcTable WITH (NOLOCK)" , $src_Conn )
$dst_BCP = New-Object System.Data.SqlClient.SqlBulkCopy( $dst_Conn )
$dst_BCP.BulkCopyTimeout = 0
$dst_BCP.BatchSize = 1000
$dst_BCP.DestinationTableName = "dbo.DstTable"

$src_Conn.Open()
$dst_Conn.Open()

$src_Data = $src_Cmd.ExecuteReader()
$dst_BCP.WriteToServer($src_Data)

$dst_BCP.Close()
$dst_Conn.Close()
$src_Data.Close()
$src_Conn.Close()
