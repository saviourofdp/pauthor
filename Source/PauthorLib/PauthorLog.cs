//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.LiveLabs.Pauthor
{
    /// <summary>
    /// PauthorLog provides a simple logging framework so that status messages and the like may be captured from classes
    /// in the Pauthor library and directed to a useful location.
    /// </summary>
    /// <remarks>
    /// All classes in the Pauthor library which emit status messages write to the <see cref="PauthorLog.Global"/>
    /// instance of this class.
    /// </remarks>
    public class PauthorLog
    {
        /// <summary>
        /// A shared global log used by all Pauthor classes which emit status messages.
        /// </summary>
        public static readonly PauthorLog Global = new PauthorLog();

        /// <summary>
        /// Level is an enumeration which defineds the relative importance and purpose of any given log message.
        /// </summary>
        public enum Level
        {
            /// <summary>
            /// The log's <see cref="PauthorLog.CurrentLevel"/> may be set to this value to disable all logging.
            /// </summary>
            Off,

            /// <summary>
            /// Used for important messages which should be visible so long as logging is enabled.
            /// </summary>
            Message,
            
            /// <summary>
            /// Used for outright failures during processing.
            /// </summary>
            Error,
            
            /// <summary>
            /// Used for conditions which may indicate that processing will produce an unexpected or undesired result.
            /// </summary>
            Warning,
            
            /// <summary>
            /// Used to indicate progress with a specific operation.
            /// </summary>
            Progress,
            
            /// <summary>
            /// Used to provide extra details about each step of an operation.
            /// </summary>
            Verbose,
            
            /// <summary>
            /// The log's <see cref="CurrentLevel"/> may be set to this value to show all log entries.
            /// </summary>
            All
        };

        /// <summary>
        /// Creates a new log with at <see cref="Level.Progress"/> level.
        /// </summary>
        public PauthorLog() : this(Level.Progress)
        {
            this.ConsoleStream = System.Console.Out;
        }

        /// <summary>
        /// Creates a new log set to the given level.
        /// </summary>
        /// <param name="level">the level of log entry which should be printed</param>
        public PauthorLog(Level level)
        {
            this.CurrentLevel = level;
        }

        /// <summary>
        /// The current level for this log.
        /// </summary>
        /// <remarks>
        /// Only log entries whose level is equal to or greater than the current level will be displayed.
        /// </remarks>
        public Level CurrentLevel
        {
            get { return m_currentLevel; }

            set { m_currentLevel = value; }
        }

        /// <summary>
        /// The destination to which log messages will be written.
        /// </summary>
        /// <remarks>
        /// If set to null, output will be sent to Console.Out. By default, this property is set to Console.Out;
        /// </remarks>
        public TextWriter ConsoleStream
        {
            get { return m_consoleStream; }

            set
            {
                m_consoleStream = value ?? System.Console.Out;
            }
        }

        /// <summary>
        /// Writes a log entry at <see cref="Level.Message"/> level.
        /// </summary>
        /// <param name="message">a string template for the message (see: <see cref="String.Format(String,Object)"/>
        /// )</param>
        /// <param name="args">the argument for the given template</param>
        public void Message(String message, params Object[] args)
        {
            this.WriteToConsole(Level.Message, message, args);
        }

        /// <summary>
        /// Writes a log entry at <see cref="Level.Error"/> level.
        /// </summary>
        /// <param name="message">a string template for the message (see: <see cref="String.Format(String,Object)"/>
        /// )</param>
        /// <param name="args">the argument for the given template</param>
        public void Error(String message, params Object[] args)
        {
            this.WriteToConsole(Level.Error, message, args);
        }

        /// <summary>
        /// Writes a log entry at <see cref="Level.Warning"/> level.
        /// </summary>
        /// <param name="message">a string template for the message (see: <see cref="String.Format(String,Object)"/>
        /// )</param>
        /// <param name="args">the argument for the given template</param>
        public void Warning(String message, params Object[] args)
        {
            this.WriteToConsole(Level.Warning, message, args);
        }

        /// <summary>
        /// Writes a log entry at <see cref="Level.Progress"/> level.
        /// </summary>
        /// <param name="message">a string template for the message (see: <see cref="String.Format(String,Object)"/>
        /// )</param>
        /// <param name="args">the argument for the given template</param>
        public void Progress(String message, params Object[] args)
        {
            this.WriteToConsole(Level.Progress, message, args);
        }

        /// <summary>
        /// Writes a log entry at <see cref="Level.Verbose"/> level.
        /// </summary>
        /// <param name="message">a string template for the message (see: <see cref="String.Format(String,Object)"/>
        /// )</param>
        /// <param name="args">the argument for the given template</param>
        public void Verbose(String message, params Object[] args)
        {
            this.WriteToConsole(Level.Verbose, message, args);
        }

        private void WriteToConsole(Level level, String message, params Object[] args)
        {
            if (level > m_currentLevel) return;

            String fullMessage = String.Format(message, args);
            StringReader reader = new StringReader(fullMessage);
            while (true)
            {
                String line = reader.ReadLine();
                if (line == null) break;

                this.ConsoleStream.WriteLine("[{0}: {1,8}] {2}", DateTime.Now.ToString("s"), level, line);
            }
        }

        private Level m_currentLevel;

        private TextWriter m_consoleStream;
    }
}
