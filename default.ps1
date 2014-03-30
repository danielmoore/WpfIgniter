properties {
    $buildDir = Split-Path $psake.build_script_file
    $outDir = Join-Path $buildDir build
    $nuspecPath = Join-Path $buildDir WpfIgniter.nuspec
    $asmInfoTemplatePath = Join-Path $buildDir CommonAssemblyInfo.cs.pstemplate
    $toolsDirPath = Join-Path $buildDir tools

    $env:PATH += ";$toolsDirPath"
    $env:EnableNuGetPackageRestore = $true
}

task Clean {
    rm -Recurse -Force $outDir -ErrorAction Ignore
    Run-MsBuild 'Clean'
}

task Update-Version {
    [xml]$nusepc = Get-Content $nuspecPath

    echo "Setting version to '$($nuspec.package.metadata.version)'"

    Expand-Template $asmInfoTemplatePath {
        $version = $nuspec.package.metadata.version
    }
}

task Compile -depends Clean {
    Run-MsBuild 'Build'
}

task Package -depends Clean, Update-Version, Compile {
    Exec { nuget pack $nuspecPath -OutputDirectory $outDir }
}

task Publish -depends Package {
    Exec { nuget push (Join-Path $outDir '*.nupkg') -NonInteractive }
}

task default -depends Compile

function Run-MsBuild($task) {
    Exec { 
        msbuild "$buildDir\WpfIgniter.sln" `
            /nologo `
            /t:$task `
            /p:Configuration=Release `
            /v:quiet `
            /p:OutDir=$outDir
    }
}

function Expand-Template([string]$TemplateFilePath, [ScriptBlock]$SetProperties) {
    $targetFileName = [IO.Path]::GetFileNameWithoutExtension($TemplateFilePath)
    $targetFilePath = Join-Path $(Split-Path $TemplateFilePath) $targetFileName
        
    Exec {
        . $SetProperties

        Invoke-Expression "@`"`r`n$([IO.File]::ReadAllText($TemplateFilePath))`r`n`"@" | `
            Out-File -Encoding utf8 -FilePath $targetFilePath -Force
    }
}