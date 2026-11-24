# Script tự động sao lưu và xuất dữ liệu CSDL SQL Server (LibraryDB) sang file LibraryDB.sql
param(
    [string]$OutputFile = "LibraryDB.sql"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not $projectRoot) { $projectRoot = Get-Location }

$appSettingsPath = Join-Path $projectRoot "appsettings.json"
$connectionString = "Data Source=LAPTOP-KFE3NMD3;Initial Catalog=LibraryDB;Integrated Security=True;TrustServerCertificate=True"

if (Test-Path $appSettingsPath) {
    try {
        $json = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        if ($json.ConnectionStrings.DefaultConnection) {
            $connectionString = $json.ConnectionStrings.DefaultConnection
        }
    } catch {}
}

$targetSqlPath = Join-Path $projectRoot $OutputFile
Write-Host ">>> Đang kết nối tới CSDL: $connectionString"
Write-Host ">>> Xuất dữ liệu mới nhất vào: $targetSqlPath"

$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

$tables = @(
    "tblCategory",
    "tblAuthor",
    "tblPublisher",
    "tblBook",
    "tblUser",
    "tblBorrowTicket",
    "tblBorrowDetail",
    "tblAdminMenu"
)

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("-- =================================================================")
[void]$sb.AppendLine("-- DỮ LIỆU CƠ SỞ DỮ LIỆU MỚI NHẤT: LibraryDB")
[void]$sb.AppendLine("-- Thời gian xuất tự động: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')")
[void]$sb.AppendLine("-- Dự án: QuanLyThuVienTruongHoc (ASP.NET Core MVC)")
[void]$sb.AppendLine("-- =================================================================")
[void]$sb.AppendLine("USE [LibraryDB];")
[void]$sb.AppendLine("GO")
[void]$sb.AppendLine("")

foreach ($table in $tables) {
    # Kiểm tra xem bảng có tồn tại không
    $cmdCheck = $conn.CreateCommand()
    $cmdCheck.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$table'"
    $exists = [int]$cmdCheck.ExecuteScalar()
    if ($exists -eq 0) { continue }

    # Lấy thông tin các cột
    $cmdCols = $conn.CreateCommand()
    $cmdCols.CommandText = @"
SELECT COLUMN_NAME, DATA_TYPE, COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') as IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = '$table'
ORDER BY ORDINAL_POSITION
"@
    $rdrCols = $cmdCols.ExecuteReader()
    $cols = @()
    $hasIdentity = $false
    while ($rdrCols.Read()) {
        $colName = $rdrCols["COLUMN_NAME"].ToString()
        $isId = [int]$rdrCols["IsIdentity"] -eq 1
        if ($isId) { $hasIdentity = $true }
        $cols += [PSCustomObject]@{
            Name = $colName
            Type = $rdrCols["DATA_TYPE"].ToString()
            IsIdentity = $isId
        }
    }
    $rdrCols.Close()

    # Đọc dữ liệu
    $colList = ($cols | ForEach-Object { "[$($_.Name)]" }) -join ", "
    $cmdData = $conn.CreateCommand()
    $cmdData.CommandText = "SELECT $colList FROM [$table]"
    $rdrData = $cmdData.ExecuteReader()

    $rows = @()
    while ($rdrData.Read()) {
        $valList = @()
        for ($i = 0; $i -lt $cols.Count; $i++) {
            if ($rdrData.IsDBNull($i)) {
                $valList += "NULL"
            } else {
                $val = $rdrData.GetValue($i)
                $type = $cols[$i].Type.ToLower()
                if ($type -in @("int", "bigint", "smallint", "tinyint", "decimal", "numeric", "float", "real")) {
                    $valList += "$val"
                } elseif ($type -eq "bit") {
                    $valList += if ([bool]$val) { "1" } else { "0" }
                } elseif ($type -in @("datetime", "datetime2", "date", "smalldatetime")) {
                    $dtStr = [DateTime]$val | Get-Date -Format "yyyy-MM-dd HH:mm:ss"
                    $valList += "'$dtStr'"
                } else {
                    $escVal = $val.ToString().Replace("'", "''")
                    $valList += "N'$escVal'"
                }
            }
        }
        $rows += "($($valList -join ', '))"
    }
    $rdrData.Close()

    if ($rows.Count -gt 0) {
        [void]$sb.AppendLine("-- -------------------------------------------------------------")
        [void]$sb.AppendLine("-- Bảng: [$table] ($($rows.Count) bản ghi)")
        [void]$sb.AppendLine("-- -------------------------------------------------------------")
        if ($hasIdentity) {
            [void]$sb.AppendLine("SET IDENTITY_INSERT [$table] ON;")
        }
        
        # Tạo batch insert tối đa 100 dòng mỗi câu lệnh
        $batchSize = 100
        for ($i = 0; $i -lt $rows.Count; $i += $batchSize) {
            $currentBatch = $rows[$i..([Math]::Min($i + $batchSize - 1, $rows.Count - 1))]
            [void]$sb.AppendLine("INSERT INTO [$table] ($colList) VALUES")
            [void]$sb.AppendLine(($currentBatch -join ",`r`n") + ";")
        }

        if ($hasIdentity) {
            [void]$sb.AppendLine("SET IDENTITY_INSERT [$table] OFF;")
        }
        [void]$sb.AppendLine("GO")
        [void]$sb.AppendLine("")
    }
}

$conn.Close()

[System.IO.File]::WriteAllText($targetSqlPath, $sb.ToString(), [System.Text.Encoding]::UTF8)
Write-Host ">>> ĐÃ XUẤT THÀNH CÔNG DỮ LIỆU CSDL VÀO: $targetSqlPath"
