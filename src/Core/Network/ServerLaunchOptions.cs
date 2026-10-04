using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Aetherium.Core.Config;

namespace Aetherium.Core.Network
{
    [Serializable]
    public sealed class ServerLaunchOptions
    {
        public const ushort DefaultUdpPort = 27015;
        private static readonly Regex FourDigitRoomCode = new Regex(@"^\d{4}$", RegexOptions.Compiled);

        /// <summary>
        /// Full internal session name or a bare four-digit room code.
        /// Defaults to <see cref="FusionSessionConfig.DefaultDedicatedSessionName"/>.
        /// </summary>
        public string SessionName = FusionSessionConfig.DefaultDedicatedSessionName;

        /// <summary>
        /// UDP listen port for the dedicated game server. The default is stable
        /// for router forwarding; zero opts into Fusion's automatic selection.
        /// </summary>
        public ushort UdpPort = DefaultUdpPort;

        /// <summary>
        /// Optional public IPv4 advertised to clients. Cloud instances with a known
        /// public-to-local UDP mapping can use this instead of STUN discovery.
        /// </summary>
        public string PublicIp;

        public static ServerLaunchOptions CreateDefault()
        {
            return new ServerLaunchOptions();
        }

        public static bool TryNormalizePublicIp(string input, out string publicIp)
        {
            publicIp = null;
            if (!IPAddress.TryParse(input?.Trim(), out IPAddress address) ||
                address.AddressFamily != AddressFamily.InterNetwork ||
                address.Equals(IPAddress.Any) ||
                address.Equals(IPAddress.Broadcast) ||
                IPAddress.IsLoopback(address))
            {
                return false;
            }

            publicIp = address.ToString();
            return true;
        }

        /// <summary>
        /// Accepts a four-digit room code or a standard internal session name.
        /// Rejects free-form strings so a typo cannot silently open a random room.
        /// </summary>
        public static bool TryNormalizeSessionName(string input, out string sessionName)
        {
            sessionName = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            string trimmed = input.Trim();
            if (FourDigitRoomCode.IsMatch(trimmed))
            {
                sessionName = FusionSessionConfig.DEFAULT_SESSION_NAME_PREFIX + trimmed;
                return true;
            }

            if (FusionSessionConfig.TryExtractRoomCode(trimmed, out _))
            {
                sessionName = trimmed;
                return true;
            }

            return false;
        }
    }
}
