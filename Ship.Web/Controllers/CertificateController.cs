﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ship.Core.Entities;
using Ship.Core.Enums;
using Ship.Infrastructure.Services;
using X.PagedList;

namespace Ship.Web.Controllers
{
    public class CertificateController : Controller
    {
        readonly CertificateService _certificateService;
        readonly SailorService _sailorService;
        readonly CertificateTypeService _certificateTypeService;
        readonly UploadFileService _uploadFileService;
        readonly NoticeService _noticeService;
        public CertificateController(CertificateService certificateService, SailorService sailorService, CertificateTypeService certificateTypeService, UploadFileService uploadFileService, NoticeService noticeService)
        {
            _certificateService = certificateService;
            _sailorService = sailorService;
            _certificateTypeService = certificateTypeService;
            _uploadFileService = uploadFileService;
            _noticeService = noticeService;
        }

        // GET: /Certificate/
        public ActionResult Index(string SailorName, string Name, string Code, string status,
            DateTime? ExpiryBeginDate, DateTime? ExpiryEndDate, DateTime? IssueBeginDate,
            DateTime? IssueEndDate, int? page)
        {
            var certificates = _certificateService.GetEntities();
            if (ExpiryBeginDate.HasValue)
            {
                certificates = certificates.Where(c => c.ExpiryDate >= ExpiryBeginDate.Value);
                ViewBag.ExpiryBeginDate = ExpiryBeginDate.Value.ToString("yyyy-MM-dd");
            }
            if (ExpiryEndDate.HasValue)
            {
                certificates = certificates.Where(c => c.ExpiryDate <= ExpiryEndDate.Value);
                ViewBag.ExpiryEndDate = ExpiryEndDate.Value.ToString("yyyy-MM-dd");
            }
            if (IssueBeginDate.HasValue)
            {
                certificates = certificates.Where(c => c.IssueDate >= IssueBeginDate.Value);
                ViewBag.IssueBeginDate = IssueBeginDate.Value.ToString("yyyy-MM-dd");
            }
            if (IssueEndDate.HasValue)
            {
                certificates = certificates.Where(c => c.IssueDate <= IssueEndDate.Value);
                ViewBag.IssueEndDate = IssueEndDate.Value.ToString("yyyy-MM-dd");
            }
            if (!String.IsNullOrWhiteSpace(SailorName))
            {
                certificates = certificates.Where(c => c.SailorName.Contains(SailorName));
            }
            if (!String.IsNullOrWhiteSpace(Name))
            {
                certificates = certificates.Where(c => c.Name.Contains(Name));
            }
            if (!String.IsNullOrWhiteSpace(Code))
            {
                certificates = certificates.Where(c => c.Code.Contains(Code));
            }
            switch (status)
            {
                case "normal":
                    certificates = certificates.Where(c => c.ExpiryDate > DateTime.Now);
                    break;
                case "notice":
                    certificates = certificates.Where(c => c.ExpiryDate > DateTime.Now && c.NoticeDate < DateTime.Now);
                    break;
                case "overdue":
                    certificates = certificates.Where(c => c.ExpiryDate < DateTime.Now);
                    break;
                default: break;
            }

            certificates = certificates.OrderByDescending(i => i.CertificateID);

            ViewBag.SailorName = SailorName;
            ViewBag.Name = Name;
            ViewBag.Code = Code;
            ViewBag.status = status;

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(certificates.ToPagedList(pageNumber, pageSize));
        }

        // GET: /Certificate/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Certificate certificate = _certificateService.Find(id);
            if (certificate == null)
            {
                return NotFound();
            }
            return View(certificate);
        }

        // GET: /Certificate/Create
        public ActionResult Create()
        {
            var SailorID = Request.Query["SailorID"];
            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", SailorID);
            ViewBag.medium = Request.Query["medium"];
            ViewBag.CertificateTypeID = new SelectList(_certificateTypeService.GetSailorCertificates(), "CertificateTypeID", "Name", null);
            return View();
        }

        // POST: /Certificate/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Certificate certificate, IFormFile certificateFile)
        {
            if (ModelState.IsValid)
            {
                var sailor = _sailorService.Find(certificate.SailorID);
                certificate.SailorName = sailor.Name;
                var certificateType = _certificateTypeService.GetSailorCertificates().FirstOrDefault(c => c.Name == certificate.Name);
                if (certificateType == null)
                {
                    certificateType = new CertificateType()
                    {
                        Name = certificate.Name,
                        CertificateCategory = CertificateCategory.船员证书
                    };
                    certificateType = _certificateTypeService.Add(certificateType);
                }
                certificate.CertificateTypeID = certificateType.CertificateTypeID;
                certificate.Name = certificateType.Name;

                certificate.FileID = _uploadFileService.AddFile(certificateFile);

                certificate = _certificateService.Add(certificate);
                _noticeService.AddSailorCertificateNotice(certificate);

                if ("Sailor".Equals(Request.Form["medium"]))
                {
                    return RedirectToAction("Details", "Sailor", new { id = certificate.SailorID, tab = "tab_certificate" });
                }

                return RedirectToAction("Index");
            }

            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", certificate.SailorID);
            ViewBag.CertificateTypeID = new SelectList(_certificateTypeService.GetSailorCertificates(), "CertificateTypeID", "Name", certificate.CertificateTypeID);
            return View(certificate);
        }

        // GET: /Certificate/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Certificate certificate = _certificateService.Find(id);
            if (certificate == null)
            {
                return NotFound();
            }

            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", certificate.SailorID);
            ViewBag.CertificateTypeID = new SelectList(_certificateTypeService.GetSailorCertificates(), "CertificateTypeID", "Name", certificate.CertificateTypeID);
            return View(certificate);
        }

        // POST: /Certificate/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Certificate certificate, IFormFile certificateFile)
        {
            if (ModelState.IsValid)
            {
                var sailor = _sailorService.Find(certificate.SailorID);
                certificate.SailorName = sailor.Name;
                certificate.Name = certificate.Name.Trim();
                var certificateType = _certificateTypeService.GetSailorCertificates().FirstOrDefault(c => c.Name == certificate.Name);
                if (certificateType == null)
                {
                    certificateType = new CertificateType()
                    {
                        Name = certificate.Name,
                        CertificateCategory = CertificateCategory.船员证书
                    };
                    certificateType = _certificateTypeService.Add(certificateType);
                }
                certificate.CertificateTypeID = certificateType.CertificateTypeID;
                certificate.Name = certificateType.Name;

                certificate.FileID = _uploadFileService.AddFile(certificateFile, certificate.FileID);

                _certificateService.Update(certificate, false);
                _noticeService.DeleteRange(n => n.Source == NoticeSource.Certificate && n.SourceID == certificate.CertificateID);
                _noticeService.AddSailorCertificateNotice(certificate);
                return RedirectToAction("Index");
            }
            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", certificate.SailorID);
            ViewBag.CertificateTypeID = new SelectList(_certificateTypeService.GetSailorCertificates(), "CertificateTypeID", "Name", certificate.CertificateTypeID);
            return View(certificate);
        }

        // GET: /Certificate/Delete/5
        public ActionResult Delete(int? id)
        {
            return PartialView();
        }

        // POST: /Certificate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var certificate = _certificateService.Find(id);
            _uploadFileService.Delete(certificate.FileID, false);
            _certificateService.Delete(id, false);
            _noticeService.DeleteRange(n => n.Source == NoticeSource.Certificate && n.SourceID == id);
            return RedirectToAction("Index");
        }
    }
}