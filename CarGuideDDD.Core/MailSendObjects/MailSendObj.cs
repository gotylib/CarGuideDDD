using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Core.MailSendObjects;

public class MailSendObj
{
    public MailCar? Car { get; init; }
    public MailUser? User { get; set; }
    public MailUser? Manager { get; init; }

    public int Score { get; set; }
}