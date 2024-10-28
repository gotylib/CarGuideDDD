using CarGuideDDD.MailServiceConsole.Objects;
using MimeKit;


namespace CarGuideDDD.MailServiceConsole.Senders
{
    public static class MessageSender
    {
        public static string password = "MJD1WjnSZrsWprd6guJi";
        public static string sender = "mixalev702@mail.ru";
        public static void SendMessage(Message sendMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Покупка машин", sender));
            message.To.Add(new MailboxAddress("Получатель", sendMessage.MailRecipient));
            message.Subject = sendMessage.Subject;

            var builder = new BodyBuilder();
            builder.TextBody = sendMessage.Body;


            // Отправка сообщения
            message.Body = builder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect("smtp.mail.ru", 465, true);
                client.Authenticate(sender, password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
