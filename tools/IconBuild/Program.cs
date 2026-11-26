using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;
using SkiaSharp;

int[] sizes = [16, 24, 32, 48, 64, 128, 256];
(string Id, string Path)[] assets =
[
    ("colibri", "colibri/desktop/src-tauri/icons/128x128.png"),
    ("jan", "jan/src-tauri/icons/icon.png"),
    ("ollama", "ollama/docs/images/favicon.png"),
    ("llamacpp", "llamacpp/examples/server/public_legacy/favicon.ico"),
    ("vllm", "vllm/docs/assets/logos/vllm-logo-only-light.png")
];

if (args is ["generate", var upstream, var output])
{
    Directory.CreateDirectory(output);
    foreach (var (id, path) in assets)
    {
        var source = Path.Combine(upstream, path);
        using var original = SKBitmap.Decode(source)
            ?? throw new InvalidDataException($"Cannot decode '{source}'. Initialize the upstream Git submodules.");
        var frames = sizes.Select(size => Render(original, size)).ToArray();
        using var writer = new BinaryWriter(File.Create(Path.Combine(output, id + ".ico")));
        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)frames.Length);

        var offset = 6 + (16 * frames.Length);
        for (var index = 0; index < frames.Length; index++)
        {
            var dimension = sizes[index] == 256 ? (byte)0 : (byte)sizes[index];
            writer.Write(dimension);
            writer.Write(dimension);
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.Write((ushort)32);
            writer.Write(frames[index].Length);
            writer.Write(offset);
            offset += frames[index].Length;
        }

        foreach (var frame in frames)
            writer.Write(frame);
    }

    return 0;
}

if (args is ["verify", var package])
{
    using var archive = ZipFile.OpenRead(package);
    using var nuspec = archive.Entries.Single(entry => entry.Name.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
    if (XDocument.Load(nuspec).Descendants().Any(element => element.Name.LocalName == "dependency"))
        throw new InvalidDataException("The icon package has a NuGet dependency.");

    const string library = "lib/netstandard2.0/SkyCD.Resource.Icons.LlmBackends.dll";
    if (archive.Entries.Where(entry => entry.FullName.StartsWith("lib/", StringComparison.Ordinal)
            || entry.FullName.StartsWith("runtimes/", StringComparison.Ordinal))
        .Any(entry => entry.FullName != library))
        throw new InvalidDataException("Unexpected runtime assets in the icon package.");

    using var entryStream = (archive.GetEntry(library) ?? throw new InvalidDataException(library)).Open();
    using var assemblyBytes = new MemoryStream();
    entryStream.CopyTo(assemblyBytes);
    var assembly = Assembly.Load(assemblyBytes.ToArray());
    var api = assembly.GetType("SkyCD.Resource.Icons.LlmBackends.BackendIcons", throwOnError: true)!;
    var openIcon = api.GetMethod("OpenIcon")!;
    var openPng = api.GetMethod("OpenPng")!;
    foreach (var (id, _) in assets)
    {
        using var icon = (Stream?)openIcon.Invoke(null, [id]) ?? throw new InvalidDataException(id);
        using var header = new BinaryReader(icon);
        if (header.ReadUInt16() != 0 || header.ReadUInt16() != 1 || header.ReadUInt16() != sizes.Length)
            throw new InvalidDataException($"Invalid ICO header for '{id}'.");
        foreach (var size in sizes)
        {
            using var frame = (Stream?)openPng.Invoke(null, [id, size]);
            using var bitmap = frame is null ? null : SKBitmap.Decode(frame);
            if (bitmap is null || bitmap.Width != size || bitmap.Height != size)
                throw new InvalidDataException($"Invalid {size} px frame for '{id}'.");
        }
    }

    if (openIcon.Invoke(null, ["missing"]) is not null || openPng.Invoke(null, ["jan", 17]) is not null)
        throw new InvalidDataException("Missing resources should return null.");

    Console.WriteLine($"Verified {assets.Length * sizes.Length} frames in the dependency-free .NET Standard 2.0 assembly.");
    return 0;
}

Console.Error.WriteLine("Usage: IconBuild generate <upstream-directory> <output-directory> | verify <package>");
return 2;

static byte[] Render(SKBitmap source, int size)
{
    var scale = Math.Min((float)size / source.Width, (float)size / source.Height);
    var width = Math.Max(1, (int)Math.Round(source.Width * scale));
    var height = Math.Max(1, (int)Math.Round(source.Height * scale));
    var left = (size - width) / 2;
    var top = (size - height) / 2;
    using var canvasBitmap = new SKBitmap(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);
    using var canvas = new SKCanvas(canvasBitmap);
    canvas.Clear(SKColors.Transparent);
    using var image = SKImage.FromBitmap(source);
    canvas.DrawImage(image, new SKRect(left, top, left + width, top + height),
        new SKSamplingOptions(SKCubicResampler.Mitchell));
    canvas.Flush();
    using var png = canvasBitmap.Encode(SKEncodedImageFormat.Png, 100)
        ?? throw new InvalidDataException("Cannot encode ICO frame as PNG.");
    return png.ToArray();
}
