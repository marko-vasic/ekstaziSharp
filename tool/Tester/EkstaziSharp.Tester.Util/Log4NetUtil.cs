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
using System.IO;
using System.Xml;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using log4net.Config;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester.Util
{
    public class Log4NetUtil
    {
        private const string Log4NetResourceName = "log4net";
        private const string Log4NetDebugResourceName = "log4net_debug";

        public static void InitializeLoggers(bool debugMode)
        {
            string logFilePath = Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "log.txt");
            Environment.SetEnvironmentVariable("EkstaziLogFilePath", logFilePath);
            string log4netConfig = null;
            if (debugMode)
            {
                log4netConfig = Resource.ResourceManager.GetString(Log4NetDebugResourceName);
            }
            else
            {
                log4netConfig = Resource.ResourceManager.GetString(Log4NetResourceName);
            }
            XmlElement log4netXml = log4netConfig.ToXmlElement();
            XmlConfigurator.Configure(log4netXml);
        }

        // Set the level for a named logger
        public static void SetLevel(string loggerName, string levelName)
        {
            ILog log = LogManager.GetLogger(loggerName);
            Logger l = (Logger)log.Logger;

            l.Level = l.Hierarchy.LevelMap[levelName];
        }

        // Add an appender to a logger
        public static void AddAppender(string loggerName, IAppender appender)
        {
            ILog log = LogManager.GetLogger(loggerName);
            Logger l = (Logger)log.Logger;

            l.AddAppender(appender);
        }

        // Create a new file appender
        public static IAppender CreateFileAppender(string name, string fileName)
        {
            FileAppender appender = new FileAppender();
            appender.Name = name;
            appender.File = fileName;
            appender.AppendToFile = true;

            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%date %level %logger - %message%newline";
            layout.ActivateOptions();

            appender.Layout = layout;
            appender.ActivateOptions();

            return appender;
        }

        public static IAppender CreateConsoleAppender(string name)
        {
            ConsoleAppender appender = new ConsoleAppender();
            appender.Name = name;

            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%date %level %logger - %message%newline";
            layout.ActivateOptions();

            appender.Layout = layout;
            appender.ActivateOptions();

            return appender;
        }

        // In order to set the level for a logger and add an appender reference you
        // can then use the following calls:
        //SetLevel("Log4net.MainForm", "ALL");
        //AddAppender("Log4net.MainForm", CreateFileAppender("appenderName", "fileName.log"));
    }
}