namespace MassMonitor;

public class MailSetting
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
}

public class SmsSetting
{
    public string Account { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    
    public string Phone { get; init; } = string.Empty;
}
public class SettingModel
{
    public string LogPath { get; init; } = string.Empty;
    public string? LineToken { get; init; }
    
    public MailSetting? MailSetting { get; set; }
    
    public SmsSetting? SmsSetting { get; set; }
}