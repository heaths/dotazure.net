// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotAzure;

const string EnvironmentVariableName = "AZURE_KEYVAULT_URL";

if (Loader.Load())
{
    Console.Error.WriteLine("loaded environment variables");
}

var endpoint = new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableName) ??
    throw new Exception($"{EnvironmentVariableName} not set"), UriKind.Absolute);
var credential = new AzureDeveloperCliCredential();
var client = new SecretClient(endpoint, credential);

KeyVaultSecret secret = await client.GetSecretAsync("my-secret");
Console.WriteLine(secret.Value ?? "(none)");
