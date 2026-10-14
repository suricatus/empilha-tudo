# Empilha Tudo · Fast-Drive

Terceiro jogo do Fast-Drive Game. Uma peça vai e volta sobre a torre, e a pessoa toca na tela para soltá-la: o que fica sobre a peça de baixo vira um andar, e o que passa da borda cai. A torre fica mais estreita a cada erro, e as peças, mais rápidas a cada andar. A mecânica é a única parte deste projeto escrita para ele; telas, placar, tempo e pacotes vêm da Base e dos pacotes do Fast-Drive, que ficam no repositório [Tools](https://github.com/suricatus/Tools) (`Ferramentas/Tools/tools/FastDrive.*`).

O cliente padrão é a própria Suricatus: é a versão de demonstração para o site e para reuniões.

## Abrir e jogar

1. Clone este repositório e o Tools lado a lado, mantendo as pastas `GitHub/Suricatus/Jogos - Prateleira/` e `GitHub/Suricatus/Ferramentas/Tools/`: o jogo encontra os pacotes por caminho relativo.
2. Abra `empilha-tudo` na Unity 6000.3.5f2.
3. Abra `Assets/Scenes/EmpilhaTudo.unity` e aperte Play.
4. No Game view, use 1080 × 1920 para o totem e o celular, ou 1920 × 1080 para o tablet, o computador e o site. O layout troca sozinho com a proporção.

Para recriar a cena do zero: **Fast-Drive > Montar cena do jogo**.

## Regras do jogo

- A partida dura até 40 segundos (`tempos.partida` no `perfil.json`) e começa depois do "3, 2, 1, Já!".
- A primeira peça tem a largura da base e vai de uma borda à outra da área do jogo. Um toque em qualquer lugar da área do jogo ou da área de apoio solta a peça.
- O que fica sobre a peça de baixo vira um andar; o que passa da borda cai. A próxima peça tem a largura do andar novo e sai do outro lado.
- Peça alinhada com a de baixo (até `encaixePerfeito` de distância) é um **encaixe perfeito**: ela encaixa inteira, sem cortar nada, e vale bônus.
- Peça com menos de `apoioMinimo` da largura apoiada (ou mais estreita que 4% da área) não fica de pé: **a torre desaba** e a partida termina na hora, com a pontuação que já tinha.
- A cada andar a peça fica um pouco mais rápida, até a velocidade máxima.
- A área de apoio mostra a próxima faixa com a barra de progresso, os andares, os encaixes perfeitos e a legenda do toque.

### Pontuação

| Jogada | Pontos |
|---|---|
| Andar encaixado | 100 |
| Encaixe perfeito | 100 + bônus de 50 |
| Desabamento | 0 (a partida termina) |

### Resultado e faixas

O Resultado mostra a pontuação, a faixa ("Medalha de Ouro") e a altura e os perfeitos ("Altura: 18 · Perfeitos: 6").

| Faixa | Pontos | Na tela |
|---|---|---|
| Medalha de Bronze | 0 | "Quase lá!" |
| Medalha de Prata | 2000 | "Você venceu!", com confete e som de vitória |
| Medalha de Ouro | 4000 | "Você venceu!", com confete e som de vitória |

As faixas foram calibradas para a partida de 40 s (simulação de jogadores): quem joga na média faz perto de 17 andares e chega à Prata; quem é preciso passa de 30 andares e chega ao Ouro. A partida dura de 30 a 35 s em média, porque quase sempre termina pelo desabamento. **Mudou a velocidade, a largura ou a duração, ajuste as faixas e os brindes.**

### Altura para ganhar prêmio

O Prêmios e o Ranking decidem pela pontuação, como nos outros jogos. Para pedir uma altura, converta em pontos: **andares × `pontosPorAndar`**. Os perfeitos somam bônus, então quem é preciso chega um pouco antes.

| Altura pedida | Pontos (sem bônus) |
|---|---|
| 10 andares | 1000 |
| 15 andares | 1500 |
| 20 andares | 2000 |
| 30 andares | 3000 |

Use o mesmo número nas `faixas` do `conteudo/empilha.json` e em `premios.faixas` do `perfil.json`.

## Pacotes

Um pacote é ligado em dois níveis:

1. **No jogo** (`empilha-tudo/Packages/manifest.json`): o pacote está instalado e entra na build. Sem ele no manifesto, nenhum cliente deste jogo pode usá-lo.
2. **No cliente** (`perfil.json`, lista `pacotes`): o pacote funciona para aquele cliente. Instalado e fora da lista, ele não faz nada.

| Pacote | No jogo | Cliente Suricatus |
|---|---|---|
| `com.suricatusgames.fastdrive.base` | Sempre (é a Base, 0.9.0 ou mais nova) | — |
| `com.suricatusgames.fastdrive.efeitos` | Instalado | Ligado (`"efeitos"`): faíscas e "+100" a cada andar, moldura no desabamento e nos segundos finais, papel picado na Prata e no Ouro |
| `com.suricatusgames.fastdrive.inatividade` | Instalado | Ligado (`"inatividade"`): volta para a Atração quando a pessoa sai |
| `com.suricatusgames.fastdrive.quiosque` | Instalado | Ligado (`"quiosque"`); no navegador fica parado |
| `com.suricatusgames.fastdrive.sons` | Instalado | Ligado (`"sons"`): efeitos padrão, sem música |
| `com.suricatusgames.fastdrive.teclado` | Instalado (usado pelo Cadastro) | — |
| `com.suricatusgames.fastdrive.cadastro` | Instalado | Ligado (`"cadastro"`): nome, e-mail e telefone |
| `com.suricatusgames.fastdrive.metricas` | Instalado | Ligado (`"metricas"`): acertos são os andares |
| `com.suricatusgames.fastdrive.painel` | Instalado | Ligado (`"painel"`), senha de demonstração `2468`: 5 toques rápidos no canto de cima à esquerda da Atração |
| `com.suricatusgames.fastdrive.premios` | Instalado | Ligado (`"premios"`): adesivo e chaveiro a partir de 2000 pontos (Prata), caneca a partir de 4000 (Ouro) |
| `com.suricatusgames.fastdrive.ranking` | Instalado | Ligado (`"ranking"`): ranking do dia, 10 posições na Atração |
| `com.suricatusgames.fastdrive.servidor` | Instalado (usado pelo Cadastro, Prêmios e Métricas) | — |

Com o Modo quiosque ligado, a build de totem abre em tela cheia e não fecha com Alt+F4: para sair, **Ctrl+Shift+Q**.

Leads, brindes e métricas vão para o Supabase da Suricatus, o mesmo de todos os jogos, com `jogo: "empilha-tudo"`. O endereço, a chave publishable e a chave da fila deste jogo ficam em `empilha-tudo/Assets/FastDrive/Resources/FastDriveServidor.json`, iguais para todos os clientes do jogo (ver o README do pacote Servidor).

## Novo cliente

1. Copie `empilha-tudo/Assets/Clientes/suricatus` para `empilha-tudo/Assets/Clientes/<cliente-evento-ano>`.
2. Troque as imagens, as fontes, o `tema.json`, o `perfil.json`, as artes de `jogo/` e o `conteudo/empilha.json`, mantendo os nomes de arquivo (ver o guia visual e as tabelas abaixo).
3. **Fast-Drive > Clientes > Atualizar clientes**.
4. Selecione o `cliente.asset` da pasta nova e use **Fast-Drive > Clientes > Usar cliente selecionado**. O Console mostra se falta algum arquivo ou se alguma cor tem contraste baixo.
5. Gere a build para a plataforma do evento.
6. Volte a cena para o cliente `suricatus`.

A pasta do cliente **nunca vai para o Git**: o `.gitignore` de `Assets/Clientes` só deixa subir a `suricatus`. Guarde as pastas de cliente fora do repositório.

### Artes do jogo (`jogo/`)

| Arquivo | O que é | Sem o arquivo |
|---|---|---|
| `peca.png` ou `peca-1.png`, `peca-2.png`… | A peça vista de frente. Com várias, cada andar usa a seguinte, em ordem | Blocos arredondados nas cores do tema |
| `base.png` | A base onde a torre começa. Aparece com a largura da primeira peça, na proporção da arte | Uma laje na cor `secundaria` do tema |
| `brilho.png` | Brilho nas pontas da peça no encaixe perfeito | Só o "Perfeito!" |
| `toque.png` | Mão de "toque aqui" acima da primeira peça e na legenda | Só o texto da legenda |

Regras das artes, para o encaixe ficar certo na tela:

- **Peça**: PNG com fundo transparente, cerca de 476 × 184 px. O corpo da peça ocupa toda a largura e vai do alto da imagem até 86% da altura; os 14% de baixo são sombra (ou transparentes). A arte estica só no meio: as pontas, um quarto da altura de cada lado, ficam como estão, então cantos arredondados e brilhos devem caber nelas.
- **Base**: PNG com fundo transparente. A primeira peça encosta no alto da imagem, então a arte não pode ter margem transparente em cima; a largura da arte é a largura da primeira peça.
- **Brilho** e **toque**: quadrados, com fundo transparente.

A arte da Atração (`atracao/arte.png`, do guia visual) é mostrada preenchendo a área e cortando embaixo: a área tem 952 × 839 px na vertical e 1120 × 470 px na horizontal. Uma arte de 1120 × 840 px aparece inteira na vertical (perde só as laterais) e, na horizontal, mostra a parte de cima: deixe o mais importante no alto e no centro.

No cliente Suricatus, as peças são os 8 blocos coloridos, a base é a plataforma dourada e o fundo é o céu com a cidade. Essas artes saem de `ferramentas/artes_cliente_suricatus.py`, a partir de `empilha-tudo/Assets/Sprites` (com as camadas de brilho e sombra misturadas do jeito certo; no `generate_sprites.py` original elas ficaram transparentes).

### `conteudo/empilha.json`

Tudo é opcional; o que faltar fica com o padrão. Larguras e distâncias são em fração da largura da área do jogo (1 = a área inteira).

| Campo | O que é | Padrão |
|---|---|---|
| `larguraInicial` | Largura da primeira peça e da base (0,3 a 0,8) | 0,65 |
| `velocidade.inicial` | Velocidade da peça no primeiro andar, em larguras da área por segundo (0,1 a 3). 0,4 = a peça anda 40% da área por segundo | 0,4 |
| `velocidade.porAndar` | Quanto a velocidade aumenta a cada andar (0 a 0,5) | 0,015 |
| `velocidade.maxima` | Velocidade máxima (nunca menor que a inicial) | 1,0 |
| `encaixePerfeito` | Distância até o alinhamento que ainda conta como encaixe perfeito (0 a 0,1). 0,02 é perto de 18 px no totem | 0,02 |
| `apoioMinimo` | Fração da peça que precisa ficar apoiada; menos que isso, a torre desaba (0 a 0,9) | 0,2 |
| `pontosPorAndar` | Pontos de cada andar | 100 |
| `bonusPerfeito` | Bônus do encaixe perfeito | 50 |
| `faixas` | Lista de `{ "nome", "pontos", "vitoria" }`. O nome aparece no Resultado e na área de apoio. Sem nenhum `vitoria`, todas menos a mais baixa contam como vitória | Bronze 0, Prata 2000, Ouro 4000 |
| `comoJogar` | Até 3 passos, com até 8 palavras cada | "Toque na tela para soltar a peça", "O que ficar para fora cai", "Encaixe perfeito vale pontos extras" |

### Textos

Trocáveis em `textos` no `perfil.json`:

| Chave | Padrão |
|---|---|
| `empilha.titulo` | Empilha Tudo |
| `empilha.subtitulo` | Empilhe as peças e construa a torre mais alta! |
| `empilha.andares`, `empilha.perfeitos` | Andares, Perfeitos |
| `empilha.perfeito` | Perfeito! (com "×2", "×3"… nos perfeitos seguidos) |
| `empilha.proximaMeta`, `empilha.metaFinal` | Próxima meta, Faixa máxima! |
| `empilha.toque` | Toque na tela para soltar a peça |
| `empilha.resultado` | Altura: {andares} · Perfeitos: {perfeitos} |

## Testes

Window > General > Test Runner > EditMode: 22 testes do Empilha Tudo (conteúdo e limites, faixas, partida: base, movimento e rebote, encaixe perfeito, corte dos dois lados, desabamento por pouco apoio e por largura mínima, toque entre peças, fim do tempo, velocidade e variações das peças), mais os testes dos pacotes instalados (235 no total).

## Estrutura

```text
empilha-tudo/
  Packages/manifest.json                   pacotes instalados no jogo
  Assets/EmpilhaTudo/                      só a mecânica do jogo
    Scripts/Runtime/Logic/                 conteúdo e partida, sem Unity
    Scripts/Runtime/View/                  torre, painel de apoio e desenho das peças (UI Toolkit)
    Resources/EmpilhaTudo.uss              estilos próprios do jogo
    Tests/EditMode/
  Assets/Clientes/<cliente>/               uma pasta por cliente, nomes fixos do guia visual
  Assets/FastDrive/                        PanelSettings, tema da interface e configuração do Servidor
  Assets/Scenes/EmpilhaTudo.unity
ferramentas/artes_cliente_suricatus.py     gera as artes do cliente Suricatus
```
