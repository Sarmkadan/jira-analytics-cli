// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace JiraAnalyticsCli.Diagnostics;

/// <summary>
/// The DNS, TCP and TLS part of <c>doctor</c>. Each step is reported separately, so the hint says which layer to fix.
/// </summary>
internal static class JiraDoctorNetwork
{
    /// <summary>
    /// Resolves the Jira host, opens a TCP connection to its port, and completes a TLS handshake.
    /// </summary>
    /// <remarks>
    /// The TLS step runs only for https URLs. Configuration validation requires https, so the step is skipped
    /// only when a caller passes a plain http URL, for example a local test server.
    /// </remarks>
    /// <param name="baseUri">Jira base URL.</param>
    /// <param name="ct">Cancels the check.</param>
    public static async Task<DoctorCheckResult> CheckAsync(Uri baseUri, CancellationToken ct)
    {
        const string name = JiraDoctor.ReachabilityCheck;
        var host = baseUri.Host;
        var port = baseUri.Port;

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(host, ct);
        }
        catch (SocketException ex)
        {
            return DoctorCheckResult.Fail(name, $"DNS lookup for {host} failed: {ex.Message}", DnsHint);
        }

        if (addresses.Length == 0)
            return DoctorCheckResult.Fail(name, $"DNS returned no addresses for {host}", DnsHint);

        using var tcp = new TcpClient();
        try
        {
            await tcp.ConnectAsync(addresses, port, ct);
        }
        catch (SocketException ex)
        {
            return DoctorCheckResult.Fail(name,
                $"TCP connection to {host}:{port} failed: {ex.Message}",
                $"Check that outbound TCP port {port} to {host} is allowed by the firewall, proxy or VPN.");
        }

        if (!baseUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return DoctorCheckResult.Pass(name,
                $"resolved {host}, connected on port {port}; TLS not checked for {baseUri.Scheme}");

        using var stream = new SslStream(tcp.GetStream());
        try
        {
            await stream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions { TargetHost = host }, ct);
        }
        catch (Exception ex) when (ex is AuthenticationException or IOException)
        {
            return DoctorCheckResult.Fail(name,
                $"TLS handshake with {host}:{port} failed: {ex.Message}",
                "Check that BaseUrl is the Jira site over https. If a proxy or antivirus inspects HTTPS, its CA certificate must be trusted by this machine.");
        }

        return DoctorCheckResult.Pass(name,
            $"resolved {host} to {addresses.Length} address(es), connected on port {port}, TLS {stream.SslProtocol}");
    }

    private const string DnsHint =
        "Check the host name in BaseUrl (for example https://yourcompany.atlassian.net) and this machine's DNS or VPN settings.";
}
