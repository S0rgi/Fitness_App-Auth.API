namespace Gainly_Auth_API.Models;
public class NotificationMessage
{
    public string Type { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string RecipientEmail { get; set; } = "";
    public string? Action { get; set; }
}



