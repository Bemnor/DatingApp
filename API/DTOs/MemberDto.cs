namespace API.DTOs;

/*this comment was made for code the course later deleted...
//using MemberDto we essentially convert the users from userRepository to a shallower user object
//the mapper cleverly assigns the properties of AppUser to MemberDto(which has less and slightly differently defined properties)
*/

public class MemberDto
{
    public int Id { get; set; }

    public string UserName { get; set; }
    public string PhotoUrl { get; set; }

    public int Age { get; set; }

    public string KnownAs { get; set; }

    public DateTime Created { get; set; }

    public DateTime LastActive { get; set; }

    public string Gender { get; set; }

    public string Introduction { get; set; }

    public string LookingFor { get; set; }

    public string Interests { get; set; }

    public string City { get; set; }

    public string Country { get; set; }

    public List<PhotoDto> Photos { get; set; }
}
