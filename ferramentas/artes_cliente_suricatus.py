"""
Monta as artes do cliente padrão (Suricatus) do Empilha Tudo a partir das artes do projeto.

- jogo/peca-1.png a peca-8.png: blocos de Assets/Sprites, recortados no corpo da peça (sombra só embaixo).
- jogo/base.png: plataforma dourada, recortada para a primeira peça encostar no alto da arte.
- jogo/brilho.png e jogo/toque.png: brilho do encaixe perfeito e indicação de toque.
- fundos/fundo-vertical.jpg e fundo-horizontal.jpg: o céu do background_celestial, redesenhado com a
  mistura certa das camadas (no PNG original, nuvens, montanhas e prédios ficaram semitransparentes).
- atracao/arte.png: torre de peças com o suricato no alto.

As artes de marca (logos, botões, fonte e tema) vêm do cliente Suricatus do Reflexo Relâmpago,
que foram feitas a partir das mesmas artes (Assets/Sprites/botoes.png, logo - suricatus.png e Yellow Banana).

Uso: python artes_cliente_suricatus.py  (Pillow 10 ou mais novo)
"""
from pathlib import Path
import random
import shutil

from PIL import Image, ImageDraw, ImageFilter

HERE = Path(__file__).resolve().parent
PROJECT = HERE.parent / "empilha-tudo"
SPRITES = PROJECT / "Assets" / "Sprites"
CLIENT = PROJECT / "Assets" / "Clientes" / "suricatus"
REFLEXO_CLIENT = HERE.parents[1] / "Suricatus - Reflexo Relampago" / "reflexo-relampago" / "Assets" / "Clientes" / "suricatus"

INK = (15, 35, 62, 255)
# Cores das peças (alto, baixo), na ordem de block_coral até block_berry do generate_sprites.py.
BLOCKS = [
    ((255, 139, 110), (205, 65, 63)),
    ((255, 193, 105), (229, 126, 50)),
    ((255, 226, 126), (235, 169, 58)),
    ((119, 230, 176), (43, 168, 125)),
    ((113, 218, 237), (42, 148, 190)),
    ((120, 178, 255), (63, 103, 204)),
    ((191, 157, 255), (120, 91, 204)),
    ((255, 141, 190), (203, 76, 137)),
]
# Corpo da peça (512 × 192): o contorno vai de x 18 a 494 e de y 8 a 166; abaixo é sombra.
BLOCK_BODY = (18, 8, 494, 192)
# Plataforma (680 × 270): do alto do contorno dourado (y 45) até o fim da sombra, na largura da parte azul.
PLATFORM_BODY = (28, 45, 652, 243)
SS = 2  # supersampling dos fundos
BS = 4  # supersampling das peças e da base


def out(relative):
    path = CLIENT / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    return path


def copy_brand():
    for relative in ["tema.json", "fontes/titulo.otf", "marca/logo-horizontal.png", "marca/logo-quadrado.png",
                     "botoes/botao-principal.png", "botoes/botao-principal-pressionado.png",
                     "botoes/botao-secundario.png", "botoes/botao-secundario-pressionado.png", "botoes/botao-sair.png"]:
        shutil.copyfile(REFLEXO_CLIENT / relative, out(relative))


def s(*values):
    return tuple(round(v * BS) for v in values)


def over(image, draw):
    """Desenha numa camada à parte e mistura sobre a imagem (desenhar direto troca os pixels, sem misturar)."""
    top, d = layer(image.size)
    draw(d)
    image.alpha_composite(top)


