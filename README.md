[![Build and publish](https://github.com/SkyCD/SkyCD.Resource.Icons.LlmBackends/actions/workflows/ci.yml/badge.svg)](https://github.com/SkyCD/SkyCD.Resource.Icons.LlmBackends/actions/workflows/ci.yml)
[![Code license: MIT](https://img.shields.io/badge/code%20license-MIT-blue.svg)](LICENSE)
[![Target framework](https://img.shields.io/badge/target-.NET%20Standard%202.0-512BD4)](#platform-support)
[![NuGet package](https://img.shields.io/github/v/tag/SkyCD/SkyCD.Resource.Icons.LlmBackends?label=NuGet&sort=semver)](https://github.com/orgs/SkyCD/packages/nuget/package/SkyCD.Resource.Icons.LlmBackends)

# SkyCD.Resource.Icons.LlmBackends

Multi-resolution icons for local AI backends, packaged as a small .NET library. [SkyCD](https://github.com/SkyCD/SkyCD) uses them in its provider picker.

## Why this exists

Apps can share the same backend artwork without maintaining their own copies. Each icon is generated from an upstream PNG pinned by a Git submodule. The library embeds seven sizes per icon: 16, 24, 32, 48, 64, 128, and 256 px.

| Backend | Upstream artwork |
| --- | --- |
| Colibrì | [Desktop application icon](https://github.com/JustVugg/colibri/blob/e479104ae01bdc40ce81e675299b1d230891b321/desktop/src-tauri/icons/128x128.png) |
| Jan | [Desktop application icon](https://github.com/janhq/jan/blob/49a84bdeaebdfc0f36c00f252eb638f516a48432/src-tauri/icons/icon.png) |
| Ollama | [Documentation favicon](https://github.com/ollama/ollama/blob/8ae8acb81afba9c3c67adf08190964eb7d1207ef/docs/images/favicon.png) |

## Install from GitHub Packages

Choose a fixed version from [GitHub Packages](https://github.com/orgs/SkyCD/packages/nuget/package/SkyCD.Resource.Icons.LlmBackends). GitHub requires package-read credentials to restore from its NuGet feed.

```powershell
dotnet nuget add source https://nuget.pkg.github.com/SkyCD/index.json --name skycd
$env:NuGetPackageSourceCredentials_skycd = "Username=YOUR_GITHUB_USERNAME;Password=$env:GITHUB_PACKAGES_TOKEN;ValidAuthenticationTypes=Basic"
dotnet add package SkyCD.Resource.Icons.LlmBackends --version YOUR_PUBLISHED_VERSION
```

Use a classic PAT with `read:packages` for local restores. In GitHub Actions, use `GITHUB_TOKEN` with `packages: read` and grant the consuming repo package access if required. If you use NuGet source mapping, map the exact package ID to `skycd`.

## Use the API

```csharp
using SkyCD.Resource.Icons.LlmBackends;

using var ico = BackendIcons.OpenIcon("colibri");
using var png = BackendIcons.OpenPng("colibri", 16);
```

Open the ICO when your toolkit accepts multi-resolution icons. Open a PNG frame when your UI needs a particular size. Both methods return `null` when the requested resource is missing. The caller disposes returned streams.

## Platform support

The DLL targets .NET Standard 2.0 and consists of managed code and embedded images. Use the same DLL on any platform with a .NET implementation that supports .NET Standard 2.0. GitHub Actions checks the package on Windows, Linux, and macOS with .NET 10.

## Build and verify

Use the .NET 10 SDK and initialize the upstream submodules.

```sh
git submodule update --init
dotnet build SkyCD.Resource.Icons.LlmBackends.csproj -c Release
dotnet pack SkyCD.Resource.Icons.LlmBackends.csproj -c Release --no-build -p:PackageVersion=0.1.1 -o artifacts
dotnet run --project tools/IconBuild -c Release --no-build -- verify artifacts/SkyCD.Resource.Icons.LlmBackends.0.1.1.nupkg
```

MSBuild runs the .NET icon generator during the build. SkiaSharp is used by the build tool; the NuGet package contains the portable DLL, documentation, and license notices. CI publishes a version when a matching tag such as `v0.1.1` is pushed from `main`.

## Licenses

The library code is [MIT](LICENSE). Artwork retains its upstream MIT or Apache-2.0 terms and applicable trademark conditions. Include the accompanying notices and license texts when redistributing the artwork. See [third-party notices](THIRD-PARTY-NOTICES.md).
