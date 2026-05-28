namespace Relay.Api.Routes;

public static class ApiRoutes
{
    public static class Documentum
    {
        public static class Brands
        {
            public const string GetAll = "api/documentum/GetAllBrands";
            public const string BrandAndQueuesAndMapping = "api/documentum/GetBrandAndQueuesAndMapping";
        }
        public static class SearchOrder
        {
            public const string Search = "api/documentum/orders/search";
        }

        public static class Queues
        {
            public const string GetAll               = "api/documentum/queues";
            public const string Add                  = "api/documentum/queues";
            public const string Update               = "api/documentum/queues";
            public const string Delete               = "api/documentum/queues/{id:int}";
            public const string GetBrandQueueMapping = "api/documentum/queues/brand-queue-mapping";
        }

        public static class SalesOrderDocuments
        {
            private const string Base = "api/documentum/sales-order-documents";
            public const string Upload = $"{Base}/upload";
            public const string CreateVersion = $"{Base}/create-version";
            public const string GetByOrderSeq = $"{Base}/{{orderSeq:int}}";
            public const string GetVersions = $"{Base}/{{documentId:int}}/versions";
            public const string Preview = $"{Base}/preview";
        }

        public static class SalesOrderNotes
        {
            private const string Base = "api/documentum/sales-order-notes";
            public const string GetByOrderSeq = $"{Base}/{{orderSeq:int}}";
            public const string Add = Base;
        }

        public static class Orders
        {
            public const string Search = "api/documentum/orders/search";
            public const string Brands = "api/documentum/orders/brands";
            public const string GetByOrderSeq = "api/documentum/orders/{orderSeq:int}";
            public const string ProductTypes = "api/documentum/orders/product-types";
            public const string Regions = "api/documentum/orders/regions";
            public const string QueuesByBrand = "api/documentum/orders/queues";
            public const string RouteToDepartment = "api/documentum/orders/route-to-department";
        }
    }
    public static class Selections
    {
        public const string GetById = "api/WebTool/selections/{id:guid}";
    }

    public static class Users
    {
        //public const string GetAll = "api/users";
        public const string CreateUser = "api/users/CreateUser";
        public const string UpdateUser = "api/users/UpdateUser";
        public const string GetByGlobalId = "api/users/{globalId}";
        //public const string Add = "api/users";
        //public const string Update = "api/users";
    }   
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
