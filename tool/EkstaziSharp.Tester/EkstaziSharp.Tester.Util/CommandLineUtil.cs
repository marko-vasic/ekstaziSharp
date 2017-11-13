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
using EkstaziSharp.Util;
using CommandLine;
using CommandLine.Text;

namespace EkstaziSharp.Tester.Util
{
    public class CommandLineUtil
    {
        #region Private Methods

        private static bool IsValueArgument(string[] args, int index)
        {
            if (index > args.Length)
            {
                return false;
            }

            string arg = args[index];
            return arg[0] != '-';
        }


        private static Parsed<T> GetParsedValues<T>(string[] args)
        {
            Parser parser = new Parser((parserSettings) =>
            {
                parserSettings.IgnoreUnknownArguments = true;
            });
            ParserResult<T> parserResult = parser.ParseArguments<T>(args);
            return parserResult as Parsed<T>;
        }

        public static T ParseArguments<T>(string[] args)
        {
            //            string[] filteredArguments = FilterUnknownArguments<T>(args);
            //            ParserResult<T> parserResult = Parser.Default.ParseArguments<T>(filteredArguments);
            Parsed<T> parsedValue = GetParsedValues<T>(args);
            if (parsedValue != null)
            {
                return parsedValue.Value;
            }
            else
            {
                return default(T);
            }
        }

        #endregion

        #region Public Methods

        public static void FilterArguments<T>(string[] args, out string[] knownArgs, out string[] unknownArgs)
        {
            Parser parser = new Parser((parserSettings) =>
            {
                parserSettings.HelpWriter = null;
            });

            ParserResult<T> parserResult = parser.ParseArguments<T>(args);
            if (parserResult.Tag == ParserResultType.NotParsed)
            {
                List<int> unknownArgsIndexes = new List<int>();
                List<string> unknownArgsList = new List<string>();

                NotParsed<T> info = parserResult as NotParsed<T>;
                foreach (var error in info.Errors)
                {
                    if (error.Tag == ErrorType.UnknownOptionError)
                    {
                        UnknownOptionError unknownOptionError = error as UnknownOptionError;
                        int uknownArgIndex = Array.FindIndex(args, (arg) => 
                        {
                            return arg.Equals("--" + unknownOptionError.Token) || arg.Equals("-" + unknownOptionError.Token);
                        });
                        unknownArgsIndexes.Add(uknownArgIndex);
                        unknownArgsList.Add(args[uknownArgIndex]);

                        for (int argIndex = uknownArgIndex + 1; argIndex < args.Length; argIndex++) 
                        {
                            if (IsValueArgument(args, argIndex))
                            {
                                unknownArgsIndexes.Add(argIndex);
                                unknownArgsList.Add(args[argIndex]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                unknownArgs = unknownArgsList.ToArray();

                if (unknownArgsIndexes.Count < args.Length)
                {
                    // if there is some argument left
                    string[] filteredArguments = new string[args.Length - unknownArgsIndexes.Count];
                    int lastAddedArgIndex = 0;
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (!unknownArgsIndexes.Contains(i))
                        {
                            filteredArguments[lastAddedArgIndex++] = args[i];
                        }
                    }
                    knownArgs = filteredArguments;
                    return;
                }
                else
                {
                    knownArgs = new string[0];
                    return;
                }
            }
            unknownArgs = new string[0];
            knownArgs = args;
        }

        public static string[] FilterUnknownArguments<T>(string[] args)
        {
            string[] unknownArgs;
            string[] knownArgs;
            FilterArguments<T>(args, out knownArgs, out unknownArgs);
            return unknownArgs;
        }

        public static string[] FilterKnownArguments<T>(string[] args)
        {
            string[] unknownArgs;
            string[] knownArgs;
            FilterArguments<T>(args, out knownArgs, out unknownArgs);
            return knownArgs;
        }

        public static void FilterArguments(string[] args, Type type, out string[] knownArgs, out string[] unknownArgs)
        {
            unknownArgs = CSharpUtil.InvokeGenericMethodByType(
                typeof(CommandLineUtil),
                "FilterUnknownArguments",
                null,
                new object[] { args },
                type) as string[];

            knownArgs = CSharpUtil.InvokeGenericMethodByType(
                typeof(CommandLineUtil),
                "FilterKnownArguments",
                null,
                new object[] { args },
                type) as string[];
        }

        public static int FindFirstUknownArg<T>(string[] args)
        {
            Parser parser = new Parser((parserSettings) =>
            {
                parserSettings.HelpWriter = null;
            });

            ParserResult<T> parserResult = parser.ParseArguments<T>(args);

            if (parserResult.Tag == ParserResultType.NotParsed)
            {
                List<int> unknownArgsIndexes = new List<int>();

                NotParsed<T> info = parserResult as NotParsed<T>;
                foreach (var error in info.Errors)
                {
                    if (error.Tag == ErrorType.UnknownOptionError)
                    {
                        UnknownOptionError unknownOptionError = error as UnknownOptionError;
                        int uknownArgIndex = Array.FindIndex(args, (arg) => 
                        {
                            if (unknownOptionError.Token.Length == 1)
                            {
                                return arg.StartsWith("-" + unknownOptionError.Token);
                            }
                            else
                            {
                                return arg.Equals("--" + unknownOptionError.Token);
                            }
                        });
                        unknownArgsIndexes.Add(uknownArgIndex);
                    }
                }

                if (unknownArgsIndexes.Count == 0)
                {
                    return -1;
                }
                int min = int.MaxValue;
                foreach (int index in unknownArgsIndexes)
                {
                    if (index < min) min = index;
                }
                return min;
            }
            return -1;
        }

        public static int FindFirstUknownArg(string[] args, Type type)
        {
            object result = CSharpUtil.InvokeGenericMethodByType(
                typeof(CommandLineUtil),
                "FindFirstUknownArg",
                null,
                new object[] { args },
                type);
            return Convert.ToInt32(result);
        }

        public static object ParseArguments(string[] args, Type type)
        {
            return CSharpUtil.InvokeGenericMethodByType(
                typeof(CommandLineUtil),
                "ParseArguments", 
                null, 
                new object[] { args }, 
                type);
        }

        public static string HelpText<T>(string[] args)
        {
            Parsed<T> parsedValue = GetParsedValues<T>(args);
            if (parsedValue == null)
            {
                return string.Empty;
            }
            HelpText help = new HelpText();
            help.AddDashesToOption = true;
            help.AdditionalNewLineAfterOption = true;
            help.AddOptions<T>(parsedValue);
            string helpString = help.ToString();
            int helpOptionIndex = helpString.IndexOf("--help");
            helpString = helpString.Substring(0, helpOptionIndex);
            return helpString.RemoveEmptyLines();
        }

        public static string HelpText(string[] args, Type type)
        {
            return CSharpUtil.InvokeGenericMethodByType(
                typeof(CommandLineUtil),
                "HelpText", 
                null, 
                new object[] { args }, 
                type) as string;
        }

        #endregion
    }
}
