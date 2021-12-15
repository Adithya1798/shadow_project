using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Login_Application.Controllers
{
        public class DeviceController : Controller
        {
            // GET: Device
            [HttpGet]
            public ActionResult ViewDevicesById(string slno)
            {
                List<Device> lstDevices = new List<Device>();
                DeviceDal dalObj = new DeviceDal();
                var result = dalObj.FetchDevicesById(@slno);
                foreach (var item in result)
                {
                    lstDevices.Add(new Device()
                    {
                        SlNo = item.SlNo,
                        DeviceType = item.DeviceType,
                        DeviceName = item.DeviceName,
                        DeviceDetail = item.DeviceDetail
                    });
                }
                return View(lstDevices);
            }
    public ActionResult Close()
        {
            Session.Clear();
            return RedirectToAction("Index", "Dropdown");
        }
    }
}