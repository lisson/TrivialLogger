$tests = @("test1.txt", "test2.txt")

for($i=0;$i -lt 3; $i++)
{
    foreach($t in $tests)
    {
        $payload = @{"LogPath"=$t;"LogMessage"="$i Write me"}
        Measure-Command { Invoke-RestMethod http://localhost:8000 -Body $($payload | ConvertTo-Json)  -Method post }
    }
}