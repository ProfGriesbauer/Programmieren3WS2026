# RunTrainRunTests.ps1, run in PowerShell.
$ErrorActionPreference = 'Stop'

$projectPath = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\OOPGames.csproj')).Path
$runnerDirectory = Join-Path $env:TEMP ('TrainRunTests-' + [guid]::NewGuid().ToString('N'))
$runnerExitCode = 0

try {
    New-Item -ItemType Directory -Path $runnerDirectory | Out-Null

    $escapedProjectPath = [System.Security.SecurityElement]::Escape($projectPath)
    $projectFile = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <EnableWindowsTargeting>true</EnableWindowsTargeting>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="$escapedProjectPath" />
  </ItemGroup>
</Project>
"@
    Set-Content -LiteralPath (Join-Path $runnerDirectory 'TrainRunTestsRunner.csproj') `
        -Value $projectFile -Encoding UTF8

    $runnerSource = @'
using System;
using OOPGames;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        try
        {
            Gruppe2_TrainRunTests.RunAllTests();
            Console.WriteLine("All Train Run tests passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
'@
    Set-Content -LiteralPath (Join-Path $runnerDirectory 'Program.cs') `
        -Value $runnerSource -Encoding UTF8

    Get-Command dotnet -ErrorAction Stop | Out-Null
    & dotnet run --project (Join-Path $runnerDirectory 'TrainRunTestsRunner.csproj') `
        -p:UseAppHost=false "-p:OutDir=$(Join-Path $runnerDirectory 'build')\"
    if ($LASTEXITCODE -ne 0) {
        $runnerExitCode = $LASTEXITCODE
    }
}
finally {
    if (Test-Path -LiteralPath $runnerDirectory) {
        Remove-Item -LiteralPath $runnerDirectory -Recurse -Force
    }
}

exit $runnerExitCode
