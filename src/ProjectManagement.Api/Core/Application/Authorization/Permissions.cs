namespace ProjectManagement.Api.Core.Application.Authorization;

public static class Permissions
{
    public const string ClaimType = "permissions";

    public static class Users
    {
        public const string Read = "users:read";
        public const string Write = "users:write";
        public const string Delete = "users:delete";
    }

    public static class Projects
    {
        public const string Read = "projects:read";
        public const string Write = "projects:write";
        public const string Delete = "projects:delete";
    }

    public static class Tasks
    {
        public const string Read = "tasks:read";
        public const string Write = "tasks:write";
        public const string Delete = "tasks:delete";
    }

    public static class Labels
    {
        public const string Read = "labels:read";
        public const string Write = "labels:write";
        public const string Delete = "labels:delete";
    }

    public static class Comments
    {
        public const string Read = "comments:read";
        public const string Write = "comments:write";
        public const string Delete = "comments:delete";
    }
}
