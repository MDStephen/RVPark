namespace asp_net_web_app.Services
{
    // Development-only email sender.
    //
    // Instead of actually emailing anyone, it prints the verification link to
    // the console (the terminal running `dotnet run`). During testing you copy
    // that link out of the terminal and paste it into your browser.
    public class DevEmailSender : IEmailSender
    {
        public Task SendVerificationEmailAsync(string toEmail, string verificationLink)
        {
            // A loud, easy-to-spot block so the link is simple to find in the log.
            Console.WriteLine();
            Console.WriteLine("==================== DEV EMAIL ====================");
            Console.WriteLine($"To:      {toEmail}");
            Console.WriteLine("Subject: Confirm your RV Park account");
            Console.WriteLine($"Verify:  {verificationLink}");
            Console.WriteLine("==================================================");
            Console.WriteLine();

            // Nothing async is actually happening yet, so hand back a finished Task.
            return Task.CompletedTask;
        }
    }
}
