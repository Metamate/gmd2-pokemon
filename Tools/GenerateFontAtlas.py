# Regenerates the bitmap font atlases used by BitmapFont.cs.
# Requires: pip install Pillow
# Run from the repo root: python tools/GenerateFontAtlas.py

from PIL import ImageFont, ImageDraw, Image

FONT_FILE = "Pokemon/Content/fonts/font.ttf"
OUT_DIR   = "Pokemon/Content/fonts"
COLS      = 16
CHARS     = [chr(i) for i in range(32, 127)]

for pixel_size, name in [(8, "small_atlas"), (16, "medium_atlas"), (32, "large_atlas")]:
    font             = ImageFont.truetype(FONT_FILE, pixel_size)
    ascent, descent  = font.getmetrics()
    cell_h           = ascent + descent

    advances = []
    cell_w   = 0
    for c in CHARS:
        bbox = font.getbbox(c)
        w    = max(1, bbox[2]) if bbox else 1
        advances.append(w)
        if w > cell_w:
            cell_w = w

    rows  = (len(CHARS) + COLS - 1) // COLS
    atlas = Image.new("RGBA", (COLS * cell_w, rows * cell_h), (0, 0, 0, 0))
    draw  = ImageDraw.Draw(atlas)

    for i, c in enumerate(CHARS):
        col = i % COLS
        row = i // COLS
        draw.text((col * cell_w, row * cell_h + ascent), c, font=font, fill=(255, 255, 255, 255), anchor='ls')

    # Threshold: alpha > 127 → fully opaque white, else transparent
    px = atlas.load()
    for y in range(atlas.height):
        for x in range(atlas.width):
            r, g, b, a = px[x, y]
            px[x, y] = (255, 255, 255, 255) if a > 127 else (0, 0, 0, 0)

    atlas.save(f"{OUT_DIR}/{name}.png")
    print(f"{name}: cell {cell_w}x{cell_h}")
    print(f"  advances: [{','.join(str(a) for a in advances)}]")
    print()

print("Done. If cell sizes or advances changed, update BitmapFont.cs accordingly.")
