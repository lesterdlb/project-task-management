namespace ProjectManagement.Api.Constants;

public static class EndpointNames
{
    private const string Prefix = "api";

    public static class Projects
    {
        public const string GroupName = nameof(Features.Projects);

        public static class Names
        {
            public const string GetProjects = nameof(GetProjects);
            public const string GetProject = nameof(GetProject);
            public const string CreateProject = nameof(CreateProject);
            public const string UpdateProject = nameof(UpdateProject);
            public const string DeleteProject = nameof(DeleteProject);
            public const string AddProjectMember = nameof(AddProjectMember);
            public const string RemoveProjectMember = nameof(RemoveProjectMember);
        }

        public static class Routes
        {
            public const string Base = $"{Prefix}/projects";
            public const string ById = $"{Base}/{{id:guid}}";
        }
    }

    public static class Users
    {
        public const string GroupName = nameof(Features.Users);

        public static class Names
        {
            public const string GetUsers = nameof(GetUsers);
            public const string GetUser = nameof(GetUser);
            public const string CreateUser = nameof(CreateUser);
            public const string UpdateUser = nameof(UpdateUser);
            public const string DeleteUser = nameof(DeleteUser);
        }

        public static class Routes
        {
            public const string Base = $"{Prefix}/users";
            public const string ById = $"{Base}/{{id:guid}}";
        }
    }

    public static class Auth
    {
        public const string GroupName = nameof(Features.Auth);

        public static class Names
        {
            public const string Login = nameof(Login);
            public const string Register = nameof(Register);
            public const string GetCurrentUser = nameof(GetCurrentUser);
            public const string ConfirmEmail = nameof(ConfirmEmail);
            public const string ResendConfirmation = nameof(ResendConfirmation);
            public const string UpdateProfile = nameof(UpdateProfile);
        }

        public static class Routes
        {
            private const string Base = $"{Prefix}/auth";
            public const string Register = $"{Base}/register";
            public const string Login = $"{Base}/login";
            public const string GetCurrentUser = $"{Base}/me";
            public const string ConfirmEmail = $"{Base}/confirm-email";
            public const string ResendConfirmation = $"{Base}/resend-confirmation";
            public const string UpdateProfile = $"{Base}/profile";
        }
    }
}
