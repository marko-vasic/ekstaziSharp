// Copyright (c) 2017, Marko Vasic
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace EkstaziSharp.Util
{
    public static class Util
    {
        public const string DefaultPathReplacementChar = "_";

        public static string CleanFileName(this string fileName, string replacement = "")
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), replacement));
        }

        /// <summary>
        /// Returns type name in ECMA-335 specification.
        /// Note that Mono.Cecil TypeDefinition.FullName uses that standard
        /// while System.Reflection Type.FullName does not
        /// </summary>
        public static string GetECMA335Name(this string typeFullName)
        {
            return typeFullName.Replace('+', '/');
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        public static string HexStringFromBytes(this byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public static string Hashed(this string text)
        {
            var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
            return hash.HexStringFromBytes();
        }

        public static bool IsFilePathTooLong(this string filePath)
        {
            return filePath.Length > 250;
        }
    }
}
