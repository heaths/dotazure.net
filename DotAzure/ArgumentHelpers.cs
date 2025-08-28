// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace DotAzure
{
    internal static class ArgumentHelpers
    {
        public static void ThrowIfNull(string argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void ThrowIfNullOrWhiteSpace(string argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            ThrowIfNull(argument, paramName);
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException($"{paramName} cannot be empty or contain only white space", paramName);
            }
        }
    }
}

#if NETSTANDARD
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
#endif
