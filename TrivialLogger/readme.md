# A trivial logger that takes in HTTP Requests and writes it onto disk.

# Example

Start the server

The following PowerShell command will create a log with "Write Me" inside.
> $payload = @{"LogPath"="test.txt";"LogMessage"="Write me"}
> Invoke-RestMethod http://localhost:8000 -Body $($payload | ConvertTo-Json)  -Method post

