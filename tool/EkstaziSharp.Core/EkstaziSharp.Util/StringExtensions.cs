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

using System;
using System.Text;
using System.ComponentModel;
using System.Xml;
using Crc32C;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace EkstaziSharp.Util
{
    public static class StringExtensions
    {
        public static string ToChecksum(this string text)
        {
            uint hash = Crc32CAlgorithm.Compute(Encoding.ASCII.GetBytes(text));
            return hash.ToString();
        }

        /// <summary>
        /// Converts input string to the type T.
        /// If string is null or conversion is not possible throws <see cref="NotSupportedException"/>
        /// </summary>
        public static T Convert<T>(this string input)
        {
            if (input == null)
            {
                throw new NotSupportedException("Null string cannot be converted");
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                return (T)converter.ConvertFromString(input);
            }

            return default(T);
        }

        /// <summary>
        /// Converts input string to <see cref="XmlDocument"/>.
        /// Requires: <paramref name="xml"/> represents valid xml.
        /// </summary>
        public static XmlDocument ToXmlDocument(this string xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        /// <summary>
        /// Converts input string to <see cref="XmlElement"/>.
        /// Requires: <paramref name="xml"/> represents valid xml.
        /// </summary>
        public static XmlElement ToXmlElement(this string xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            return document.DocumentElement;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static string RemoveEmptyLines(this string text)
        {
            return Regex.Replace(text, @"^\s*$\n|\r", "", RegexOptions.Multiline).TrimEnd();
        }

        public static string CutTillFirstOccurence(this string text, char character)
        {
            if (text.IsNullOrEmpty())
            {
                return text;
            }

            int cutIndex = text.IndexOf(character);
            return text.Substring(0, cutIndex);
        }
    }
}
