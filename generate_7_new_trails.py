#!/usr/bin/env python3
"""Generate textures for 7 new trails: Knife, Charm, Power, Flying, Luck, Rule, Devour"""
import os
import struct
import zlib

base_dir = "/home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Content/Trails"

def create_png(width, height, pixels, filepath):
    """Create a PNG file from raw pixel data (RGBA bytes)."""
    def make_chunk(chunk_type, data):
        c = chunk_type + data
        return struct.pack('>I', len(data)) + c + struct.pack('>I', zlib.crc32(c) & 0xffffffff)

    header = b'\x89PNG\r\n\x1a\n'
    ihdr = struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0)
    raw = b''
    for y in range(height):
        raw += b'\x00'
        for x in range(width):
            idx = (y * width + x) * 4
            raw += bytes(pixels[idx:idx+4])
    compressed = zlib.compress(raw)
    with open(filepath, 'wb') as f:
        f.write(header)
        f.write(make_chunk(b'IHDR', ihdr))
        f.write(make_chunk(b'IDAT', compressed))
        f.write(make_chunk(b'IEND', b''))

def glow(w, h, r, g, b, a=255, blur=3):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    max_d = min(cx, cy)
    for y in range(h):
        for x in range(w):
            dx, dy = x - cx, y - cy
            dist = (dx*dx + dy*dy) ** 0.5
            if dist < max_d:
                t = dist / max_d
                alpha = int(max(0, 1 - t) * a)
                fade = max(0, 1 - t * t)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def streak(w, h, r, g, b, a=255, blur=2):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    for y in range(h):
        for x in range(w):
            dx = abs(x - cx) / cx
            dy = abs(y - cy) / cy
            alpha = int(max(0, 1 - dy) * max(0, 1 - dx*dx) * a)
            fade = max(0, 1 - dx*dx) * max(0, 1 - dy)
            pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def ring(w, h, r, g, b, a=255):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    max_d = min(cx, cy)
    for y in range(h):
        for x in range(w):
            dx, dy = x - cx, y - cy
            dist = (dx*dx + dy*dy) ** 0.5
            if dist < max_d:
                ring_pos = dist / max_d
                alpha = int(max(0, 1 - abs(ring_pos - 0.5)*2) * a)
                fade = max(0, 1 - ring_pos)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def shard(w, h, r, g, b, a=255):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    for y in range(h):
        for x in range(w):
            dx, dy = x - cx, y - cy
            dist = (dx*dx + dy*dy) ** 0.5
            angle = abs((dx / max(abs(dy), 0.01)) if dy != 0 else 999)
            if dist < min(cx, cy) and angle > 1.5:
                t = dist / min(cx, cy)
                alpha = int(max(0, 1 - t) * a)
                fade = max(0, 1 - t*t)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def heart(w, h, r, g, b, a=255):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    for y in range(h):
        for x in range(w):
            nx = (x - cx) / cx
            ny = (y - cy) / cy
            hx = nx * nx + (ny - 0.3) * (ny - 0.3) * (ny - 0.3) - 0.6
            if hx < 0:
                t = abs(hx) * 2
                alpha = int(max(0, min(1, 1 - t)) * a)
                fade = max(0, 1 - t)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def star_4(w, h, r, g, b, a=255):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    for y in range(h):
        for x in range(w):
            dx, dy = x - cx, y - cy
            dist = (dx*dx + dy*dy) ** 0.5
            angle = abs((dx / max(abs(dy), 0.01)) if dy != 0 else 999)
            if dist < min(cx, cy) and (angle > 2.0 or angle < 0.5):
                t = dist / min(cx, cy)
                alpha = int(max(0, 1 - t) * a)
                fade = max(0, 1 - t*t)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def clover(w, h, r, g, b, a=255):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    for y in range(h):
        for x in range(w):
            nx = (x - cx) / cx
            ny = (y - cy) / cy
            dist = (nx*nx + ny*ny) ** 0.5
            angle = (nx if nx != 0 else 0.001)
            ang = abs((ny / angle) if angle != 0 else 999)
            if dist < 1.0 and (dist > 0.3 or (abs(nx) < 0.2 and ny > 0)):
                t = dist
                alpha = int(max(0, 1 - t) * a)
                fade = max(0, 1 - t*t)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def vortex(w, h, r, g, b, a=255):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    for y in range(h):
        for x in range(w):
            dx, dy = x - cx, y - cy
            dist = (dx*dx + dy*dy) ** 0.5
            angle = (dx if dx != 0 else 0.001)
            ang = abs((dy / angle) if angle != 0 else 999)
            if dist < min(cx, cy):
                t = dist / min(cx, cy)
                swirl = abs(ang - 1.0) * 2
                alpha = int(max(0, 1 - t) * max(0, 1 - swirl) * a)
                fade = max(0, 1 - t*t)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def grid_node(w, h, r, g, b, a=255):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    for y in range(h):
        for x in range(w):
            dx, dy = x - cx, y - cy
            dist = (dx*dx + dy*dy) ** 0.5
            if dist < min(cx, cy):
                t = dist / min(cx, cy)
                cross = max(0, 1 - abs(dx)/cx*2) + max(0, 1 - abs(dy)/cy*2)
                alpha = int(max(0, 1 - t) * min(1, cross) * a)
                fade = max(0, 1 - t*t)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def tendril(w, h, r, g, b, a=255):
    pixels = [0] * (w * h * 4)
    cx, cy = w // 2, h // 2
    for y in range(h):
        for x in range(w):
            dx, dy = x - cx, y - cy
            dist = (dx*dx + dy*dy) ** 0.5
            if dist < min(cx, cy):
                t = dist / min(cx, cy)
                alpha = int(max(0, 1 - t) * a)
                fade = max(0, 1 - t)
                pixels[(y*w+x)*4:(y*w+x)*4+4] = [min(255, int(r*fade)), min(255, int(g*fade)), min(255, int(b*fade)), alpha]
    return pixels

