# Ícone do Empilha Tudo

- `empilha-tudo-icone.png`: imagem quadrada de 1024 × 1024 px, para a Play Store, o Windows e outros usos digitais.
- `empilha-tudo-icone-fundo.png` e `empilha-tudo-icone-frente.png`: as duas camadas do ícone adaptativo do Android. O fundo é só o céu, porque o aparelho estica e corta as pontas; a frente tem a torre dentro dos 66% do meio, que é a parte que nenhuma máscara corta.
- `empilha-tudo-icone.ico`: ícone do Windows com versões de 256, 128, 64, 48, 32 e 16 px.
- `criar-icone.ps1`: recria os quatro arquivos com o suricato e as peças do jogo em `Assets/Clientes/suricatus` e copia as três artes do Android para `Assets/Sprites`, de onde o Player Settings as lê.

O ícone não contém nome nem identidade de cliente. Para atualizar as artes, rode `./Branding/criar-icone.ps1` na pasta do projeto Unity e, se o nome dos arquivos mudar, aponte de novo em **Player Settings > Icon**.

A arte da ficha da loja (512 × 512, destaque 1024 × 500 e capturas de tela) fica no Play Console, não aqui.
