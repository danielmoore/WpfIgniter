param(
    [Parameter(Mandatory = $true)]
    [xml]$Xaml,
    [double]$RoundTolerance = 0,
    [double[]]$Origin = @(0,0)
    )
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName 'PresentationCore'

function New-Point([double]$X, [double]$Y) {
     New-Object Windows.Point -ArgumentList @($X, $Y)
}

$globalOrigin = New-Point @Origin

$geomList = @()

function ConvertTo-PathGeometry ($Data) {
    $geom = [System.Windows.Media.Geometry]::Parse($Data)

    [System.Windows.Media.Geometry].InvokeMember('GetAsPathGeometry', 'InvokeMethod,Instance,NonPublic', $null, $geom, @());
}

$nodes = if ($Xaml.Canvas -ne $null) {
    $output = 'ImageSource'
    $Xaml.Canvas.ChildNodes 
} elseif ($Xaml.Path -ne $null) { 
    $output = 'Geometry'
    @($Xaml.Path) 
} else { return }

foreach($path in $nodes) {
    $geom = ConvertTo-PathGeometry $path.Data

    $geomList += [pscustomobject]@{
        Geometry = $geom
        Fill = $path.Fill
        Origin = switch ($output) {
            'ImageSource' {
                $canvasLoc = New-Point $path.Attributes['Canvas.Left'].Value $path.Attributes['Canvas.Top'].Value
                [Windows.Point]::Subtract($geom.Bounds.Location, $canvasLoc)
            }

            'Geometry' { $geom.Bounds.Location }
        }
    }
}

function Optimize-SegmentPoint($Point, $Origin) {
    function round($value) {
        [Math]::Round($value / $RoundTolerance) * $RoundTolerance
    }

    $loc = [Windows.Point]::Add([Windows.Point]::Subtract($Point, $Origin), $globalOrigin)

    if ($RoundTolerance -gt 0) {
        New-Point (round $loc.X) (round $loc.Y)
    } else {
        $loc
    }
}

$xCoords = New-Object Collections.Generic.SortedSet[double]
$yCoords = New-Object Collections.Generic.SortedSet[double]

function Add-Coordinates($Point) {
    $xCoords.Add($Point.X) | Out-Null
    $yCoords.Add($Point.Y) | Out-Null
}

foreach($item in $geomList) {
    foreach($fig in $item.Geometry.Figures) {
        $fig.StartPoint = Optimize-SegmentPoint $fig.StartPoint $item.Origin
        Add-Coordinates $fig.StartPoint
        
        foreach($seg in $fig.Segments) {
            if (($seg | gm -Name 'Points') -ne $null) {
                for($i = 0; $i -lt $seg.Points.Count; $i += 1) {
                    $seg.Points[$i] = Optimize-SegmentPoint $seg.Points[$i] $item.Origin
                    Add-Coordinates $seg.Points[$i]
                }
            } else {
                # Some figures have a single Point property, others have Point1, Point2, etc.
                $pointProps = $seg | gm -Name 'Point*' | select -ExpandProperty 'Name'

                if ($pointProps -ne $null) {
                    foreach($propName in @($pointProps)) {
                        $seg.$propName = Optimize-SegmentPoint $seg.$propName $item.Origin
                        Add-Coordinates $seg.Point
                    }
                }
            }
        }
    }
}

switch ($output) {
    'ImageSource' {
@"
<DrawingImage>
    <DrawingImage.Drawing>
        <DrawingGroup>
            <DrawingGroup.GuidelineSet>
            	<GuidelineSet
                    GuidelinesX="$([string]::Join(',', $xCoords))"
                    GuidelinesY="$([string]::Join(',', $yCoords))"/>
            </DrawingGroup.GuidelineSet>

$(foreach ($item in $geomList) {
@"
            <GeometryDrawing Brush="$($item.Fill)" Geometry="$($item.Geometry.ToString())"/>

"@
})        </DrawingGroup>
    </DrawingImage.Drawing>
</DrawingImage>
"@
    }

    'Geometry' {
@"
<PathGeometry>$($geomList.Geometry.ToString())</PathGeometry>
"@
    }
}