def create_ghost(w, h, r, g, b, a=120):
    return glow(w, h, r, g, b, a, blur=4)

# ========== KnifeTrail (刃) ==========
knife_dir = os.path.join(base_dir, "KnifeTrail")
os.makedirs(knife_dir, exist_ok=True)
create_png(32, 32, streak(32, 32, 200, 215, 240, 230), os.path.join(knife_dir, "KnifeTrailFlash.png"))
create_png(32, 16, streak(32, 16, 160, 180, 220, 200), os.path.join(knife_dir, "KnifeTrailCut.png"))
create_png(16, 16, shard(16, 16, 180, 195, 230, 200), os.path.join(knife_dir, "KnifeTrailShard.png"))
create_png(32, 32, create_ghost(32, 32, 190, 200, 220, 120), os.path.join(knife_dir, "KnifeTrailGhost.png"))
print("KnifeTrail textures generated.")

# ========== CharmTrail (魅) ==========
charm_dir = os.path.join(base_dir, "CharmTrail")
os.makedirs(charm_dir, exist_ok=True)
create_png(32, 32, heart(32, 32, 255, 140, 180, 220), os.path.join(charm_dir, "CharmTrailHeart.png"))
create_png(32, 32, ring(32, 32, 255, 160, 200, 180), os.path.join(charm_dir, "CharmTrailRing.png"))
create_png(32, 32, glow(32, 32, 255, 180, 220, 160, blur=5), os.path.join(charm_dir, "CharmTrailMist.png"))
create_png(32, 32, create_ghost(32, 32, 255, 180, 210, 140), os.path.join(charm_dir, "CharmTrailGhost.png"))
print("CharmTrail textures generated.")

