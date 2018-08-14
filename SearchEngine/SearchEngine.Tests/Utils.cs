﻿using System;

namespace SearchEngine.Tests
{
    public static class Utils
    {
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string RandomWord(int maxLength)
        {
            var length = Random.Next(1, maxLength + 1);
            var result = new char[length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Alphabet[Random.Next(Alphabet.Length)];
            }
            return new string(result);
        }
    }
}
