$creds = cat ..\..\mkortas.webserwer.credentials.json | convertfrom-json

$ftp = $creds.server
$user = $creds.user
$password = $creds.password
$localRoot = pwd
$remoteRoot = "mkortas.webserwer.pl"
$webclient = New-Object System.Net.WebClient 

$webclient.Credentials = New-Object System.Net.NetworkCredential($user,$password)

function recalculateHashes() {
	echo "recalculating hashes..."
	ls -r | ?{!$_.psiscontainer} | get-hash | select @{Name="Path"; Expression={resolve-path $_.Path -relative}}, HashString | export-csv ./files.csv
}`

function getRemoteHashesFile() {
	echo "downloading hashes file from ftp..."
	$uri = New-Object System.Uri("$ftp/$remoteRoot/files.csv")
	$webclient.downloadFile($uri, "$localRoot\files.remote.csv")
}

function uploadFile($path) {
	write-host "uploading $path"
	$uri = New-Object System.Uri("$ftp/$remoteRoot/$path")
	$webclient.uploadFile($uri, "$localRoot\$path")
}

function deleteFile($path) {
	write-host "deleting $path"
	$uri = New-Object System.Uri("$ftp/$remoteRoot/$path")
	$ftprequest = [System.Net.FtpWebRequest]::create($uri)
	$ftprequest.Credentials =  New-Object System.Net.NetworkCredential($user, $password)
	$ftprequest.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
	$ftprequest.GetResponse() | out-null
}

function uploadDifferences() {
	write-host "getting differences..."
	$locfiles = import-csv ./files.csv
	$remfiles = import-csv ./files.remote.csv
	$files = diff $locfiles $remfiles -prop path, hashString | group path
	write-host "uploading files:"
	$files | % {
		if($_.Group.Count -gt 1) { #updated file
			uploadFile $_.Name
		} 
		elseif($_.Group.SideIndicator -eq "<=") { #added file
			uploadFile $_.Name
		}
		elseif($_.Group.SideIndicator -eq "=>") { #removed file
			deleteFile $_.Name
		}
	}
	write-host "done"
}

recalculateHashes
getRemoteHashesFile
uploadDifferences
uploadFile "files.csv"