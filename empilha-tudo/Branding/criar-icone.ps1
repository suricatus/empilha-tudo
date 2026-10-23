$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$project = Split-Path -Parent $PSScriptRoot
$outputPng = Join-Path $PSScriptRoot 'empilha-tudo-icone.png'
$outputIco = Join-Path $PSScriptRoot 'empilha-tudo-icone.ico'
# Camadas do ícone adaptativo do Android: o fundo é esticado e cortado pela máscara do aparelho,
# e a frente só pode usar os 66% do meio. Por isso a torre é desenhada de novo, menor, na frente.
$outputBackPng = Join-Path $PSScriptRoot 'empilha-tudo-icone-fundo.png'
$outputFrontPng = Join-Path $PSScriptRoot 'empilha-tudo-icone-frente.png'
$unityIcons = Join-Path $project 'Assets/Sprites'
$assetRoot = Join-Path $project 'Assets/Clientes/suricatus/jogo'
$mascotSource = [System.Drawing.Bitmap]::new((Join-Path $project 'Assets/Clientes/suricatus/atracao/arte.png'))
$mascot = $mascotSource.Clone([System.Drawing.Rectangle]::new(440,0,250,340),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
for ($y=305; $y -lt $mascot.Height; $y++) {
  for ($x=0; $x -lt $mascot.Width; $x++) {
    $pixel = $mascot.GetPixel($x,$y)
    # A arte original já mostra o suricato apoiado na peça azul. Remove só a peça nesta faixa,
    # para desenhar a mesma camada por baixo e preservar as patas em primeiro plano.
    $blueFill = $pixel.B -gt 100 -and $pixel.G -gt 85 -and $pixel.B -gt ($pixel.R * 1.2) -and $pixel.G -gt ($pixel.R * 1.15)
    $blueOutline = $pixel.R -lt 26 -and $pixel.G -gt 24 -and $pixel.G -lt 48 -and $pixel.B -gt 42 -and $pixel.B -lt 82
    if ($pixel.A -gt 0 -and ($blueFill -or $blueOutline)) {
      $mascot.SetPixel($x,$y,[System.Drawing.Color]::Transparent)
    }
  }
}

$bitmap = [System.Drawing.Bitmap]::new(1024,1024,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

$square = [System.Drawing.Rectangle]::new(0,0,1024,1024)
$background = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
  $square,
  [System.Drawing.Color]::FromArgb(255,12,31,68),
  [System.Drawing.Color]::FromArgb(255,20,75,103),
  90.0)
$graphics.FillRectangle($background,$square)

$goldPath = [System.Drawing.Drawing2D.GraphicsPath]::new()
$goldPath.AddEllipse(155,35,710,710)
$goldGlow = [System.Drawing.Drawing2D.PathGradientBrush]::new($goldPath)
$goldGlow.CenterPoint = [System.Drawing.PointF]::new(510,380)
$goldGlow.CenterColor = [System.Drawing.Color]::FromArgb(115,255,190,93)
$goldGlow.SurroundColors = @([System.Drawing.Color]::FromArgb(0,255,190,93))
$graphics.FillPath($goldGlow,$goldPath)

$bluePath = [System.Drawing.Drawing2D.GraphicsPath]::new()
$bluePath.AddEllipse(90,155,850,810)
$blueGlow = [System.Drawing.Drawing2D.PathGradientBrush]::new($bluePath)
$blueGlow.CenterPoint = [System.Drawing.PointF]::new(512,540)
$blueGlow.CenterColor = [System.Drawing.Color]::FromArgb(62,75,175,210)
$blueGlow.SurroundColors = @([System.Drawing.Color]::FromArgb(0,55,155,210))
$graphics.FillPath($blueGlow,$bluePath)

$starBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(150,255,236,177))
foreach ($point in @(@(123,230,5),@(858,163,5),@(910,390,4),@(102,520,4),@(898,710,5),@(150,805,4),@(790,87,3))) {
  $graphics.FillEllipse($starBrush,[single]$point[0],[single]$point[1],[single]$point[2],[single]$point[2])
}

