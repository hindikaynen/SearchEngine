using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Tests
{
    public static class Utils
    {
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string RandomWord(int minLength = 3, int maxLength = 10)
        {
            var length = Random.Next(minLength, maxLength + 1);
            var result = new char[length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Alphabet[Random.Next(Alphabet.Length)];
            }
            return new string(result);
        }

        public static string RandomString()
        {
            return Guid.NewGuid().ToString();
        }

        public static int RandomInteger()
        {
            return Random.Next();
        }

        public static int RandomInteger(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }

        public static T RandomElement<T>(List<T> list)
        {
            return list[RandomInteger(0, list.Count)];
        }

        public static Guid[] RandomIds(int count)
        {
            var result = new Guid[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Guid.NewGuid();
            }
            return result;
        }
    }
}
