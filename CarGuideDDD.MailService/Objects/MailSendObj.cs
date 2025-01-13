namespace CarGuideDDD.MailService.Objects;

public class MailSendObj
{
    int EventId { get; set; }
    public Car? Car { get; init; }
    public User? User { get; init; }
    public User? Manager { get; init; }
    
    public int Score { get; set; }
}