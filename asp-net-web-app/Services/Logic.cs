using asp_net_web_app.Data;

public class UserLogic
{
    private readonly DatabaseWrapper _db;

    public UserLogic(DatabaseWrapper db)
    {
        _db = db;
    }

    public string AddUser(string username)
    {
        if (username != "")
        {
            username = username.Trim();
            if (string.IsNullOrWhiteSpace(username)) return "characters or numbers required";
            if (username.Length < 3) return "too short";
            if (username.Length > 50) return "too long";

            _db.AddUser(username);
            return "success";
        }
        return "empty";
    }

    public void AddReservation(string s)
    {
        _db.AddReservation(s);
    }
}
