import os
import math
from PIL import Image, ImageDraw, ImageFilter

base_dir = "/home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Content/Trails"

def create_glow_texture(size, color, output_path, blur_radius=3):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    max_r = size // 2
    for r in range(max_r, 0, -1):
        alpha = int(255 * (1 - r / max_r) ** 2)
        c = (color[0], color[1], color[2], alpha)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=c)
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_streak_texture(width, height, color, output_path, blur_radius=2):
    img = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = width // 2, height // 2
    for x in range(width):
        dist_from_center = abs(x - cx) / cx
        alpha = int(255 * (1 - dist_from_center) ** 3)
        for y in range(height):
            dy = abs(y - cy) / cy
            if dy < 0.5:
                a = int(alpha * (1 - dy * 2))
                c = (color[0], color[1], color[2], a)
                draw.point((x, y), fill=c)
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_shard_texture(size, color, output_path, blur_radius=1):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    points = [(cx, 0), (size - 1, cy), (cx, size - 1), (0, cy)]
    draw.polygon(points, fill=(color[0], color[1], color[2], 255))
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_ring_texture(size, color, output_path, blur_radius=2):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    for r in range(size // 4, size // 2):
        alpha = int(255 * (1 - abs(r - size * 0.375) / (size * 0.125)))
        c = (color[0], color[1], color[2], alpha)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=c, width=2)
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_note_texture(size, color, output_path):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    r = size // 4
    draw.ellipse([cx - r, cy - r // 2, cx + r, cy + r // 2], fill=(color[0], color[1], color[2], 200))
    draw.rectangle([cx + r - 2, cy - r, cx + r + 2, cy + r], fill=(color[0], color[1], color[2], 200))
    img = img.filter(ImageFilter.GaussianBlur(radius=1))
    img.save(output_path)

def create_rune_texture(size, color, output_path):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    s = size // 3
    draw.ellipse([cx - s, cy - s, cx + s, cy + s], outline=(color[0], color[1], color[2], 200), width=2)
    draw.line([cx - s, cy, cx + s, cy], fill=(color[0], color[1], color[2], 200), width=2)
    draw.line([cx, cy - s, cx, cy + s], fill=(color[0], color[1], color[2], 200), width=2)
    img = img.filter(ImageFilter.GaussianBlur(radius=1))
    img.save(output_path)

def create_page_texture(width, height, color, output_path):
    img = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rectangle([0, 0, width - 1, height - 1], fill=(color[0], color[1], color[2], 180))
    draw.line([width // 4, height // 3, 3 * width // 4, height // 3], fill=(255, 255, 255, 100), width=1)
    draw.line([width // 4, height // 2, 3 * width // 4, height // 2], fill=(255, 255, 255, 80), width=1)
    draw.line([width // 4, 2 * height // 3, 3 * width // 4, 2 * height // 3], fill=(255, 255, 255, 60), width=1)
    img = img.filter(ImageFilter.GaussianBlur(radius=1))
    img.save(output_path)

def create_after_texture(size, color, output_path):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    for r in range(size // 3, 0, -1):
        alpha = int(100 * (1 - r / (size // 3)))
        c = (color[0], color[1], color[2], alpha)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=c)
    img = img.filter(ImageFilter.GaussianBlur(radius=3))
    img.save(output_path)

def create_ghost_texture(size, color, output_path):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    for r in range(size // 2, 0, -1):
        alpha = int(80 * (1 - r / (size // 2)) ** 2)
        c = (color[0], color[1], color[2], alpha)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=c)
    img = img.filter(ImageFilter.GaussianBlur(radius=4))
    img.save(output_path)

# ========== SwordTrail ==========
sword_dir = os.path.join(base_dir, "SwordTrail")
os.makedirs(sword_dir, exist_ok=True)
create_glow_texture(32, (180, 200, 255), os.path.join(sword_dir, "SwordTrailGlow.png"), blur_radius=2)
create_glow_texture(16, (140, 180, 255), os.path.join(sword_dir, "SwordTrailQi.png"), blur_radius=1)
create_streak_texture(48, 12, (120, 160, 255), os.path.join(sword_dir, "SwordTrailScar.png"), blur_radius=1)
create_ghost_texture(32, (180, 200, 255), os.path.join(sword_dir, "SwordTrailGhost.png"))
print("SwordTrail textures generated.")

# ========== WarTrail ==========
war_dir = os.path.join(base_dir, "WarTrail")
os.makedirs(war_dir, exist_ok=True)
create_glow_texture(32, (160, 140, 120), os.path.join(war_dir, "WarTrailSmoke.png"), blur_radius=4)
create_shard_texture(16, (200, 180, 140), os.path.join(war_dir, "WarTrailShrapnel.png"), blur_radius=1)
create_glow_texture(24, (255, 160, 60), os.path.join(war_dir, "WarTrailFlame.png"), blur_radius=2)
create_ghost_texture(32, (180, 120, 80), os.path.join(war_dir, "WarTrailGhost.png"))
print("WarTrail textures generated.")

# ========== LoveTrail ==========
love_dir = os.path.join(base_dir, "LoveTrail")
os.makedirs(love_dir, exist_ok=True)
create_streak_texture(48, 10, (255, 120, 150), os.path.join(love_dir, "LoveTrailThread.png"), blur_radius=1)
# Heart shape
heart_size = 32
heart_img = Image.new('RGBA', (heart_size, heart_size), (0, 0, 0, 0))
heart_draw = ImageDraw.Draw(heart_img)
cx, cy = heart_size // 2, heart_size // 2
for t in range(0, 360, 2):
    rad = math.radians(t)
    x = 16 * math.sin(rad) ** 3
    y = -(13 * math.cos(rad) - 5 * math.cos(2 * rad) - 2 * math.cos(3 * rad) - math.cos(4 * rad))
    px = int(cx + x * 0.8)
    py = int(cy + y * 0.8)
    if 0 <= px < heart_size and 0 <= py < heart_size:
        heart_draw.ellipse([px - 2, py - 2, px + 2, py + 2], fill=(255, 100, 150, 200))
heart_img = heart_img.filter(ImageFilter.GaussianBlur(radius=2))
heart_img.save(os.path.join(love_dir, "LoveTrailHeart.png"))
create_glow_texture(32, (255, 150, 180), os.path.join(love_dir, "LoveTrailMist.png"), blur_radius=5)
create_ghost_texture(32, (255, 180, 200), os.path.join(love_dir, "LoveTrailGhost.png"))
print("LoveTrail textures generated.")

# ========== KillingTrail ==========
killing_dir = os.path.join(base_dir, "KillingTrail")
os.makedirs(killing_dir, exist_ok=True)
create_streak_texture(40, 10, (200, 40, 40), os.path.join(killing_dir, "KillingTrailStreak.png"), blur_radius=1)
create_glow_texture(28, (180, 30, 30), os.path.join(killing_dir, "KillingTrailAura.png"), blur_radius=3)
create_glow_texture(32, (100, 20, 40), os.path.join(killing_dir, "KillingTrailShadow.png"), blur_radius=4)
create_ghost_texture(32, (180, 60, 60), os.path.join(killing_dir, "KillingTrailGhost.png"))
print("KillingTrail textures generated.")

# ========== WisdomTrail ==========
wisdom_dir = os.path.join(base_dir, "WisdomTrail")
os.makedirs(wisdom_dir, exist_ok=True)
create_rune_texture(24, (220, 210, 140), os.path.join(wisdom_dir, "WisdomTrailRune.png"))
create_glow_texture(28, (200, 200, 120), os.path.join(wisdom_dir, "WisdomTrailGlow.png"), blur_radius=3)
create_page_texture(24, 28, (180, 170, 120), os.path.join(wisdom_dir, "WisdomTrailPage.png"))
create_ghost_texture(32, (200, 200, 160), os.path.join(wisdom_dir, "WisdomTrailGhost.png"))
print("WisdomTrail textures generated.")

# ========== UnrealTrail ==========
unreal_dir = os.path.join(base_dir, "UnrealTrail")
os.makedirs(unreal_dir, exist_ok=True)
create_ring_texture(32, (180, 200, 240), os.path.join(unreal_dir, "UnrealTrailWave.png"), blur_radius=2)
create_after_texture(28, (140, 180, 220), os.path.join(unreal_dir, "UnrealTrailAfter.png"))
create_shard_texture(16, (200, 220, 255), os.path.join(unreal_dir, "UnrealTrailShard.png"), blur_radius=1)
create_ghost_texture(32, (160, 180, 220), os.path.join(unreal_dir, "UnrealTrailGhost.png"))
print("UnrealTrail textures generated.")

# ========== VoiceTrail ==========
voice_dir = os.path.join(base_dir, "VoiceTrail")
os.makedirs(voice_dir, exist_ok=True)
create_ring_texture(28, (200, 180, 240), os.path.join(voice_dir, "VoiceTrailWave.png"), blur_radius=2)
create_note_texture(20, (220, 200, 255), os.path.join(voice_dir, "VoiceTrailNote.png"))
create_glow_texture(16, (160, 140, 220), os.path.join(voice_dir, "VoiceTrailResonance.png"), blur_radius=1)
create_ghost_texture(32, (180, 160, 220), os.path.join(voice_dir, "VoiceTrailGhost.png"))
print("VoiceTrail textures generated.")

print("\nAll 7 trail textures generated successfully!")