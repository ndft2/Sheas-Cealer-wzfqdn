namespace Sheas_Cealer.Models;

internal class CealHostRule
{
    internal CealHostRule(string domains, string? sni, string ip)
    {
        Domains = domains;
        Sni = sni;
        Ip = ip;
    }

    public string Domains { get; internal set; }
    public string? Sni { get; internal set; }
    public string Ip { get; internal set; }
}