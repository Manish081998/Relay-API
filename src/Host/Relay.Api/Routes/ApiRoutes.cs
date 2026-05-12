namespace Relay.Api.Routes;

public static class ApiRoutes
{
    public static class Documents
    {
        public const string GetById     = "api/documentum/documents/{id:guid}";
        public const string Search      = "api/documentum/documents/search";
        public const string UpdateById  = "api/documentum/documents/{id:guid}";
    }

    public static class Annotations
    {
        public const string GetById = "api/documentum/annotations/{id:int}";
    }

    public static class Users
    {
        public const string GetById       = "api/intranet/users/{id:guid}";
        public const string UpdateByEmail = "api/intranet/users/by-email";
    }

    public static class Selections
    {
        public const string GetById = "api/WebTool/selections/{id:guid}";
    }
    public static class Orders
    {
        public const string Search = "api/documentum/orders/search";
    }

    public static class DocumentumUsers
    {
        public const string GetAll = "api/documentum/users";
        public const string Add    = "api/documentum/users";
        public const string Update = "api/documentum/users";
    }

    public static class DocumentumBrands
    {
        public const string GetAll = "api/documentum/GetAllBrands";
    }

    public static class Authenticatication
    {
        public const string GenerateToken = "api/GenerateToken";
        public const string RefreshToken = "api/RefreshToken";

    }
}
