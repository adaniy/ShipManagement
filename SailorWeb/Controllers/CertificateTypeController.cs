﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SailorWeb.Models;
using SailorDomain.Entities;
using SailorWeb.Services;

namespace SailorWeb.Controllers
{
    public class CertificateTypeController : Controller
    {
        readonly ICertificateTypeService _certificateTypeService;
        public CertificateTypeController(ICertificateTypeService certificateTypeService)
        {
            _certificateTypeService = certificateTypeService;
        }

        // GET: /CertificateType/
        public ActionResult Index(string Name,string Description,int? Category)
        {
            var query = _certificateTypeService.GetEntities();
            if (!String.IsNullOrWhiteSpace(Name))
            {
                query = query.Where(x => x.Name.Contains(Name));
            }
            if (!String.IsNullOrWhiteSpace(Description))
            {
                query = query.Where(x => x.Description.Contains(Description));
            }
            if (Category.HasValue)
            {
                query = query.Where(x => x.CertificateCategory == (CertificateCategory)Category);
            }
            var categories = from CertificateCategory p in Enum.GetValues(typeof(CertificateCategory))
                             select new { ID = (int)p, Name = p.ToString() };
            ViewBag.Category = new SelectList(categories, "ID", "Name", Category);
            return View(query.ToList());
        }

        // GET: /CertificateType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CertificateType certificatetype = _certificateTypeService.Find(id);
            if (certificatetype == null)
            {
                return HttpNotFound();
            }
            return View(certificatetype);
        }

        // GET: /CertificateType/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /CertificateType/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CertificateTypeID,Name,Description,CertificateCategory")] CertificateType certificatetype)
        {
            if (ModelState.IsValid)
            {
                _certificateTypeService.Add(certificatetype);
                return RedirectToAction("Index");
            }

            return View(certificatetype);
        }

        // GET: /CertificateType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CertificateType certificatetype = _certificateTypeService.Find(id);
            if (certificatetype == null)
            {
                return HttpNotFound();
            }
            return View(certificatetype);
        }

        // POST: /CertificateType/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CertificateTypeID,Name,Description,CertificateCategory")] CertificateType certificatetype)
        {
            if (ModelState.IsValid)
            {
                _certificateTypeService.Update(certificatetype);
                return RedirectToAction("Index");
            }
            return View(certificatetype);
        }

        // GET: /CertificateType/Delete/5
        public ActionResult Delete(int? id)
        {
            return PartialView();
        }

        // POST: /CertificateType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _certificateTypeService.Delete(id);
            return RedirectToAction("Index");
        }

        public ActionResult GetSailorCertificates(string query)
        {
            var list = new List<CertificateType>();
            if (!String.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();
                list = _certificateTypeService.GetSailorCertificates().Where(c => c.Name.Contains(query)).ToList();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVesselCertificates(string query)
        {
            var list = new List<CertificateType>();
            if (!String.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();
                list = _certificateTypeService.GetVesselCertificates().Where(c => c.Name.Contains(query)).ToList();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}
