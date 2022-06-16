Import-Module $PSScriptRoot\Modules\Set-Window.psm1

$sampleName = $args[0]
$sampleExePath = "./Builds/$($sampleName)/$($sampleName).exe"
$gridMode = $args[1].ToLower()
$interface = $args[2]

$splitGridMode = $gridMode -split "x"

$x = [int]$splitGridMode[1]
$y = [int]$splitGridMode[0]
$width = [int]$args[3]
$height = [int]$args[4]

$nodeCount = $x * $y

echo "Testing sample $($y)x$($x) $($nodeCount) nodes: '$($sampleName)' at path: '$($sampleExePath)'."
$processes = [Object[]]::new(0)

$emitter = start-process -passthru -filepath $sampleExePath -argumentlist "-screen-fullscreen 0 -window-mode borderless -screen-width $($width) -screen-height $($height) -gridSize $($gridMode) -adapterName $($interface) -emitterNode 0 $($nodeCount - 1) 225.0.1.0:25690,25691 -handshakeTimeout 20000 -communicationTimeout 20000"
$processes += $emitter.Id

for ($i = 0; $i -lt $nodeCount - 1; $i++) {
    $repeater = start-process -passthru -filepath $sampleExePath -argumentlist "-screen-fullscreen 0 -window-mode borderless -screen-width $($width) -screen-height $($height) -gridSize $($gridMode) -adapterName $($interface) -node $($i + 1) 225.0.1.0:25691,25690 -handshakeTimeout 20000 -communicationTimeout 20000"
    $processes += $repeater.Id
}

echo "Started: '$($processes.count)' nodes."
start-sleep 1

for ($i = 0; $i -lt $processes.count; $i++) {
    $column = $i % $y
    $row = [math]::Floor(($i / $y) % $x)

    $winX = $column * $width
    $winY = $row * $height

    echo "Setting window position for node: '$($i)' at grid coordinate: ($($column), $($row)): to ($($winX),$($winY)) on process: '$($processes[$i])'."

    Set-Window -ProcessId $processes[$i] -X $winX -Y $winY
}

echo "Hit Ctrl-C to close the cluster"

try
{
    while($true)
    {
        Start-Sleep -Seconds 1
    }
}

finally
{
    for ($i = $nodeCount - 1; $i -ge 0; $i--) {
        stop-process -Id $processes[$i]
    }
}