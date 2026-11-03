namespace Praktikum542.Common
{
    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string Manager = "manager";
        public const string Client = "client";
    }

    public static class UserStatuses
    {
        public const string Active = "active";
        public const string Deleted = "deleted";
    }

    public static class BookingStatuses
    {
        public const string Pending = "pending";
        public const string Confirmed = "confirmed";
        public const string Cancelled = "cancelled";
    }
}