# Sprites — Empilha Tudo

Artes sem texto, em PNG RGBA, com fontes de geração em `generate_sprites.py`.
Todos os PNGs foram preparados como sprites Unity a 100 pixels por unidade.

## Arquivos

- `background_celestial.png` — fundo vertical 1080 × 1920; preencher a tela.
- `block_*.png` — oito cores de peça (512 × 192); configurar como **Sliced** no Sprite Editor, mantendo borda de 25 px.
- `platform_gold.png` — base da torre (680 × 270).
- `debris_1.png` a `debris_4.png` — fragmentos para quedas (160 × 140).
- `sparkle_precision.png` e `sparkle_white.png` — brilhos para encaixe perfeito e efeitos.
- `tap_indicator.png` — indicação visual de toque (180 × 180).

As peças usam o mesmo tamanho e pivô central para facilitar a troca de cor por rodada.
