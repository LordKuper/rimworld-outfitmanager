﻿using System;

namespace LordKuper.OutfitManager
{
    /// <summary>
    ///     Provides logging functionality for the OutfitManager mod.
    /// </summary>
    internal static class Logger
    {
        /// <summary>
        ///     Logs an error message with optional exception details.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">The exception associated with the error, or <c>null</c> if none.</param>
        internal static void LogError(string message, Exception exception = null)
        {
            Common.Logger.LogError(OutfitManagerMod.ModId, message, exception);
        }

        /// <summary>
        ///     Logs a general informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        internal static void LogMessage(string message)
        {
            Common.Logger.LogMessage(OutfitManagerMod.ModId, message);
        }

        /// <summary>
        ///     Logs a warning message with optional exception details.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        /// <param name="exception">The exception associated with the warning, or <c>null</c> if none.</param>
        internal static void LogWarning(string message, Exception exception = null)
        {
            Common.Logger.LogWarning(OutfitManagerMod.ModId, message, exception);
        }
    }
}