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

using System.Collections.Generic;
using Microsoft.Build.Framework;
using System.Text;
using System;

namespace EkstaziSharp.Tester
{
    /// <summary>
    /// Logs events that occur during build of a project.
    /// </summary>
    public class MSBuildLogger : ILogger
    {
        #region Fields

        /// <summary>
        /// Preserves error messages occured during a build
        /// </summary>
        private StringBuilder errorLog = new StringBuilder();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Errors occured during a build
        /// </summary>
        public string BuildErrors { get; private set; }

        /// <summary>
        /// This will gather info about the projects built
        /// </summary>
        public IList<string> BuildDetails { get; private set; }

        #endregion

        #region ILogger Properties

        /// <summary>
        /// Does not have effect in current implementation
        /// </summary>
        public LoggerVerbosity Verbosity { get; set; }

        public string Parameters { get; set; }

        #endregion

        #region ILogger Methods

        /// <summary>
        /// Initialize is guaranteed to be called by MSBuild at the start of the build
        /// before any events are raised.
        /// </summary>
        public void Initialize(IEventSource eventSource)
        {
            BuildDetails = new List<string>();

            eventSource.ProjectStarted += new ProjectStartedEventHandler(ProjectStartedHandler);
            eventSource.ErrorRaised += new BuildErrorEventHandler(ErrorRaisedHandler);
            eventSource.ProjectStarted += new ProjectStartedEventHandler(ProjectStartedHandler);
            eventSource.ProjectFinished += new ProjectFinishedEventHandler(ProjectFinishedHandler);
        }

        private void ProjectFinishedHandler(object sender, ProjectFinishedEventArgs e)
        {
            errorLog.AppendLine("Project Finished: " + e.Message);
        }

        /// <summary>
        /// Shutdown() is guaranteed to be called by MSBuild at the end of the build, after all
        /// events have been raised.
        /// </summary>
        public void Shutdown()
        {
            BuildErrors = errorLog.ToString();
        }

        #endregion

        #region Event Handlers

        private void ErrorRaisedHandler(object sender, BuildErrorEventArgs e)
        {
            errorLog.AppendLine($"ERROR {e.File}({e.LineNumber},{e.ColumnNumber}): {e.Message}");
        }

        private void ProjectStartedHandler(object sender, ProjectStartedEventArgs e)
        {
            BuildDetails.Add(e.Message);
            errorLog.AppendLine("Project Started: " + e.Message);
        }

        #endregion
    }
}