/*
SomeHelpers.cs
06.10.2021 22:22:26
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    /// <summary>
    /// Defines the <see cref="SomeHelpers" />.
    /// </summary>
    public static class SomeHelpers
    {
        /// <summary>
        /// The TruncateString.
        /// </summary>
        /// <param name="sourceStr">The sourceStr<see cref="string"/>.</param>
        /// <param name="length">The length<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string TruncateString(string sourceStr, int length)
        {
            if (sourceStr.Length > length)
            {
                sourceStr = sourceStr.Substring(0, length);
            }
            return sourceStr;
        }
    }
}
