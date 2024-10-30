using MimeKit;

namespace CarGuideDDD.MailService
{
    public static class MessageSender
    {
        private const string Password = "MJD1WjnSZrsWprd6guJi";
        private const string Sender = "mixalev702@mail.ru";

        public static void SendMessage(Message sendMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Покупка машин", Sender));
            message.To.Add(new MailboxAddress("Получатель", sendMessage.MailRecipient));
            message.Subject = sendMessage.Subject;

            var builder = new BodyBuilder();
            builder.TextBody = sendMessage.Body;


            // Отправка сообщения
            message.Body = builder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.Connect("smtp.mail.ru", 465, true);
            client.Authenticate(Sender, Password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
