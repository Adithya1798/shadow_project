using Login_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Login_Application.Controllers
{
    public class DropdownController : Controller
    {
        // GET: Dropdown
        public ActionResult Index()
        {
            Login_ApplicationEntities sd = new Login_ApplicationEntities();
            ViewBag.OrganizationList = new SelectList(GetOrganizationList(), "Oid", "Oname");
            return View();
        }
        public List<organization> GetOrganizationList()
        {
            Login_ApplicationEntities sd = new Login_ApplicationEntities();
            List<organization> organizations = sd.organizations.ToList();
            return organizations;
        }

        public ActionResult GetLocationList(int Oid)
        {
            Login_ApplicationEntities sd = new Login_ApplicationEntities();
            List<Location> selectList = sd.Locations.Where(x => x.Oid == Oid).ToList();
            ViewBag.Llist = new SelectList(selectList, "Lid", "Lname");
            return PartialView("DisplayLocation");
        }
        public ActionResult GetSiteList(int Lid)
        {
            Login_ApplicationEntities sd = new Login_ApplicationEntities();
            List<Site> selectList = sd.Sites.Where(x => x.Lid == Lid).ToList();
            ViewBag.Slist = new SelectList(selectList, "Sid", "Sname");
            return PartialView("DisplaySite");
        }
        public ActionResult GetDeviceList(int Sid)
        {
            Login_ApplicationEntities sd = new Login_ApplicationEntities();
            List<Device> selectList = sd.Devices.Where(x => x.Sid == Sid).ToList();
            ViewBag.Dlist = new SelectList(selectList, "Did", "Dname");
            return PartialView("DisplayDevice");
        }
        public ActionResult Devices(int id)
        {
           Login_ApplicationEntities db = new Login_ApplicationEntities();

            DropdownClass empss = new DropdownClass();
            empss.Devices = db.Devices.ToList();
            var value = empss.Devices.Single(d => d.Did == id);
            return View(value);
        }
    }
}