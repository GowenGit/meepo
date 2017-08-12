using System;

namespace Meepo.Core.Helpers
{
    internal static class ExceptionHelpers
    {
        public static string ToFormattedException(this Exception ex)
        {
            return $"Message: {ex.Message}{Environment.NewLine}" +
                   $"Inner Exception: {ex.InnerException?.Message}{Environment.NewLine}" +
                   $"Stack Trace: {ex.StackTrace}{Environment.NewLine}";
        }
    }
}
