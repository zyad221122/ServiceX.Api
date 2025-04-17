namespace ServiceX.Contracts.UserContract;

public class UserDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }

    public int Balanace { get; set; }
    public string ProfileImageUrl { get; set; } // رابط الصورة
}
