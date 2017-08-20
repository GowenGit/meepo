using System;
using System.Text;
using System.Threading.Tasks;

namespace Meepo.Core.Extensions
{
    public static class MeepoExtensions
    {
        /// <summary>
        /// Encode string to bytes.
        /// </summary>
        /// <param name="message">Message to encode</param>
        /// <returns></returns>
        public static byte[] Encode(this string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        /// <summary>
        /// Decode string from bytes.
        /// </summary>
        /// <param name="bytes">Bytes to decode</param>
        /// <returns></returns>
        public static string Decode(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Send string to all nodes.
        /// </summary>
        /// <param name="meepo">Server instance</param>
        /// <param name="message">Message to send</param>
        public static Task SendAsync(this IMeepo meepo, string message)
        {
            return meepo.SendAsync(Encode(message));
        }

        /// <summary>
        /// Send string to all nodes.
        /// </summary>
        /// <param name="meepo">Server instance</param>
        /// <param name="id">Client ID</param>
        /// <param name="message">Message to send</param>
        public static Task SendAsync(this IMeepo meepo, Guid id, string message)
        {
            return meepo.SendAsync(id, Encode(message));
        }
    }
}
