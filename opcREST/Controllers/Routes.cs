namespace opcRESTconnector {
    public class BaseRoutes{
        public const string auth = "/auth";
        public const string admin = "/admin";
        public const string login = "/login";
        public const string logout = "/logout";
        public const string write_access = "/write_access";
        public const string update_pw = "/update_pw";
        public const string users = "/users";
        public const string reset_pw = "/reset_pw";
        public const string create_user = users + "/create";
        public const string read_user = users + "/read";
        public const string update = "/update";
        public const string delete = "/delete";
        public const string read_session = users + "/sessions";
        public const string api = "/api";
        public const string internal_css = "/_css";
        public const string internal_js = "/_js";
    }

    public class Routes{
        public const string admin = BaseRoutes.admin;
        public const string login = BaseRoutes.auth + BaseRoutes.login;
        public const string logout = BaseRoutes.auth + BaseRoutes.logout;
        public const string write_access = BaseRoutes.admin + BaseRoutes.write_access;
        public const string rest = BaseRoutes.api + "/REST";
        public const string json = BaseRoutes.api + "/JSON";
        public const string update_pw = BaseRoutes.auth + BaseRoutes.update_pw;
        public const string create_user = BaseRoutes.admin + BaseRoutes.create_user;
        public const string read_session = BaseRoutes.admin + BaseRoutes.read_session;

    }
}