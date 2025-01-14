using CarGuideDDD.MailService.Objects;

namespace CarGuideDDD.MailService.Services.Interfaces;

public interface IMailServices
{
    public bool SendUserNotFountManagerMessage(User? user);

    public bool SendUserNoHaveCarMessage(User? user, Car? car);

    public bool SendBuyCarMessage(User? user, User? manager, Car? car);

    public bool SendInformCarMessage(User? user, User? manager, Car? car);

    public bool SendReminderToAddCar(User? user);

}