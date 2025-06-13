﻿namespace Jobbr.Server.WebAPI.Infrastructure;

public static class StringExtensions
{
    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return char.ToLower(input[0]) + input[1..];
    }
}