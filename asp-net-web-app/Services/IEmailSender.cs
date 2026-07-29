namespace asp_net_web_app.Services
{
    // A tiny abstraction over "send an email".
    //
    // Right now the only implementation (DevEmailSender) just prints the
    // verification link to the console, so we can build and test the whole
    // sign-up + verify flow without a real mail server.
    //
    // Later, a real SmtpEmailSender can implement this same interface, and the
    // ONLY change needed is the single registration line in Program.cs.
    public interface IEmailSender
    {
        Task SendVerificationEmailAsync(string toEmail, string verificationLink);
    }
}
