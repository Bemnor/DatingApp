using API.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    public DateOnly DateOfBirth { get; set; }

    public string KnownAs { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    public string Gender { get; set; }

    public string Introduction { get; set; }

    public string LookingFor { get; set; }

    public string Interests { get; set; }

    public string City { get; set; }

    public string Country { get; set; }

    public List<Photo> Photos { get; set; } = new List<Photo>();

    // public int GetAge()
    // {
    //     return DateOfBirth.CalculateAge();
    // }
}
