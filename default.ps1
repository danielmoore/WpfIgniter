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

task Update-CompiledVersion {
    Get-Version 

    Write-Host "Setting compiled version to '$($Script:version)'"

    Expand-Template $asmInfoTemplatePath {
        $version = $Script:version
		
		$dotNetVersion = ($version -split '-')[0]
    }
}

task Update-PackageVersion {
    Write-Host "Setting packaged version to '$($Script:version)'"

	[xml]$nuspec = Get-Content $nuspecPath

	$nuspec.package.metadata.version = "$Script:version"

	$writeSettings = new-Object -TypeName 'Xml.XmlWriterSettings'

	$writeSettings.Indent = $true
	$writeSettings.IndentChars = "  "

	Exec { 
		try { 
			$writer = [Xml.XmlWriter]::Create($nuspecPath, $writeSettings)
			$nuspec.Save($writer) 
		} finally {
			if ($writer -ne $null) {
				$writer.Dispose();
			}
		}
	}
}

task Read-Version {
	while ($version -eq $null) {
		$currentVersion = Get-Version 

		$version = Read-Host "New Version (Currently $currentVersion)"
	}

	$Script:version = $version
}

task Set-Version -depends Read-Version, Update-CompiledVersion, Update-PackageVersion {
	$version = Get-Version
	
	Exec { git commit -am "Updating version to $version" }
}

task Set-Tag {
	$version = Get-Version

	Exec { git tag v/$version --force }
	Exec { git push origin tag v/$version --force }
}

task Compile -depends Clean {
    Run-MsBuild 'Build'
}

task Package -depends Clean, Update-CompiledVersion, Compile {
    Exec { nuget pack $nuspecPath -OutputDirectory $outDir }
}

task Publish -depends Package, Set-Tag {
    Exec { nuget push (Join-Path $outDir '*.nupkg') -NonInteractive }
}

task Release -depends Publish, Set-Version

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
