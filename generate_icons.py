from pathlib import Path
import sys

from PIL import Image, ImageOps

ROOT = Path(__file__).resolve().parent
SIZES = (16, 24, 32, 48, 64, 128, 256)
ASSETS = {
    "colibri": "colibri/desktop/src-tauri/icons/128x128.png",
    "jan": "jan/src-tauri/icons/icon.png",
    "ollama": "ollama/docs/images/favicon.png",
}


def main():
    output = Path(sys.argv[1])
    output.mkdir(parents=True, exist_ok=True)
    for backend, source in ASSETS.items():
        with Image.open(ROOT / "upstream" / source) as image:
            rgba = image.convert("RGBA")
            frames = [ImageOps.pad(rgba, (size, size), Image.Resampling.LANCZOS)
                      for size in SIZES]
        target = output / f"{backend}.ico"
        frames[-1].save(target, sizes=[frame.size for frame in frames],
                        append_images=frames[:-1])
        with Image.open(target) as icon:
            if icon.ico.sizes() != {frame.size for frame in frames}:
                raise ValueError(f"{target} is missing one or more icon sizes")


if __name__ == "__main__":
    main()
