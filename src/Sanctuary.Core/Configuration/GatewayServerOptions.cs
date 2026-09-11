using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Core.Configuration;

public sealed class GatewayServerOptions : ServerOptions
{
    /// <summary>
    /// The environment identifier reported to the client.
    /// Use <c>intl</c> when configuring a Chinese client.
    /// </summary>
    /// <example>live</example>
    [Required]
    public required string Environment { get; set; }

    /// <summary>
    /// Client version the server supports.
    /// </summary>
    /// <example>1.910.1.530630</example>
    [Required]
    public required string ClientVersion { get; set; }

    [Required]
    public required string ServerAddress { get; set; }

    [Required]
    public required string LoginGatewayAddress { get; set; }

    [Required]
    public required string LoginGatewayChallenge { get; set; }

    public bool ShowMemberNagScreen { get; set; }

    /// <summary>
    /// Give admin and mod accounts the community staff jobs (Referee, Enforcer) at level 20. These were never in
    /// the original game; off for the offline build, where admin only unlocks the '!' console commands.
    /// </summary>
    public bool GrantStaffProfiles { get; set; }
}