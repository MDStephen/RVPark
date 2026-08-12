using Microsoft.AspNetCore.Mvc.RazorPages;


namespace asp_net_web_app.Pages;

public class DashboardModel : PageModel
{
    public string UserDisplayName { get; set; } = "Team Member";
    public string InitialServerTime { get; set; } = string.Empty;
    public string TimeZoneName { get; set; } = string.Empty;

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name))
        {
            UserDisplayName = User.Identity.Name;
        }

        // Capture initial server date/time and timezone display
        var now = DateTime.Now;
        InitialServerTime = now.ToString("yyyy-MM-ddTHH:mm:ss");
        TimeZoneName = TimeZoneInfo.Local.IsDaylightSavingTime(now) 
            ? TimeZoneInfo.Local.DaylightName
            : TimeZoneInfo.Local.StandardName;

        
        /* // Just for debugging authorization
        Console.WriteLine("----------------------------------------------------------------");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"CLAIM: {claim.Type} = {claim.Value}");
        }

        Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"IsAdmin: {User.IsInRole("Admin")}");
        Console.WriteLine($"IsEmployee: {User.IsInRole("Employee")}");
        */
    }
}
