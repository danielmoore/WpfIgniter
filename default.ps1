properties {
    $buildDir = Split-Path $psake.build_script_file
    $outDir = Join-Path $buildDir build/
    $nuspecPath = Join-Path $buildDir WpfIgniter.nuspec
    $asmInfoTemplatePath = Join-Path $buildDir CommonAssemblyInfo.cs.pstemplate
    $toolsDirPath = Join-Path $buildDir tools

    $buildConfiguration = 'Release'

    $env:PATH += ";$toolsDirPath"
    $env:EnableNuGetPackageRestore = $true
}

task Clean {
    rm -Recurse -Force $outDir -ErrorAction Ignore
    Run-MsBuild 'Clean'
}

task Update-Version {
    Get-Version 

    Write-Host "Setting version to '$($Script:version)'"

    Expand-Template $asmInfoTemplatePath {
        $version = $Script:version
    }
}

task Set-Tag {
	$version = Get-Version

	Exec { git tag v/$version }
	Exec { git push origin tag v/$version }
}

task Compile -depends Clean {
    Run-MsBuild 'Build'
}

task Package -depends Clean, Update-Version, Compile {
    Exec { nuget pack $nuspecPath -OutputDirectory $outDir }
}

task Publish -depends Package, Set-Tag {
    Exec { nuget push (Join-Path $outDir '*.nupkg') -NonInteractive }
}

task default -depends Compile

function Run-MsBuild($task) {
    Exec {
        msbuild "$buildDir\WpfIgniter.sln" `
            /nologo `
            /t:$task `
            /p:Configuration=$buildConfiguration `
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

function Get-Version {
	if (-not (Test-Path variable:script:version)) {
	    Write-Debug "Reading version from `"$nuspecPath`""

	   [xml]$nuspec = Get-Content $nuspecPath
	
		$Script:version = $nuspec.package.metadata.version
	}

	return $Script:version;
}
