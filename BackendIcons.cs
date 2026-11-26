namespace SkyCD.Resource.Icons.LlmBackends;

/// <summary>Embedded multi-resolution icons for local LLM backends.</summary>
public static class BackendIcons
{
    /// <summary>Opens a multi-resolution ICO. The caller owns the returned stream.</summary>
    public static Stream? OpenIcon(string backendId) =>
        typeof(BackendIcons).Assembly.GetManifestResourceStream($"Icons.{backendId}.ico");

    /// <summary>Opens the PNG frame of the exact requested size, if available.</summary>
    public static Stream? OpenPng(string backendId, int size)
    {
        using var icon = OpenIcon(backendId);
        if (icon is null || size is < 1 or > 256)
            return null;

        using var reader = new BinaryReader(icon);
        if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1)
            return null;

        var count = reader.ReadUInt16();
        var dimension = size == 256 ? 0 : size;
        for (var index = 0; index < count; index++)
        {
            var width = reader.ReadByte();
            var height = reader.ReadByte();
            reader.ReadUInt16(); // Palette size and reserved byte.
            reader.ReadUInt32(); // Color planes and bit depth.
            var length = reader.ReadInt32();
            var offset = reader.ReadUInt32();
            if (width != dimension || height != dimension)
                continue;

            icon.Position = offset;
            return new MemoryStream(reader.ReadBytes(length), writable: false);
        }

        return null;
    }
}
