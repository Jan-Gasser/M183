using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Web.Mvc;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using Pruefung_Praktisch_Musterloesung.Models;

namespace Pruefung_Praktisch_Musterloesung.Controllers
{
    public class Lab2Controller : Controller
    {

        /**
        * Attack 1: Session-Cookie setzten
        * Beschreibung: Man setzt im Browser das Cookie auf eine Session-ID eines eingeloggten Users und bekommt so Zugriff auf das Backend.
        * Die Applikation prüft aus die sid. Dann setzt man das Cookie auf diese ID und bekommt so Zugriff.
        * URL: meineseite.com/Lab2/Backend
        *
        * Attack 2: SQL-Injection
        * Beschreibung: Der Angreifer gibt beim Login einen SQL-Befehl anstelle des Passwortes mit, der dann direkt auf der Datenbank ausgeführt wird.
        * So kann er diverse Befehle direkt dort ausführen. Diese schickt er mit dem Login-Formular per Post mit (deshalb nicht in der URL)
        * URL: meineseite.com/Lab2/Login
        * */

        public ActionResult Index() {

            var sessionid = Request.QueryString["sid"];

            if (string.IsNullOrEmpty(sessionid))
            {
                var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
                sessionid = string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
            }

            ViewBag.sessionid = sessionid;

            return View();
        }

        [HttpPost]
        public ActionResult Login()
        {
            var username = Request["username"];
            var password = Request["password"];
            var sessionid = Request.QueryString["sid"];
            SqlParameter[] myparams = new SqlParameter[2];
            myparams[0] = new SqlParameter("@Username", username);
            myparams[1] = new SqlParameter("@Passowrd", password);


            // hints:
            //var used_browser = Request.Browser.Platform;
            //var ip = Request.UserHostAddress;

            Lab2Userlogin model = new Lab2Userlogin();

            if (model.checkCredentials(username, password))
            {
                model.storeSessionInfos(username, password, sessionid);

                HttpCookie c = new HttpCookie("sid");
                c.Expires = DateTime.Now.AddMonths(2);
                c.Value = sessionid;
                Response.Cookies.Add(c);

                return RedirectToAction("Backend", "Lab2");
            }
            else
            {
                ViewBag.message = "Wrong Credentials";
                return View();
            }
        }

        public ActionResult Backend()
        {
            var sessionid = "";

            if (Request.Cookies.AllKeys.Contains("sid"))
            {
                sessionid = Request.Cookies["sid"].Value.ToString();
            }           

            if (!string.IsNullOrEmpty(Request.QueryString["sid"]))
            {
                sessionid = Request.QueryString["sid"];
            }
            
            // hints:
            var used_browser = Request.Browser.Platform;
            var ip = Request.UserHostAddress;

            Lab2Userlogin model = new Lab2Userlogin();

            if (model.checkSessionInfos(sessionid, used_browser, ip))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Lab2");
            }              
        }
    }
}