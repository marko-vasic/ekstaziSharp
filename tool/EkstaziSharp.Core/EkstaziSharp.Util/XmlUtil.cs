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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EkstaziSharp.Util
{
    public static class XmlUtil
    {
        /// <summary>
        /// Reads value of attribute with a given name from xml node and converts it to T
        /// </summary>
        /// <throws><see cref="NotSupportedException"/> in case conversion to <paramref name="T"/> is not possible</throws>
        public static T ParseAttribute<T>(this XmlNode node, string attributeName)
        {
            XmlNode attribute = node.Attributes.GetNamedItem(attributeName);
            if (attribute == null)
            {
                throw new ArgumentOutOfRangeException($"Attribute {attributeName} cannot be parsed from the xml node");
            }
            return attribute.InnerText.Convert<T>();
        }

        public static List<XmlNode> GetChildren(this XmlNode node, string name)
        {
            List<XmlNode> nodes = new List<XmlNode>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == name)
                {
                    nodes.Add(childNode);
                }

                List<XmlNode> childNodes = GetChildren(childNode, name);
                if (childNodes != null && childNodes.Count > 0)
                {
                        nodes.AddRange(childNodes);
                }
            }

            return nodes;
        }
    }
}
