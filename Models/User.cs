namespace dotnet.Models;

public class UserModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
}

public class ContactInfo
{
    public string? MobileNumber { get; set; }
    public string? Office { get; set; }
    public string? Residence { get; set; }
}