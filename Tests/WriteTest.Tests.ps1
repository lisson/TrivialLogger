Describe "LogServerTest"  {
    It "Writes 2 log files" {
        $x64 = ".\TrivialLogger\bin\x64\Release\"
        
        "rmdir /s /q .\UserLogs\__1" | cmd

        $server = Start-Process $x64\TrivialLogger.exe -PassThru

        $server | Should -Not -BeNullOrEmpty

        $tests = @("test1.txt", "test2.txt")
        for($i=0;$i -lt 3; $i++)
        {
            foreach($t in $tests)
            {
                $payload = @{"LogPath"=$t;"LogMessage"="$i Write me"}
                Measure-Command { Invoke-RestMethod http://localhost:8000 -Body $($payload | ConvertTo-Json)  -Method post }
            }
        }

        Stop-Process $server

        $test1 = Get-Content .\UserLogs\__1\test1.txt
        $test2 = Get-Content .\UserLogs\__1\test2.txt

        $test1.Count | Should -Be 3
        $test2.Count | Should -Be 3
    }
}

