# SkyCD.Resource.Icons.LlmBackends

.NET 10 resources for identifying local LLM backends. Each icon includes 16, 24, 32, 48, 64, 128 and 256 px frames. Builds use the Git-pinned original artwork. See [Third-party notices](THIRD-PARTY-NOTICES.md) for asset terms.

```csharp
using SkyCD.Resource.Icons.LlmBackends;

using var ico = BackendIcons.OpenIcon("colibri");
using var png = BackendIcons.OpenPng("colibri", 16);
```

## Build

```sh
git submodule update --init
python -m pip install -r requirements.txt
dotnet build -c Release
```

Pull requests and pushes build and pack the project. Push a version tag such as `v0.1.0` to publish that immutable version to [SkyCD GitHub Packages](https://nuget.pkg.github.com/SkyCD/index.json). Authenticate with a package-read credential to restore the package. Grant the consuming repository Actions read access in the package settings.

The library code is MIT. Bundled artwork retains its upstream Apache-2.0 or MIT terms and applicable trademark conditions. When redistributing, include the accompanying notices and license texts.
