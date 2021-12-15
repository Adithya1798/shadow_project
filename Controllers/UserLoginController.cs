using Login_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Login_Application.Controllers
{
    public class UserLoginController : Controller
    {
        // GET: UserLogin
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(LoginClass lc)
        {
            string conStr = ConfigurationManager.ConnectionStrings["Constr"].ConnectionString;
            SqlConnection sqlcon = new SqlConnection(conStr);
            string sqlquery = "SELECT TOP (1000) [username], [password] FROM[Login].[dbo].[UserLogin]";
            sqlcon.Open();
            SqlCommand sqlcom = new SqlCommand(sqlquery, sqlcon);
            sqlcom.Parameters.AddWithValue("@username", lc.username);
            sqlcom.Parameters.AddWithValue("@password", lc.password);
            SqlDataReader sqdr = sqlcom.ExecuteReader();
            if (sqdr.Read())
            {
                Session["username"] = lc.username.ToString();
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewData["Message"] = "Login Details Failed";
            }
            sqlcon.Close();

            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "UserLogin");
        }
    }
}