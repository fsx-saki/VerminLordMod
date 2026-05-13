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
        dist_from_center = abs(x - cx) / cx if cx > 0 else 0
        alpha = int(255 * (1 - dist_from_center) ** 3)
        for y in range(height):
            dy = abs(y - cy) / cy if cy > 0 else 0
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

def create_cross_texture(size, color, output_path, blur_radius=1):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    w = max(1, size // 8)
    draw.rectangle([cx - w, 0, cx + w, size - 1], fill=(color[0], color[1], color[2], 200))
    draw.rectangle([0, cy - w, size - 1, cy + w], fill=(color[0], color[1], color[2], 200))
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_arc_texture(size, color, output_path, blur_radius=1):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    r = size // 2 - 2
    draw.arc([cx - r, cy - r, cx + r, cy + r], 0, 180, fill=(color[0], color[1], color[2], 200), width=2)
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_spiral_texture(size, color, output_path, blur_radius=1):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    for t in range(0, 360 * 3, 10):
        rad = math.radians(t)
        r = t / (360 * 3) * (size // 2 - 2)
        x = int(cx + r * math.cos(rad))
        y = int(cy + r * math.sin(rad))
        if 0 <= x < size and 0 <= y < size:
            alpha = int(200 * (1 - t / (360 * 3)))
            draw.ellipse([x - 1, y - 1, x + 1, y + 1], fill=(color[0], color[1], color[2], alpha))
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_feather_texture(size, color, output_path, blur_radius=1):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    points = [(cx, 0), (size - 1, cy), (cx, size - 1), (cx // 2, cy)]
    draw.polygon(points, fill=(color[0], color[1], color[2], 200))
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_clover_texture(size, color, output_path, blur_radius=1):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    r = size // 4
    for angle in [0, 90, 180, 270]:
        rad = math.radians(angle)
        hx = cx + int(r * math.cos(rad))
        hy = cy + int(r * math.sin(rad))
        draw.ellipse([hx - r // 2, hy - r // 2, hx + r // 2, hy + r // 2], fill=(color[0], color[1], color[2], 200))
    draw.ellipse([cx - 2, cy - 2, cx + 2, cy + 2], fill=(color[0], color[1], color[2], 220))
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_grid_texture(size, color, output_path, blur_radius=1):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    step = size // 4
    for i in range(0, size, step):
        draw.line([(i, 0), (i, size - 1)], fill=(color[0], color[1], color[2], 100), width=1)
        draw.line([(0, i), (size - 1, i)], fill=(color[0], color[1], color[2], 100), width=1)
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

def create_vortex_texture(size, color, output_path, blur_radius=2):
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    for r in range(size // 2, 0, -1):
        alpha = int(150 * (1 - r / (size // 2)))
        c = (color[0], color[1], color[2], alpha)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=c, width=1)
    if blur_radius > 0:
        img = img.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    img.save(output_path)

# ========== REGENERATE CORRUPTED TEXTURES ==========
print("=== Regenerating corrupted textures ===")

# CloudTrail
cloud_dir = os.path.join(base_dir, "CloudTrail")
os.makedirs(cloud_dir, exist_ok=True)
create_glow_texture(32, (220, 220, 240), os.path.join(cloud_dir, "CloudTrailPuff.png"), blur_radius=5)
create_streak_texture(40, 8, (200, 200, 230), os.path.join(cloud_dir, "CloudTrailWisp.png"), blur_radius=3)
create_ring_texture(24, (180, 200, 220), os.path.join(cloud_dir, "CloudTrailCond.png"), blur_radius=2)
create_ghost_texture(32, (200, 210, 230), os.path.join(cloud_dir, "CloudTrailGhost.png"))
print("CloudTrail textures regenerated.")

# SkyTrail
sky_dir = os.path.join(base_dir, "SkyTrail")
os.makedirs(sky_dir, exist_ok=True)
create_arc_texture(32, (180, 200, 255), os.path.join(sky_dir, "SkyTrailArc.png"), blur_radius=2)
create_streak_texture(48, 8, (160, 200, 255), os.path.join(sky_dir, "SkyTrailAurora.png"), blur_radius=3)
create_cross_texture(24, (200, 220, 255), os.path.join(sky_dir, "SkyTrailZenith.png"), blur_radius=1)
create_ghost_texture(32, (180, 210, 255), os.path.join(sky_dir, "SkyTrailGhost.png"))
print("SkyTrail textures regenerated.")

# MudTrail
mud_dir = os.path.join(base_dir, "MudTrail")
os.makedirs(mud_dir, exist_ok=True)
create_glow_texture(24, (120, 80, 40), os.path.join(mud_dir, "MudTrailClod.png"), blur_radius=3)
create_streak_texture(36, 6, (100, 70, 40), os.path.join(mud_dir, "MudTrailCrack.png"), blur_radius=1)
create_glow_texture(16, (80, 60, 30), os.path.join(mud_dir, "MudTrailDrip.png"), blur_radius=2)
create_ghost_texture(32, (100, 75, 50), os.path.join(mud_dir, "MudTrailGhost.png"))
print("MudTrail textures regenerated.")

# BoneTrail
bone_dir = os.path.join(base_dir, "BoneTrail")
os.makedirs(bone_dir, exist_ok=True)
create_arc_texture(28, (220, 210, 190), os.path.join(bone_dir, "BoneTrailRib.png"), blur_radius=1)
create_glow_texture(20, (200, 220, 180), os.path.join(bone_dir, "BoneTrailMarrow.png"), blur_radius=3)
create_shard_texture(16, (230, 220, 200), os.path.join(bone_dir, "BoneTrailSpike.png"), blur_radius=1)
create_ghost_texture(32, (200, 195, 185), os.path.join(bone_dir, "BoneTrailGhost.png"))
print("BoneTrail textures regenerated.")

# MetalTrail
metal_dir = os.path.join(base_dir, "MetalTrail")
os.makedirs(metal_dir, exist_ok=True)
create_shard_texture(16, (255, 220, 100), os.path.join(metal_dir, "MetalTrailSpark.png"), blur_radius=2)
create_shard_texture(20, (200, 180, 140), os.path.join(metal_dir, "MetalTrailShard.png"), blur_radius=1)
create_streak_texture(40, 6, (180, 170, 150), os.path.join(metal_dir, "MetalTrailStreak.png"), blur_radius=1)
create_ghost_texture(32, (180, 170, 150), os.path.join(metal_dir, "MetalTrailGhost.png"))
print("MetalTrail textures regenerated.")

# LifeDeathTrail
life_death_dir = os.path.join(base_dir, "LifeDeathTrail")
os.makedirs(life_death_dir, exist_ok=True)
create_glow_texture(16, (200, 100, 120), os.path.join(life_death_dir, "LifeDeathTrailPetal.png"), blur_radius=1)
create_glow_texture(20, (180, 220, 140), os.path.join(life_death_dir, "LifeDeathTrailBloom.png"), blur_radius=2)
create_ring_texture(28, (180, 160, 200), os.path.join(life_death_dir, "LifeDeathTrailRing.png"), blur_radius=2)
create_ghost_texture(32, (180, 160, 180), os.path.join(life_death_dir, "LifeDeathTrailGhost.png"))
print("LifeDeathTrail textures regenerated.")

# QiTrail
qi_dir = os.path.join(base_dir, "QiTrail")
os.makedirs(qi_dir, exist_ok=True)
create_streak_texture(48, 8, (180, 220, 255), os.path.join(qi_dir, "QiTrailStream.png"), blur_radius=2)
create_glow_texture(12, (255, 240, 200), os.path.join(qi_dir, "QiTrailAcupoint.png"), blur_radius=2)
create_streak_texture(32, 6, (200, 230, 255), os.path.join(qi_dir, "QiTrailPulse.png"), blur_radius=1)
create_ghost_texture(32, (180, 210, 240), os.path.join(qi_dir, "QiTrailGhost.png"))
print("QiTrail textures regenerated.")

# ========== NEW TRAILS ==========
print("\n=== Generating new trail textures ===")

# ========== KnifeTrail (刃) ==========
knife_dir = os.path.join(base_dir, "KnifeTrail")
os.makedirs(knife_dir, exist_ok=True)
create_streak_texture(48, 8, (200, 210, 230), os.path.join(knife_dir, "KnifeTrailFlash.png"), blur_radius=1)
create_streak_texture(36, 4, (180, 190, 220), os.path.join(knife_dir, "KnifeTrailCut.png"), blur_radius=1)
create_shard_texture(14, (220, 225, 240), os.path.join(knife_dir, "KnifeTrailShard.png"), blur_radius=1)
create_ghost_texture(32, (190, 200, 220), os.path.join(knife_dir, "KnifeTrailGhost.png"))
print("KnifeTrail textures generated.")

# ========== CharmTrail (魅) ==========
charm_dir = os.path.join(base_dir, "CharmTrail")
os.makedirs(charm_dir, exist_ok=True)
# Heart shape
heart_size = 28
heart_img = Image.new('RGBA', (heart_size, heart_size), (0, 0, 0, 0))
heart_draw = ImageDraw.Draw(heart_img)
cx, cy = heart_size // 2, heart_size // 2
for t in range(0, 360, 2):
    rad = math.radians(t)
    x = 16 * math.sin(rad) ** 3
    y = -(13 * math.cos(rad) - 5 * math.cos(2 * rad) - 2 * math.cos(3 * rad) - math.cos(4 * rad))
    px = int(cx + x * 0.7)
    py = int(cy + y * 0.7)
    if 0 <= px < heart_size and 0 <= py < heart_size:
        heart_draw.ellipse([px - 2, py - 2, px + 2, py + 2], fill=(255, 120, 180, 200))
heart_img = heart_img.filter(ImageFilter.GaussianBlur(radius=2))
heart_img.save(os.path.join(charm_dir, "CharmTrailHeart.png"))
create_ring_texture(28, (255, 150, 200), os.path.join(charm_dir, "CharmTrailRing.png"), blur_radius=2)
create_glow_texture(32, (255, 180, 210), os.path.join(charm_dir, "CharmTrailMist.png"), blur_radius=5)
create_ghost_texture(32, (255, 180, 210), os.path.join(charm_dir, "CharmTrailGhost.png"))
print("CharmTrail textures generated.")

# ========== PowerTrail (力) ==========
power_dir = os.path.join(base_dir, "PowerTrail")
os.makedirs(power_dir, exist_ok=True)
create_ring_texture(36, (255, 200, 80), os.path.join(power_dir, "PowerTrailShock.png"), blur_radius=2)
create_glow_texture(28, (255, 180, 60), os.path.join(power_dir, "PowerTrailAura.png"), blur_radius=4)
create_streak_texture(32, 8, (255, 160, 50), os.path.join(power_dir, "PowerTrailBurst.png"), blur_radius=2)
create_ghost_texture(32, (255, 180, 80), os.path.join(power_dir, "PowerTrailGhost.png"))
print("PowerTrail textures generated.")

# ========== FlyingTrail (翔) ==========
flying_dir = os.path.join(base_dir, "FlyingTrail")
os.makedirs(flying_dir, exist_ok=True)
create_feather_texture(24, (220, 230, 255), os.path.join(flying_dir, "FlyingTrailFeather.png"), blur_radius=1)
create_streak_texture(40, 6, (200, 220, 255), os.path.join(flying_dir, "FlyingTrailWind.png"), blur_radius=2)
create_glow_texture(20, (180, 210, 255), os.path.join(flying_dir, "FlyingTrailAfter.png"), blur_radius=3)
create_ghost_texture(32, (200, 220, 255), os.path.join(flying_dir, "FlyingTrailGhost.png"))
print("FlyingTrail textures generated.")

# ========== LuckTrail (运) ==========
luck_dir = os.path.join(base_dir, "LuckTrail")
os.makedirs(luck_dir, exist_ok=True)
create_clover_texture(24, (180, 255, 120), os.path.join(luck_dir, "LuckTrailClover.png"), blur_radius=1)
create_streak_texture(36, 4, (255, 220, 100), os.path.join(luck_dir, "LuckTrailLine.png"), blur_radius=1)
create_glow_texture(16, (255, 240, 180), os.path.join(luck_dir, "LuckTrailOrb.png"), blur_radius=3)
create_ghost_texture(32, (220, 240, 180), os.path.join(luck_dir, "LuckTrailGhost.png"))
print("LuckTrail textures generated.")

# ========== RuleTrail (律) ==========
rule_dir = os.path.join(base_dir, "RuleTrail")
os.makedirs(rule_dir, exist_ok=True)
create_grid_texture(32, (180, 180, 220), os.path.join(rule_dir, "RuleTrailGrid.png"), blur_radius=1)
create_streak_texture(40, 4, (200, 200, 240), os.path.join(rule_dir, "RuleTrailChain.png"), blur_radius=1)
create_ring_texture(30, (200, 200, 230), os.path.join(rule_dir, "RuleTrailRing.png"), blur_radius=2)
create_ghost_texture(32, (180, 180, 220), os.path.join(rule_dir, "RuleTrailGhost.png"))
print("RuleTrail textures generated.")

# ========== DevourTrail (噬) ==========
devour_dir = os.path.join(base_dir, "DevourTrail")
os.makedirs(devour_dir, exist_ok=True)
create_vortex_texture(36, (120, 60, 180), os.path.join(devour_dir, "DevourTrailVortex.png"), blur_radius=2)
create_glow_texture(16, (180, 100, 200), os.path.join(devour_dir, "DevourTrailDigest.png"), blur_radius=2)
create_ring_texture(28, (100, 50, 160), os.path.join(devour_dir, "DevourTrailWave.png"), blur_radius=2)
create_ghost_texture(32, (120, 60, 160), os.path.join(devour_dir, "DevourTrailGhost.png"))
print("DevourTrail textures generated.")

print("\nAll textures generated successfully!")