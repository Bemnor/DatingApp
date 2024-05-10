namespace API.Entities;

//think of this class as a schema for a userObj we store in DB
//whenever we want to update this schema we need to run the "dotnet ef migrations add" cmd.
//then we commit that migration by running "dotnet ef database update"
public class AppUser
{
    public int Id { get; set; }

    public string UserName { get; set; }

    public byte[] PasswordHash { get; set; }

    public byte[] PasswordSalt { get; set; }
}
