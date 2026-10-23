# Empilha Tudo · Fast-Drive

Terceiro jogo do Fast-Drive Game, a arquitetura da Suricatus para fazer vários jogos com o mesmo layout, ligando e desligando pacotes por cliente. Leia o `README.md` deste repositório e o `README.md` da Base (`C:\Users\User\Documents\GitHub\Suricatus\Ferramentas\Tools\tools\FastDrive.Base`) antes de mudar código. O Caça Palavras (`Jogos - Prateleira\Suricatus - Caca Palavras`) é o jogo de referência; o Reflexo Relâmpago é o mais parecido em estrutura (artes do jogo em `jogo/`, faixas no conteúdo).

## Regras de arquitetura

- **Base e pacotes ficam fora do jogo**, no repositório Tools (`Ferramentas/Tools/tools/FastDrive.*`, pacotes `com.suricatusgames.fastdrive.*`). O jogo os referencia no `Packages/manifest.json` por caminho relativo (`file:../../../../Ferramentas/Tools/tools/...`). Nunca crie pacote dentro de `empilha-tudo/Packages` nem código de pacote em `Assets/`.
- **Base** (`FastDrive.Base`): fluxo, perfil, avisos, tema, layout e telas padrão. Nada específico de um jogo entra nela.
- **Jogo** (`Assets/EmpilhaTudo`): só a mecânica, via `GameplayProvider` e `MatchContext`. O jogo não monta telas da sequência nem mexe em placar e tempo. Cada andar é `match.Hit(pontos)`; o desabamento é `match.Miss()` seguido de `match.Finish(...)`, sem esperar o tempo acabar.
- **Pacotes**: implementam `IFastDriveModule`, se registram no `ModuleRegistry` e só conversam por avisos (`SignalBus`). Nunca chamam o jogo nem outro pacote. Ligar um pacote tem dois níveis: instalado no `manifest.json` do jogo e listado em `pacotes` no `perfil.json` do cliente.
- **Clientes** (`Assets/Clientes/<cliente>`): só dados, com os nomes de arquivo fixos do guia visual. As artes do jogo (peças, base, brilho, toque) ficam em `jogo/`. Nenhuma imagem, fonte, texto ou número de um cliente fica no código: o código só tem os padrões, e o cliente troca pelo `perfil.json`, pelo `conteudo/empilha.json` e pelos arquivos da pasta. Cliente não muda posição nem tamanho de nada; isso é projeto sob medida. Pastas de cliente nunca vão para o Git: só a `suricatus` sobe (`Assets/Clientes/.gitignore`), e a cena volta para ela antes de qualquer commit.
- O layout (posição e tamanho) fica em USS com as medidas do guia visual, em px na referência 1080 × 1920 (vertical) ou 1920 × 1080 (horizontal). Cor, fonte, cantos e texturas vêm do tema, aplicados pelo `UiFactory`. Dentro da área do jogo, a torre usa frações do lado da área (que é quadrada), em `TowerView`.
- A sequência é fixa: Atração, Como jogar, Contagem, Gameplay, Resultado, Cadastro\*, Final\*. Não existe "Jogar de novo" (o documento de game design pede, mas a regra do Fast-Drive vale): o Final volta para a Atração.
- Pontuação em pontos, como nos outros jogos: Prêmios e Ranking decidem pela pontuação. A altura aparece no Resultado. A "altura para ganhar prêmio" do documento vira pontos (ver a conta no README).
- Plataformas: totem Windows, totem ou tablet Android, navegador (WebGL, site da Suricatus). Um aparelho por evento. O cliente `suricatus` também é o app da Play Store (teste interno): nele o quiosque não bloqueia a saída nem fixa o app, e o `.aab` sai pelo menu **Fast-Drive > Android** (ver "Publicar na Play Store" no README).

## Convenções

- Identificadores de código em inglês; textos de tela, mensagens de erro, comentários e documentação em português do Brasil.
- Textos de tela ficam em `Texts` (chaves `empilha.*`, com reserva no código) ou no conteúdo do cliente, nunca soltos no código do jogo.
- Lógica do jogo sem dependência de Unity em `Scripts/Runtime/Logic`, com testes EditMode.
- Os padrões (velocidade, largura, encaixe perfeito e as faixas Prata 2000 e Ouro 4000) foram calibrados por simulação de jogadores para a partida de 40 s. Mudou o ritmo ou a duração, refaça a conta e ajuste as faixas e os brindes.
- As artes do cliente Suricatus saem de `ferramentas/artes_cliente_suricatus.py` (a partir de `Assets/Sprites` e do cliente Suricatus do Reflexo). Para trocar uma arte do cliente padrão, mude o script e rode de novo.

## Trabalhando com o editor aberto

O projeto tem o pacote `com.unity.pipeline`: com a Unity aberta, use a CLI `unity` (skill unity-cli) para compilar (`recompile`, `console_status`), rodar testes (`run_tests --filter Suricatus.EmpilhaTudo.Tests --filter_type assembly`), montar a cena (`eval` com `Suricatus.FastDrive.Editor.GameSceneSetup.Setup("suricatus")`) e capturar a tela (`capture_game_view --width 540 --height 960 --save_path Temp/cap/x.png`; o arquivo vai para `Assets/Temp`, apague depois com `AssetDatabase.DeleteAsset("Assets/Temp")`).

Em segundo plano, a Unity só lê arquivos novos depois de `AssetDatabase.Refresh()` (por `eval`) ou quando ganha foco. Com `set_autotick` ligado, o Play mode anda em tempo real mesmo sem foco. Para revisar telas, avance o fluxo chamando `GameHost.Next` com a tela atual (campo privado `current`), tudo no mesmo `eval`. Para a horizontal, `UnityEditor.PlayModeWindow.SetCustomRenderingResolution(1920, 1080, "...")` (e volte para 1080 × 1920 depois).

Um erro de `com.unity.collections` sobre `System.Runtime.CompilerServices.Unsafe.dll` no Console vem do cache de pacotes da Unity e não tem relação com o projeto.
