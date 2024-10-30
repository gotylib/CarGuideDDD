namespace CarGuideDDD.MailService.Objects;

public class MailSendObj
{
    public Car? Car { get; init; }
    public User? User { get; init; }
    public User? Manager { get; init; }
}