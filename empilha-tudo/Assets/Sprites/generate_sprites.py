from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter
import math, random, uuid

OUT = Path(__file__).parent
S = 4
INK = (15, 35, 62, 255)

def scale(v): return int(round(v * S))

def rounded_gradient(size, box, radius, top, bottom, outline=INK, width=8, shine=True):
    w,h=size
    mask=Image.new('L',(scale(w),scale(h)),0)
    md=ImageDraw.Draw(mask)
    b=tuple(scale(v) for v in box)
    md.rounded_rectangle(b,radius=scale(radius),fill=255)
    grad=Image.new('RGBA',mask.size)
    pix=grad.load()
    for y in range(grad.height):
        t=y/max(1,grad.height-1)
        c=tuple(round(top[i]*(1-t)+bottom[i]*t) for i in range(3))+(255,)
        for x in range(grad.width): pix[x,y]=c
    out=Image.new('RGBA',mask.size,(0,0,0,0)); out.paste(grad,(0,0),mask)
    d=ImageDraw.Draw(out)
    d.rounded_rectangle(b,radius=scale(radius),outline=outline,width=scale(width))
    if shine:
        x0,y0,x1,y1=b
        d.rounded_rectangle((x0+scale(15),y0+scale(12),x1-scale(15),y0+scale(28)),radius=scale(8),fill=(255,255,255,80))
        d.ellipse((x0+scale(22),y0+scale(34),x0+scale(54),y0+scale(48)),fill=(255,255,255,95))
    return out

