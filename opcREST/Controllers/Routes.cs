namespace opcRESTconnector {
    public class BaseRoutes{
        public const string admin = "/admin";
        public const string login = "/login";
        public const string logout = "/logout";
        public const string write_access = "/write_access";
        public const string update_pw = "/update_pw";
        public const string api = "/api";
        public const string internal_css = "/_css";
    }

    public class Routes{
        public const string admin = BaseRoutes.admin;
        public const string login = BaseRoutes.admin + BaseRoutes.login;
        public const string logout = BaseRoutes.admin + BaseRoutes.logout;
        public const string write_access = BaseRoutes.admin + BaseRoutes.write_access;
        public const string rest = BaseRoutes.api + "/REST";
        public const string json = BaseRoutes.api + "/JSON";
        public const string update_pw = BaseRoutes.admin + BaseRoutes.update_pw;

    }
}