def block(top, bottom):
    """O mesmo desenho do block() de Assets/Sprites/generate_sprites.py, com as camadas misturadas."""
    image = Image.new("RGBA", s(512, 192), (0, 0, 0, 0))
    shadow, d = layer(image.size)
    d.rounded_rectangle(s(25, 34, 495, 181), radius=25 * BS, fill=(3, 14, 32, 90))
    image.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(7 * BS)))

    mask = Image.new("L", image.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle(s(18, 8, 494, 166), radius=25 * BS, fill=255)
    gradient = Image.new("RGB", (1, image.height))
    for y in range(image.height):
        t = y / (image.height - 1)
        gradient.putpixel((0, y), tuple(round(top[i] * (1 - t) + bottom[i] * t) for i in range(3)))
    body = Image.new("RGBA", image.size, (0, 0, 0, 0))
    body.paste(gradient.resize(image.size), (0, 0), mask)
    image.alpha_composite(body)

    x0, y0, x1, _ = 18, 8, 494, 166
    over(image, lambda d: d.rounded_rectangle(s(18, 8, 494, 166), radius=25 * BS, outline=INK, width=8 * BS))
    over(image, lambda d: (
        d.rounded_rectangle(s(x0 + 15, y0 + 12, x1 - 15, y0 + 28), radius=8 * BS, fill=(255, 255, 255, 80)),
        d.ellipse(s(x0 + 22, y0 + 34, x0 + 54, y0 + 48), fill=(255, 255, 255, 95)),
        d.line(s(34, 153, 478, 153), fill=(255, 255, 255, 70), width=3 * BS)))
    return image.resize((512, 192), Image.Resampling.LANCZOS).crop(BLOCK_BODY)


def platform():
    """O mesmo desenho do platform() de Assets/Sprites/generate_sprites.py, com as camadas misturadas."""
    image = Image.new("RGBA", s(680, 270), (0, 0, 0, 0))
    over(image, lambda d: d.rounded_rectangle(s(34, 130, 646, 240), radius=30 * BS, fill=(2, 17, 37, 75)))
    over(image, lambda d: (
        d.rounded_rectangle(s(28, 62, 652, 220), radius=32 * BS, fill=(13, 35, 62, 255), outline=INK, width=8 * BS),
        d.rounded_rectangle(s(40, 72, 640, 190), radius=25 * BS, fill=(35, 78, 108, 255), outline=(23, 54, 84, 255), width=5 * BS),
        d.rounded_rectangle(s(40, 45, 640, 145), radius=26 * BS, fill=(255, 188, 91, 255), outline=INK, width=8 * BS)))
    over(image, lambda d: (
        d.rounded_rectangle(s(54, 54, 626, 80), radius=12 * BS, fill=(255, 231, 173, 160)),
        d.ellipse(s(72, 89, 110, 106), fill=(255, 255, 255, 105))))
    return image.resize((680, 270), Image.Resampling.LANCZOS).crop(PLATFORM_BODY)


def game_arts():
    for index, (top, bottom) in enumerate(BLOCKS, 1):
        block(top, bottom).save(out(f"jogo/peca-{index}.png"))
    platform().save(out("jogo/base.png"))
    shutil.copyfile(SPRITES / "sparkle_precision.png", out("jogo/brilho.png"))
    shutil.copyfile(SPRITES / "tap_indicator.png", out("jogo/toque.png"))


def layer(size):
    image = Image.new("RGBA", size, (0, 0, 0, 0))
    return image, ImageDraw.Draw(image)


def sky(width, height):
    """O céu do background_celestial em qualquer proporção, cada camada misturada sobre a de baixo."""
    w, h = width * SS, height * SS
    sx, sy = width / 1080, height / 1920
    stops = [(0, (15, 36, 76)), (.48, (45, 112, 160)), (.76, (117, 187, 198)), (1, (247, 207, 144))]
    gradient = Image.new("RGB", (1, h))
    for y in range(h):
        t = y / max(1, h - 1)
        for (a, c0), (b, c1) in zip(stops, stops[1:]):
            if a <= t <= b:
                q = (t - a) / (b - a)
                gradient.putpixel((0, y), tuple(round(c0[k] * (1 - q) + c1[k] * q) for k in range(3)))
                break
    image = gradient.resize((w, h)).convert("RGBA")

    def px(x, y):
        return int(x * sx * SS), int(y * sy * SS)

    clouds, d = layer((w, h))
    for x, y, rx, ry in [(130, 250, 105, 30), (850, 390, 150, 38), (330, 590, 82, 24), (890, 140, 75, 25)]:
        (cx, cy), r = px(x, y), (rx * sx * SS, ry * sx * SS)
        d.ellipse((cx - r[0], cy - r[1], cx + r[0], cy + r[1]), fill=(255, 244, 210, 24))
    random.seed(41)
    for _ in range(int(45 * sx)):
        x = random.randrange(w)
        y = random.randrange(int(50 * sy * SS), int(950 * sy * SS))
        r = random.choice([2, 3, 4]) * SS
        d.ellipse((x - r, y - r, x + r, y + r), fill=(255, 248, 220, random.randrange(45, 125)))
    image.alpha_composite(clouds)

    for points, color in [
        ([(0, 1310), (170, 1020), (350, 1290), (545, 970), (790, 1310), (980, 1085), (1080, 1210), (1080, 1920), (0, 1920)], (36, 91, 116, 170)),
        ([(0, 1450), (245, 1215), (450, 1420), (680, 1170), (920, 1415), (1080, 1260), (1080, 1920), (0, 1920)], (22, 65, 92, 185)),
    ]:
        mountains, d = layer((w, h))
        d.polygon([px(x, y) for x, y in points], fill=color)
        image.alpha_composite(mountains)

    # Os prédios mantêm a largura do desenho original; na horizontal, a fileira se repete.
    city, d = layer((w, h))
    buildings = [(0, 1510, 100, 190), (100, 1440, 118, 260), (232, 1515, 115, 185), (362, 1385, 150, 315),
                 (527, 1490, 100, 210), (635, 1420, 130, 280), (782, 1510, 125, 190), (920, 1390, 160, 310)]
    k = height / 1920 if width < height else 0.75
    offset = 0
    while offset < width:
        for x, y, bw, bh in buildings:
            left, right = (offset + x * k) * SS, (offset + (x + bw) * k) * SS
            bottom = height * SS
            top = bottom - (1920 - y) * k * SS
            d.rounded_rectangle((left, top, right, bottom), radius=12 * k * SS, fill=(13, 48, 75, 150))
            for wx in range(int(x + 20), int(x + bw - 10), 35):
                for wy in range(int(y + 30), int(y + bh - 12), 42):
                    wl, wt = (offset + wx * k) * SS, bottom - (1920 - wy) * k * SS
                    d.rounded_rectangle((wl, wt, wl + 9 * k * SS, wt + 14 * k * SS), radius=3 * k * SS, fill=(255, 205, 124, 110))
        offset += 1080 * k
    image.alpha_composite(city)
    return image.resize((width, height), Image.Resampling.LANCZOS).convert("RGB")


def backgrounds():
    sky(1080, 1920).save(out("fundos/fundo-vertical.jpg"), quality=90)
    sky(1920, 1080).save(out("fundos/fundo-horizontal.jpg"), quality=90)


def sliced(art, width, height):
    """A peça na largura pedida: as pontas (um quarto da altura de cada lado) não esticam, como no jogo."""
    scale = height / art.height
    art = art.resize((max(1, round(art.width * scale)), height), Image.Resampling.LANCZOS)
    cap = round(height / 4)
    piece = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    piece.paste(art.crop((0, 0, cap, height)), (0, 0))
    piece.paste(art.crop((cap, 0, art.width - cap, height)).resize((width - 2 * cap, height)), (cap, 0))
    piece.paste(art.crop((art.width - cap, 0, art.width, height)), (width - cap, 0))
    return piece


def tower(image, center, base_width, piece_height, pieces, first_color=0):
    """Uma torre sobre a base, com o pé no fundo da imagem. Devolve a altura (y) do alto da torre.
    pieces: (deslocamento do centro, largura) de cada andar, de baixo para cima."""
    base = Image.open(CLIENT / "jogo" / "base.png")
    base = base.resize((base_width, round(base.height * base_width / base.width)), Image.Resampling.LANCZOS)
    bottom = image.height - base.height - 6
    image.alpha_composite(base, (center - base_width // 2, bottom))
    step = round(piece_height * 0.86)
    for index, (offset, width) in enumerate(pieces):
        art = Image.open(CLIENT / "jogo" / f"peca-{(first_color + index) % len(BLOCKS) + 1}.png")
        image.alpha_composite(sliced(art, width, piece_height), (center + offset - width // 2, bottom - piece_height))
        bottom -= step
    return bottom - (piece_height - step)


def attract_art():
    """
    A Base mostra a arte da Atração preenchendo a área e cortando embaixo: 952 × 839 na vertical e 1120 × 470
    na horizontal. Em 1120 × 840, a vertical mostra a torre inteira (perde só as laterais) e a horizontal, o suricato
    e o alto das três torres. Fundo transparente.
    """
    width, height = 1120, 840
    image = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    # Torres dos lados, mais baixas, com o alto à vista na horizontal.
    tower(image, 235, 250, 52, [(-8, 210), (4, 196), (-4, 186), (8, 172), (2, 166), (10, 152), (4, 146), (12, 136), (8, 130)], 4)
    tower(image, 885, 250, 52, [(6, 210), (-6, 198), (2, 188), (-10, 174), (-4, 166), (-12, 152), (-8, 144)], 2)
    # Torre do meio, com o suricato no alto.
    top = tower(image, 560, 470, 70, [(-14, 400), (-4, 380), (-10, 364), (4, 344), (-2, 330), (8, 312)])
    meerkat = Image.open(REFLEXO_CLIENT / "jogo" / "alvo-3.png")
    meerkat = meerkat.crop(meerkat.getbbox())
    meerkat = meerkat.resize((round(meerkat.width * 330 / meerkat.height), 330), Image.Resampling.LANCZOS)
    image.alpha_composite(meerkat, (568 - meerkat.width // 2, top - meerkat.height + 22))

    # Um pedaço cortado caindo, à direita da torre do meio.
    falling = sliced(Image.open(CLIENT / "jogo" / "peca-7.png"), 120, 70).rotate(-28, expand=True, resample=Image.Resampling.BICUBIC)
    image.alpha_composite(falling, (760, 300))

    sparkle = Image.open(CLIENT / "jogo" / "brilho.png")
    for x, y, size in [(330, 120, 72), (760, 60, 88), (130, 300, 44), (960, 250, 52), (395, 250, 40)]:
        image.alpha_composite(sparkle.resize((size, size), Image.Resampling.LANCZOS), (x, y))
    image.save(out("atracao/arte.png"))


if __name__ == "__main__":
    copy_brand()
    game_arts()
    backgrounds()
    attract_art()
    print("Artes do cliente Suricatus em", CLIENT)
