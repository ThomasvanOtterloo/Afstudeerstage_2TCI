using EonWatchesAPI.Enums;

namespace EonWatchesAPI.DbContext;

public class Trader
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public Roles Role { get; set; }
    
}