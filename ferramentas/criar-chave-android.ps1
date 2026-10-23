# Cria a chave de envio do Empilha Tudo para a Play Store e o arquivo que a build lê.
#
#   ./ferramentas/criar-chave-android.ps1
#
# Roda uma vez só. A chave e as senhas ficam em empilha-tudo/Chaves, que nunca vai para o Git:
# guarde uma cópia em lugar seguro, porque sem ela não dá para enviar uma atualização do app.

$ErrorActionPreference = 'Stop'

$project = Join-Path (Split-Path -Parent $PSScriptRoot) 'empilha-tudo'
$folder = Join-Path $project 'Chaves'
$keystore = Join-Path $folder 'empilha-tudo-upload.keystore'
$config = Join-Path $folder 'android.json'
$alias = 'upload'

if (Test-Path $keystore) {
  Write-Output "A chave já existe: $keystore"
  Write-Output 'Apague o arquivo à mão se quiser criar outra (e lembre que a Play Store não aceita troca de chave sem pedido ao Google).'
  exit 0
}

# O keytool vem com a Unity, junto do suporte a Android.
$editor = Split-Path -Parent (Split-Path -Parent (Get-Process -Name 'Unity' -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Path))
$keytool = $null
foreach ($root in @($editor, 'C:\Program Files\Unity\Hub\Editor\6000.3.5f2\Editor')) {
  if (-not $root) { continue }
  $candidate = Join-Path $root 'Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin/keytool.exe'
  if (Test-Path $candidate) { $keytool = $candidate; break }
}
if (-not $keytool) { $keytool = (Get-Command keytool -ErrorAction SilentlyContinue).Source }
if (-not $keytool) { throw 'Não achei o keytool. Ele vem com o módulo Android da Unity (OpenJDK).' }

$senha = Read-Host 'Senha da chave (guarde bem: sem ela não dá para atualizar o app)' -AsSecureString
$repetir = Read-Host 'Repita a senha' -AsSecureString
$texto = [System.Net.NetworkCredential]::new('', $senha).Password
if ($texto -ne [System.Net.NetworkCredential]::new('', $repetir).Password) { throw 'As senhas não são iguais.' }
if ($texto.Length -lt 6) { throw 'A senha precisa de pelo menos 6 caracteres.' }

New-Item -ItemType Directory -Force -Path $folder | Out-Null

& $keytool -genkeypair -v `
  -keystore $keystore `
  -alias $alias `
  -keyalg RSA -keysize 2048 -validity 10000 `
  -storepass $texto -keypass $texto `
  -dname 'CN=Suricatus Games, OU=Jogos, O=Suricatus Games, L=Rio de Janeiro, ST=RJ, C=BR'
if ($LASTEXITCODE -ne 0) { throw "O keytool falhou (código $LASTEXITCODE)." }

$json = [ordered]@{
  keystore = 'Chaves/empilha-tudo-upload.keystore'
  alias = $alias
  senhaKeystore = $texto
  senhaAlias = $texto
} | ConvertTo-Json
Set-Content -Path $config -Value $json -Encoding utf8

$ignore = Join-Path $folder '.gitignore'
Set-Content -Path $ignore -Value "*`n!.gitignore`n" -Encoding utf8

Write-Output ''
Write-Output "Chave criada: $keystore"
Write-Output "Configuração da build: $config"
Write-Output 'Guarde uma cópia da pasta Chaves fora do computador. Agora use Fast-Drive > Android > Gerar .aab para a Play Store.'
