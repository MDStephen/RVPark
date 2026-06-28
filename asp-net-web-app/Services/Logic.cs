public class Userlogic {
    private DatabaseWrapper dw;
    private List<User> userList;

    public UserLogic(DatabaseWrapper dw){
        this.dw = dw;
        this.userList = dw.GetUsers();
    }

    public String AddUser(string username){
        if (username != ""){
            username = username.Trim();
            if (string.IsNullOrWhiteSpace(username)) return "characters or numbers required";
            if (username.Length < 3) return "too short";
            if (username.Length > 50) return "too long";

            dw.AddUser(username);
            return "success";
        }
        return "empty";
    }

    public List<User> GetUserList() {return userList;}

}