# O fundo do ícone adaptativo é só o céu: sem torre e sem borda, porque a máscara do aparelho corta as pontas.
$fundo = $bitmap.Clone([System.Drawing.Rectangle]::new(0,0,1024,1024),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$fundo.Save($outputBackPng,[System.Drawing.Imaging.ImageFormat]::Png)

# A torre usa as peças e a base do jogo; o Suricatu salta do topo em primeiro plano.
# Ela é desenhada em uma camada à parte, para servir também de frente do ícone adaptativo.
$art = [System.Drawing.Bitmap]::new(1024,1024,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$artGraphics = [System.Drawing.Graphics]::FromImage($art)
$artGraphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$artGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$artGraphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

$levels = @(
  @{ File = 'peca-1.png'; X = 317; Y = 770; Width = 390 },
  @{ File = 'peca-2.png'; X = 327; Y = 690; Width = 370 },
  @{ File = 'peca-3.png'; X = 337; Y = 610; Width = 350 },
  @{ File = 'peca-4.png'; X = 347; Y = 530; Width = 330 },
  @{ File = 'peca-5.png'; X = 357; Y = 450; Width = 310 },
  @{ File = 'peca-6.png'; X = 367; Y = 370; Width = 290 }
)
$platform = [System.Drawing.Bitmap]::new((Join-Path $assetRoot 'base.png'))
$artGraphics.DrawImage($platform,[System.Drawing.Rectangle]::new(232,800,560,178))
foreach ($level in $levels) {
  $piece = [System.Drawing.Bitmap]::new((Join-Path $assetRoot $level.File))
  $artGraphics.DrawImage($piece,[System.Drawing.Rectangle]::new($level.X,$level.Y,$level.Width,92))
  $piece.Dispose()
}
$artGraphics.DrawImage($mascot,[System.Drawing.Rectangle]::new(366,22,292,397))
$graphics.DrawImage($art,[System.Drawing.Rectangle]::new(0,0,1024,1024))

# Frente do ícone adaptativo: a mesma torre encolhida para caber nos 676 px do meio (66% de 1024).
$frente = [System.Drawing.Bitmap]::new(1024,1024,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$frenteGraphics = [System.Drawing.Graphics]::FromImage($frente)
$frenteGraphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$frenteGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$frenteGraphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$frenteGraphics.DrawImage($art,
  [System.Drawing.Rectangle]::new(314,174,396,676),
  [System.Drawing.Rectangle]::new(232,22,560,956),
  [System.Drawing.GraphicsUnit]::Pixel)
$frente.Save($outputFrontPng,[System.Drawing.Imaging.ImageFormat]::Png)

$borderPath = [System.Drawing.Drawing2D.GraphicsPath]::new()
$borderPath.AddArc(18,18,74,74,180,90)
$borderPath.AddArc(932,18,74,74,270,90)
$borderPath.AddArc(932,932,74,74,0,90)
$borderPath.AddArc(18,932,74,74,90,90)
$borderPath.CloseFigure()
$borderPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(105,255,220,150),3)
$graphics.DrawPath($borderPen,$borderPath)
$bitmap.Save($outputPng,[System.Drawing.Imaging.ImageFormat]::Png)

# ICO com PNG embutido em 256, 128, 64, 48, 32 e 16 px.
$sizes = @(256,128,64,48,32,16)
$frames = [System.Collections.Generic.List[byte[]]]::new()
foreach ($size in $sizes) {
  $frame = [System.Drawing.Bitmap]::new($size,$size,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $frameGraphics = [System.Drawing.Graphics]::FromImage($frame)
  $frameGraphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
  $frameGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
  $frameGraphics.DrawImage($bitmap,[System.Drawing.Rectangle]::new(0,0,$size,$size))
  $memory = [System.IO.MemoryStream]::new()
  $frame.Save($memory,[System.Drawing.Imaging.ImageFormat]::Png)
  $frames.Add($memory.ToArray())
  $memory.Dispose(); $frameGraphics.Dispose(); $frame.Dispose()
}

$icoStream = [System.IO.File]::Create($outputIco)
$writer = [System.IO.BinaryWriter]::new($icoStream)
$writer.Write([UInt16]0); $writer.Write([UInt16]1); $writer.Write([UInt16]$sizes.Count)
$offset = 6 + 16*$sizes.Count
for ($index=0; $index -lt $sizes.Count; $index++) {
  $size = $sizes[$index]
  $dimension = if ($size -eq 256) { 0 } else { $size }
  $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
  $writer.Write([byte]0); $writer.Write([byte]0)
  $writer.Write([UInt16]1); $writer.Write([UInt16]32)
  $writer.Write([UInt32]$frames[$index].Length); $writer.Write([UInt32]$offset)
  $offset += $frames[$index].Length
}
foreach ($frameData in $frames) { $writer.Write($frameData) }

$writer.Dispose(); $icoStream.Dispose()

# As três artes do Android entram no projeto, onde o PlayerSettings as encontra.
Copy-Item $outputPng (Join-Path $unityIcons 'empilha-tudo-icone.png') -Force
Copy-Item $outputBackPng (Join-Path $unityIcons 'empilha-tudo-icone-fundo.png') -Force
Copy-Item $outputFrontPng (Join-Path $unityIcons 'empilha-tudo-icone-frente.png') -Force

$mascotSource.Dispose(); $mascot.Dispose(); $platform.Dispose(); $bitmap.Dispose(); $graphics.Dispose()
$art.Dispose(); $artGraphics.Dispose(); $fundo.Dispose(); $frente.Dispose(); $frenteGraphics.Dispose()
$background.Dispose(); $goldGlow.Dispose(); $goldPath.Dispose(); $blueGlow.Dispose(); $bluePath.Dispose()
$starBrush.Dispose(); $borderPen.Dispose(); $borderPath.Dispose()
Write-Output "Ícones salvos: $outputPng, $outputBackPng, $outputFrontPng e $outputIco"
Write-Output "Copiados para $unityIcons"
