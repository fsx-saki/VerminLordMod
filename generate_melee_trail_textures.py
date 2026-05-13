import os
import struct
import zlib

base_dir = "/home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Content/Trails/MeleeSwingTrail"
os.makedirs(base_dir, exist_ok=True)

def create_png(width, height, pixels, filepath):
    def make_row(row_data):
        raw = b'\x00'
        for r, g, b, a in row_data:
            raw += struct.pack('BBBB', r, g, b, a)
        return raw

    raw_data = b''
    for row in pixels:
        raw_data += make_row(row)

    def make_chunk(chunk_type, data):
        c = chunk_type + data
        return struct.pack('>I', len(data)) + c + struct.pack('>I', zlib.crc32(c) & 0xFFFFFFFF)

    signature = b'\x89PNG\r\n\x1a\n'
    ihdr = struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0)
    compressed = zlib.compress(raw_data)

    with open(filepath, 'wb') as f:
        f.write(signature)
        f.write(make_chunk(b'IHDR', ihdr))
        f.write(make_chunk(b'IDAT', compressed))
        f.write(make_chunk(b'IEND', b''))

def gradient_arc(w, h, r, g, b, a):
    pixels = []
    for y in range(h):
        row = []
        for x in range(w):
            cx, cy = x / w, y / h
            dist = abs(cx - 0.5) * 2
            fade = max(0, 1 - dist * 1.5)
            edge = 1 - abs(cy - 0.5) * 2
            alpha = int(fade * edge * a)
            row.append((r, g, b, max(0, min(255, alpha))))
        pixels.append(row)
    return pixels

def gradient_stab(w, h, r, g, b, a):
    pixels = []
    for y in range(h):
        row = []
        for x in range(w):
            cx, cy = x / w, y / h
            tip = 1 - cx
            spread = 1 - abs(cy - 0.5) * 2
            alpha = int(tip * spread * a)
            row.append((r, g, b, max(0, min(255, alpha))))
        pixels.append(row)
    return pixels

def gradient_ring(w, h, r, g, b, a):
    pixels = []
    for y in range(h):
        row = []
        for x in range(w):
            cx, cy = x / w, y / h
            dx, dy = cx - 0.5, cy - 0.5
            dist = (dx * dx + dy * dy) ** 0.5 * 2
            ring = max(0, 1 - abs(dist - 0.5) * 4)
            alpha = int(ring * a)
            row.append((r, g, b, max(0, min(255, alpha))))
        pixels.append(row)
    return pixels

create_png(64, 16, gradient_arc(64, 16, 200, 200, 220, 200), os.path.join(base_dir, "MeleeSwingArc.png"))
create_png(32, 16, gradient_stab(32, 16, 220, 210, 200, 220), os.path.join(base_dir, "MeleeStabImpact.png"))
create_png(32, 32, gradient_ring(32, 32, 180, 180, 200, 180), os.path.join(base_dir, "MeleeSmashRing.png"))
print("MeleeSwingTrail textures generated.")