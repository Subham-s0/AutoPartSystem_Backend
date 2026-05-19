namespace VehiStock.Domain.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Customer = "Customer";

    public static readonly string[] All = [Admin, Staff, Customer];
    public static readonly string[] Registrable = [Staff, Customer];
}
