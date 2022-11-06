for($i=0;$i -lt 3; $i++)
{
    $payload = @{"LogPath"="test.txt";"LogMessage"="$i Write me"}
    Measure-Command { Invoke-RestMethod http://localhost:8000 -Body $($payload | ConvertTo-Json)  -Method post }
}