using Sentra.Dashboard.Models;

namespace Sentra.Dashboard.Services;

/// <summary>
/// A representative slice of the CIS Microsoft Windows / Edge / Office benchmarks.
/// Rule IDs and titles follow real CIS numbering conventions.
/// </summary>
public static class CisCatalog
{
    public const string AccountPolicies = "Account Policies";
    public const string LocalPolicies = "Local Policies";
    public const string Firewall = "Windows Defender Firewall";
    public const string AuditPolicy = "Advanced Audit Policy";
    public const string Edge = "Microsoft Edge";
    public const string Office = "Microsoft Office";

    public static readonly IReadOnlyList<CisRule> Rules =
    [
        // Account Policies
        new("1.1.1", "Enforce password history: 24 or more passwords remembered", AccountPolicies, CisLevel.Level1, "24 passwords"),
        new("1.1.2", "Maximum password age: 365 or fewer days, but not 0", AccountPolicies, CisLevel.Level1, "365 days"),
        new("1.1.4", "Minimum password length: 14 or more characters", AccountPolicies, CisLevel.Level1, "14 characters"),
        new("1.1.5", "Password must meet complexity requirements: Enabled", AccountPolicies, CisLevel.Level1, "Enabled"),
        new("1.2.1", "Account lockout duration: 15 or more minutes", AccountPolicies, CisLevel.Level1, "15 minutes"),
        new("1.2.2", "Account lockout threshold: 5 or fewer invalid attempts", AccountPolicies, CisLevel.Level1, "5 attempts"),

        // Local Policies
        new("2.2.21", "Deny log on through Remote Desktop Services: Guests, Local account", LocalPolicies, CisLevel.Level1, "Guests, Local account"),
        new("2.3.1.1", "Accounts: Block Microsoft accounts", LocalPolicies, CisLevel.Level1, "Users can't add or log on"),
        new("2.3.7.1", "Interactive logon: Do not display last user name: Enabled", LocalPolicies, CisLevel.Level1, "Enabled"),
        new("2.3.7.4", "Interactive logon: Machine inactivity limit: 900 or fewer seconds", LocalPolicies, CisLevel.Level1, "900 seconds"),
        new("2.3.10.2", "Network access: Do not allow anonymous enumeration of SAM accounts", LocalPolicies, CisLevel.Level1, "Enabled"),
        new("2.3.11.7", "Network security: LAN Manager authentication level: NTLMv2 only", LocalPolicies, CisLevel.Level1, "Send NTLMv2 response only. Refuse LM & NTLM"),

        // Windows Defender Firewall
        new("9.1.1", "Windows Firewall: Domain: Firewall state: On", Firewall, CisLevel.Level1, "On (recommended)"),
        new("9.1.2", "Windows Firewall: Domain: Inbound connections: Block", Firewall, CisLevel.Level1, "Block (default)"),
        new("9.2.1", "Windows Firewall: Private: Firewall state: On", Firewall, CisLevel.Level1, "On (recommended)"),
        new("9.2.2", "Windows Firewall: Private: Inbound connections: Block", Firewall, CisLevel.Level1, "Block (default)"),
        new("9.3.1", "Windows Firewall: Public: Firewall state: On", Firewall, CisLevel.Level1, "On (recommended)"),
        new("9.3.5", "Windows Firewall: Public: Apply local firewall rules: No", Firewall, CisLevel.Level2, "No"),

        // Advanced Audit Policy
        new("17.1.1", "Audit Credential Validation: Success and Failure", AuditPolicy, CisLevel.Level1, "Success and Failure"),
        new("17.2.1", "Audit Application Group Management: Success and Failure", AuditPolicy, CisLevel.Level1, "Success and Failure"),
        new("17.5.1", "Audit Account Lockout: Failure", AuditPolicy, CisLevel.Level1, "Failure"),
        new("17.5.4", "Audit Logon: Success and Failure", AuditPolicy, CisLevel.Level1, "Success and Failure"),
        new("17.7.1", "Audit Audit Policy Change: Success", AuditPolicy, CisLevel.Level1, "Success"),
        new("17.9.1", "Audit Security State Change: Success", AuditPolicy, CisLevel.Level1, "Success"),

        // Microsoft Edge
        new("E.1.4", "SmartScreen protection: Enabled", Edge, CisLevel.Level1, "Enabled"),
        new("E.1.7", "Prevent bypassing SmartScreen warnings about downloads", Edge, CisLevel.Level1, "Enabled"),
        new("E.2.2", "Default Adobe Flash setting: Block", Edge, CisLevel.Level1, "BlockPlugins"),
        new("E.3.1", "Enable site isolation for every site", Edge, CisLevel.Level1, "Enabled"),
        new("E.4.2", "Allow user-level native messaging hosts: Disabled", Edge, CisLevel.Level2, "Disabled"),
        new("E.5.1", "Enable saving passwords to the password manager: Disabled", Edge, CisLevel.Level2, "Disabled"),

        // Microsoft Office
        new("OFC.1.1", "Block macros from running in Office files from the Internet", Office, CisLevel.Level1, "Enabled"),
        new("OFC.1.3", "VBA Macro Notification Settings: Disable with notification", Office, CisLevel.Level1, "Disable all with notification"),
        new("OFC.2.1", "Disable Trust Bar notification for unsigned application add-ins", Office, CisLevel.Level1, "Enabled"),
        new("OFC.3.2", "Protected View for files originating from the Internet: Enabled", Office, CisLevel.Level1, "Enabled"),
        new("OFC.4.1", "Disable all ActiveX controls: Enabled", Office, CisLevel.Level2, "Enabled"),
        new("OFC.5.2", "Automatically download content for e-mail from the Internet: Disabled", Office, CisLevel.Level1, "Disabled"),
    ];
}
