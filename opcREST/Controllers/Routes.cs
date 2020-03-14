namespace opcRESTconnector {
    public class BaseRoutes{
        public const string admin = "/admin";
        public const string login = "/login";
        public const string logout = "/logout";
        public const string write_access = "/write_access";
    }

    public class Routes{
        public const string admin = BaseRoutes.admin;
        public const string login = BaseRoutes.admin + BaseRoutes.login;
        public const string logout = BaseRoutes.admin + BaseRoutes.logout;
        public const string write_access = BaseRoutes.admin + BaseRoutes.write_access;
    }
}