def save(im,name,ppu=100):
    im=im.resize((im.width//S,im.height//S),Image.Resampling.LANCZOS)
    path=OUT/(name+'.png'); im.save(path)
    guid=uuid.uuid4().hex
    meta=f'''fileFormatVersion: 2\nguid: {guid}\nTextureImporter:\n  internalIDToNameTable: []\n  externalObjects: {{}}\n  serializedVersion: 13\n  mipmaps:\n    mipMapMode: 0\n    enableMipMap: 0\n  isReadable: 0\n  streamingMipmaps: 0\n  textureFormat: 1\n  maxTextureSize: 2048\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 1\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  nPOTScale: 0\n  sRGBTexture: 1\n  alphaSource: 1\n  alphaIsTransparency: 1\n  textureType: 8\n  textureShape: 1\n  singleChannelComponent: 0\n  flipbookRows: 1\n  flipbookColumns: 1\n  maxTextureSizeSet: 0\n  compressionQuality: 50\n  textureCompression: 0\n  spriteMode: 1\n  spriteExtrude: 1\n  spriteMeshType: 1\n  alignment: 0\n  spritePivot: {{x: 0.5, y: 0.5}}\n  spritePixelsTo保Units: {ppu}\n  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}\n  spriteGenerateFallbackPhysicsShape: 1\n  alphaUsage: 1\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'''.replace('spritePixelsTo保Units','spritePixelsToUnits')
    (OUT/(name+'.png.meta')).write_text(meta,encoding='utf-8')

def block(name,top,bottom):
    im=Image.new('RGBA',(scale(512),scale(192)),(0,0,0,0))
    # soft grounding shadow
    sh=Image.new('RGBA',im.size,(0,0,0,0)); sd=ImageDraw.Draw(sh)
    sd.rounded_rectangle((scale(25),scale(34),scale(495),scale(181)),radius=scale(25),fill=(3,14,32,90))
    im.alpha_composite(sh.filter(ImageFilter.GaussianBlur(scale(7))))
    base=rounded_gradient((512,192),(18,8,494,166),25,top,bottom,width=8)
    im.alpha_composite(base)
    d=ImageDraw.Draw(im)
    d.line((scale(34),scale(153),scale(478),scale(153)),fill=(255,255,255,70),width=scale(3))
    save(im,name,100)

def background():
    w,h=1080,1920
    im=Image.new('RGBA',(w*S,h*S)); p=im.load()
    stops=[(0,(15,36,76)),(.48,(45,112,160)),(.76,(117,187,198)),(1,(247,207,144))]
    for y in range(im.height):
        t=y/max(1,im.height-1)
        for j in range(len(stops)-1):
            if stops[j][0]<=t<=stops[j+1][0]:
                a,c0=stops[j]; b,c1=stops[j+1]; q=(t-a)/(b-a); break
        col=tuple(round(c0[k]*(1-q)+c1[k]*q) for k in range(3))+(255,)
        for x in range(im.width): p[x,y]=col
    d=ImageDraw.Draw(im,'RGBA')
    # distant soft clouds and tiny stars
    for x,y,rx,ry in [(130,250,105,30),(850,390,150,38),(330,590,82,24),(890,140,75,25)]:
        x*=S;y*=S;rx*=S;ry*=S
        d.ellipse((x-rx,y-ry,x+rx,y+ry),fill=(255,244,210,24))
    random.seed(41)
    for _ in range(45):
        x=random.randrange(w*S); y=random.randrange(50*S,950*S); r=random.choice([2,3,4])*S
        d.ellipse((x-r,y-r,x+r,y+r),fill=(255,248,220,random.randrange(45,125)))
    # layered rounded mountain silhouettes
    d.polygon([(0,1310*S),(170*S,1020*S),(350*S,1290*S),(545*S,970*S),(790*S,1310*S),(980*S,1085*S),(1080*S,1210*S),(1080*S,1920*S),(0,1920*S)],fill=(36,91,116,170))
    d.polygon([(0,1450*S),(245*S,1215*S),(450*S,1420*S),(680*S,1170*S),(920*S,1415*S),(1080*S,1260*S),(1080*S,1920*S),(0,1920*S)],fill=(22,65,92,185))
    # lower distant skyline hints
    for x,y,bw,bh in [(0,1510,100,190),(100,1440,118,260),(232,1515,115,185),(362,1385,150,315),(527,1490,100,210),(635,1420,130,280),(782,1510,125,190),(920,1390,160,310)]:
        d.rounded_rectangle((x*S,y*S,(x+bw)*S,(y+bh)*S),radius=12*S,fill=(13,48,75,150))
        for wx in range(x+20,x+bw-10,35):
            for wy in range(y+30,y+bh-12,42): d.rounded_rectangle((wx*S,wy*S,(wx+9)*S,(wy+14)*S),radius=3*S,fill=(255,205,124,110))
    save(im,'background_celestial',100)

def platform():
    im=Image.new('RGBA',(scale(680),scale(270)),(0,0,0,0))
    d=ImageDraw.Draw(im)
    d.rounded_rectangle((scale(34),scale(130),scale(646),scale(240)),radius=scale(30),fill=(2,17,37,75))
    # pedestal bevel and front face
    d.rounded_rectangle((scale(28),scale(62),scale(652),scale(220)),radius=scale(32),fill=(13,35,62,255),outline=INK,width=scale(8))
    d.rounded_rectangle((scale(40),scale(72),scale(640),scale(190)),radius=scale(25),fill=(35,78,108,255),outline=(23,54,84,255),width=scale(5))
    d.rounded_rectangle((scale(40),scale(45),scale(640),scale(145)),radius=scale(26),fill=(255,188,91,255),outline=INK,width=scale(8))
    d.rounded_rectangle((scale(54),scale(54),scale(626),scale(80)),radius=scale(12),fill=(255,231,173,160))
    d.ellipse((scale(72),scale(89),scale(110),scale(106)),fill=(255,255,255,105))
    save(im,'platform_gold',100)

def sparkle(name,color):
    im=Image.new('RGBA',(scale(128),scale(128)),(0,0,0,0)); d=ImageDraw.Draw(im)
    pts=[(64,5),(77,48),(123,64),(77,78),(64,123),(49,78),(5,64),(49,48)]
    pts=[(scale(x),scale(y)) for x,y in pts]
    d.polygon(pts,fill=color,outline=(255,255,255,240),width=scale(3))
    d.ellipse((scale(56),scale(56),scale(72),scale(72)),fill=(255,255,255,245))
    save(im,name,100)

def tap_indicator():
    im=Image.new('RGBA',(scale(180),scale(180)),(0,0,0,0)); d=ImageDraw.Draw(im)
    d.ellipse((scale(15),scale(15),scale(165),scale(165)),outline=(255,255,255,90),width=scale(5))
    d.ellipse((scale(32),scale(32),scale(148),scale(148)),outline=(255,255,255,155),width=scale(4))
    d.rounded_rectangle((scale(78),scale(45),scale(108),scale(105)),radius=scale(15),fill=(255,255,255,255),outline=INK,width=scale(5))
    d.rounded_rectangle((scale(53),scale(78),scale(125),scale(131)),radius=scale(23),fill=(255,255,255,255),outline=INK,width=scale(5))
    d.line((scale(83),scale(50),scale(83),scale(75)),fill=(210,231,246,255),width=scale(4))
    save(im,'tap_indicator',100)

def debris():
    colors=[(255,117,88),(65,188,222),(255,194,82),(143,117,232)]
    for idx,col in enumerate(colors,1):
        im=Image.new('RGBA',(scale(160),scale(140)),(0,0,0,0));d=ImageDraw.Draw(im)
        pts=[(16,23),(132,8),(151,43),(138,111),(84,128),(22,106),(7,61)]
        pts=[(scale(x),scale(y)) for x,y in pts]
        d.polygon([(x+scale(3),y+scale(5)) for x,y in pts],fill=(4,20,43,70))
        d.polygon(pts,fill=col+(255,),outline=INK,width=scale(7))
        d.line((scale(28),scale(35),scale(118),scale(26)),fill=(255,255,255,120),width=scale(5))
        save(im,f'debris_{idx}',100)

background()
block('block_coral',(255,139,110),(205,65,63))
block('block_tangerine',(255,193,105),(229,126,50))
block('block_sunshine',(255,226,126),(235,169,58))
block('block_mint',(119,230,176),(43,168,125))
block('block_aqua',(113,218,237),(42,148,190))
block('block_sky',(120,178,255),(63,103,204))
block('block_lilac',(191,157,255),(120,91,204))
block('block_berry',(255,141,190),(203,76,137))
platform()
sparkle('sparkle_precision',(255,230,137,255))
sparkle('sparkle_white',(255,255,255,255))
tap_indicator()
debris()