# ========== PowerTrail (力) ==========
power_dir = os.path.join(base_dir, "PowerTrail")
os.makedirs(power_dir, exist_ok=True)
create_png(32, 32, ring(32, 32, 255, 200, 80, 200), os.path.join(power_dir, "PowerTrailShock.png"))
create_png(32, 32, glow(32, 32, 255, 180, 60, 220, blur=3), os.path.join(power_dir, "PowerTrailAura.png"))
create_png(32, 16, streak(32, 16, 255, 160, 50, 200), os.path.join(power_dir, "PowerTrailBurst.png"))
create_png(32, 32, create_ghost(32, 32, 255, 180, 80, 140), os.path.join(power_dir, "PowerTrailGhost.png"))
print("PowerTrail textures generated.")

# ========== FlyingTrail (翔) ==========
flying_dir = os.path.join(base_dir, "FlyingTrail")
os.makedirs(flying_dir, exist_ok=True)
create_png(24, 24, glow(24, 24, 220, 235, 255, 200, blur=2), os.path.join(flying_dir, "FlyingTrailFeather.png"))
create_png(32, 12, streak(32, 12, 180, 210, 255, 150), os.path.join(flying_dir, "FlyingTrailWindTail.png"))
create_png(16, 16, glow(16, 16, 160, 200, 255, 120, blur=2), os.path.join(flying_dir, "FlyingTrailSpeedAfter.png"))
create_png(32, 32, create_ghost(32, 32, 200, 220, 255, 100), os.path.join(flying_dir, "FlyingTrailGhost.png"))
print("FlyingTrail textures generated.")

# ========== LuckTrail (运) ==========
luck_dir = os.path.join(base_dir, "LuckTrail")
os.makedirs(luck_dir, exist_ok=True)
create_png(28, 28, clover(28, 28, 100, 220, 100, 220), os.path.join(luck_dir, "LuckTrailClover.png"))
create_png(20, 20, star_4(20, 20, 255, 220, 80, 230), os.path.join(luck_dir, "LuckTrailStar.png"))
create_png(24, 8, streak(24, 8, 220, 200, 100, 180), os.path.join(luck_dir, "LuckTrailThread.png"))
create_png(32, 32, create_ghost(32, 32, 180, 230, 180, 120), os.path.join(luck_dir, "LuckTrailGhost.png"))
print("LuckTrail textures generated.")

# ========== RuleTrail (规) ==========
rule_dir = os.path.join(base_dir, "RuleTrail")
os.makedirs(rule_dir, exist_ok=True)
create_png(16, 16, grid_node(16, 16, 150, 200, 255, 200), os.path.join(rule_dir, "RuleTrailNode.png"))
create_png(24, 10, streak(24, 10, 180, 210, 240, 180), os.path.join(rule_dir, "RuleTrailMark.png"))
create_png(32, 32, ring(32, 32, 120, 180, 255, 160), os.path.join(rule_dir, "RuleTrailRing.png"))
create_png(32, 32, create_ghost(32, 32, 180, 200, 230, 100), os.path.join(rule_dir, "RuleTrailGhost.png"))
print("RuleTrail textures generated.")

# ========== DevourTrail (噬) ==========
devour_dir = os.path.join(base_dir, "DevourTrail")
os.makedirs(devour_dir, exist_ok=True)
create_png(32, 32, vortex(32, 32, 180, 80, 200, 220), os.path.join(devour_dir, "DevourTrailMaw.png"))
create_png(16, 20, glow(16, 20, 100, 220, 80, 200, blur=2), os.path.join(devour_dir, "DevourTrailAcid.png"))
create_png(32, 10, tendril(32, 10, 200, 120, 220, 180), os.path.join(devour_dir, "DevourTrailTendril.png"))
create_png(32, 32, create_ghost(32, 32, 160, 80, 180, 130), os.path.join(devour_dir, "DevourTrailGhost.png"))
print("DevourTrail textures generated.")

print("\nAll 7 trail textures generated successfully!")