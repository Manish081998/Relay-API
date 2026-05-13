namespace Relay.Api.Routes;

public static class ApiRoutes
{
    public static class Documentum
    {
        public static class Annotation
        {
            public const string GetById = "api/documentum/annotations/{id:int}";
        }
        public static class Brands
        {
            public const string GetAll = "api/documentum/GetAllBrands";
            public const string BrandAndQueuesAndMapping = "api/documentum/GetBrandAndQueuesAndMapping";
        }
        public static class Documents
        {
            public const string GetById = "api/documentum/documents/{id:guid}";
            public const string Search = "api/documentum/documents/search";
            public const string UpdateById = "api/documentum/documents/{id:guid}";
        }
        public static class SearchOrder
        {
            public const string Search = "api/documentum/orders/search";
        }

        public static class Orders
        {
            public const string Search = "api/documentum/orders/search";
            public const string Brands = "api/documentum/orders/brands";
            public const string ProductTypes = "api/documentum/orders/product-types";
            public const string QueuesByBrand = "api/documentum/orders/queues";
        }
    }
   
    //public static class Documents
    //{
    //    public const string GetById = "api/documentum/documents/{id:guid}";
    //    public const string Search = "api/documentum/documents/search";
    //    public const string UpdateById = "api/documentum/documents/{id:guid}";
    //}

    //public static class Annotations
    //{
    //    public const string GetById = "api/documentum/annotations/{id:int}";
    //}

    //public static class Users
    //{
    //    public const string GetById = "api/intranet/users/{id:guid}";
    //    public const string UpdateByEmail = "api/intranet/users/by-email";
    //}

    public static class Selections
    {
        public const string GetById = "api/WebTool/selections/{id:guid}";
    }

    //public static class Orders

    //{
    //    public const string Search = "api/documentum/orders/search";
    //}

    public static class Users
    {
        public const string GetAll = "api/users";
        public const string CreateUser = "api/users/CreateUser";
        public const string GetByGlobalId = "api/users/{globalId}";
        public const string Add = "api/users";
        public const string Update = "api/users";
    }

    //public static class DocumentumBrands
    //{
    //    public const string GetAll = "api/documentum/GetAllBrands";
    //}
    public static class Intranet
    {
        public const string SearchEdgeOrders = "api/intranet/edge-orders/SearchEdgeOrders";
    }
    public static class Authenticatication
    {
        public const string GenerateToken = "api/GenerateToken";
        public const string RefreshToken = "api/RefreshToken";

    }
}
