﻿using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Frontend.Controllers
{
    public class LeakMemoryController : Controller
    {
        static List<byte[]> memoryLeak = new List<byte[]>();
        public IActionResult Index()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    byte[] byffer = new byte[1024 * 1024 * 25]; //alocate 25M
                    memoryLeak.Add(byffer);
                    Thread.Sleep(1000);
                }
            });
            return View();
        }
    }
}
