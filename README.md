# Dotazure

[![releases](https://img.shields.io/github/v/release/heaths/dotazure.net.svg?logo=github)](https://github.com/heaths/dotazure.net/releases/latest)
[![ci](https://github.com/heaths/dotazure.net/actions/workflows/ci.yml/badge.svg?event=push)](https://github.com/heaths/dotazure.net/actions/workflows/ci.yml)

Locate and load environment variables defined when provisioning an [Azure Developer CLI] project.

## Getting Started

If you do not already have an [Azure Developer CLI] (azd) project, you can create one:

```sh
azd init
```

After you define some resources e.g., an [Azure Key Vault](https://github.com/heaths/dotazure-rs/blob/main/infra/resources.bicep),
you can provision those resources which will create a `.env` file with any `output` parameters:

```sh
azd up
```

## Example

After `azd up` provisions resources and creates a `.env` file, you can call `Loader.Load()` to load those environment variables
from the default environment e.g.,

```csharp
using DotAzure;

Loader.Load();

// Assumes bicep contains e.g.
//
// output AZURE_KEYVAULT_URL string = kv.properties.vaultUri
Console.WriteLine($"AZURE_KEYVAULT_URL={Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URL")}");
```

If you want to customize behavior, you can call `Loader.Load()` with `LoadOptions` to set a builder-like `AzdContext` object.

## License

Licensed under the [MIT](https://github.com/heaths/dotazure/blob/refactor/LICENSE.txt) license.

[Azure Developer CLI]: https://aka.ms/azd
