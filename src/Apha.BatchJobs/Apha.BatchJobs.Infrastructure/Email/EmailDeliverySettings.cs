namespace Apha.BatchJobs.Infrastructure.Email;

/// <summary>
/// Email delivery settings — explicit redirect configuration, shared by every job's
/// outgoing email. Bound from the "EmailDelivery" section.
/// </summary>
public sealed class EmailDeliverySettings
{
    public const string SectionName = "EmailDelivery";

    /// <summary>
    /// Sole authority over redirect behavior — never inferred from environment name or any
    /// other implicit signal. False (the default) sends to the real recipient list unchanged.
    /// </summary>
    public bool RedirectEnabled { get; set; } = false;

    /// <summary>
    /// The single address every email is sent to instead when <see cref="RedirectEnabled"/>
    /// is true. Required whenever redirect is active — an empty value fails the send rather
    /// than risk delivering to a real recipient.
    /// </summary>
    public string RedirectTo { get; set; } = string.Empty;
}
