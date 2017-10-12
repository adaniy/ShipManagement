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
using PagedList;
using SailorWeb.Services;

namespace SailorWeb.Controllers
{
    [Authorize]
    public class InterviewController : Controller
    {
        readonly ISailorService _sailorService;
        readonly IInterviewService _interviewService;
        public InterviewController(ISailorService sailorService, IInterviewService interviewService)
        {
            _sailorService = sailorService;
            _interviewService = interviewService;
        }

        // GET: /Interview/
        public ActionResult Index(string SailorName, DateTime? BeginDate, DateTime? EndDate,int? Post,int? page)
        {
            var interviews = _interviewService.GetEntities();
            if (BeginDate.HasValue)
            {
                interviews = interviews.Where(i => i.InterviewDate >= BeginDate.Value);
                ViewBag.BeginDate = BeginDate.Value.ToString("yyyy-MM-dd");
            }
            if (EndDate.HasValue)
            {
                interviews = interviews.Where(i => i.InterviewDate <= EndDate.Value);
                ViewBag.EndDate = EndDate.Value.ToString("yyyy-MM-dd");
            }
            if (Post.HasValue)
            {
                interviews = interviews.Where(i => i.Post == (Post)Post);
            }

            if (!String.IsNullOrWhiteSpace(SailorName))
            {
                interviews = interviews.Where(i => i.SailorName.Contains(SailorName));
            }
            interviews = interviews.OrderByDescending(i => i.InterviewID);
            ViewBag.SailorName = SailorName;
            var posts = from Post p in Enum.GetValues(typeof(Post))
                        select new { ID = (int)p, Name = p.ToString() };
            ViewBag.Post = new SelectList(posts, "ID", "Name", Post);
            ViewBag.SelectedPost = Post;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(interviews.ToPagedList(pageNumber, pageSize));
        }

        // GET: /Interview/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Interview interview = _interviewService.Find(id);
            if (interview == null)
            {
                return HttpNotFound();
            }
            return View(interview);
        }

        // GET: /Interview/Create
        public ActionResult Create()
        {
            var SailorID = Request.QueryString["SailorID"];
            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", SailorID);
            ViewBag.medium = Request.QueryString["medium"];
            return View();
        }

        // POST: /Interview/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="InterviewID,Post,EnglishLevel,Listening,Speaking,Reading,Writing,Expertise,Qualification,EmergencyHandle,ServiceAwareness,Health,Management,SmsOperation,Other,InterviewScore,Conclusion,Comment,Interviewer,InterviewDate,InterviewPlace,ProfessionalScore,ComprehensiveScore,ComprehensiveAssessment,SailorRequirement,SailorID")] Interview interview)
        {
            if (ModelState.IsValid)
            {
                var sailor = _sailorService.Find(interview.SailorID);
                interview.SailorName = sailor.Name;
                _interviewService.Add(interview);
                if ("Sailor".Equals(Request.Form["medium"]))
                {
                    return RedirectToAction("Details", "Sailor", new { id = interview.SailorID, tab = "tab_interview" });
                }
                return RedirectToAction("Index");
            }

            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", interview.SailorID);
            return View(interview);
        }

        // GET: /Interview/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Interview interview = _interviewService.Find(id);
            if (interview == null)
            {
                return HttpNotFound();
            }
            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", interview.SailorID);
            return View(interview);
        }

        // POST: /Interview/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="InterviewID,Post,EnglishLevel,Listening,Speaking,Reading,Writing,Expertise,Qualification,EmergencyHandle,ServiceAwareness,Health,Management,SmsOperation,Other,InterviewScore,Conclusion,Comment,Interviewer,InterviewDate,InterviewPlace,ProfessionalScore,ComprehensiveScore,ComprehensiveAssessment,SailorRequirement,SailorID")] Interview interview)
        {
            if (ModelState.IsValid)
            {
                var sailor = _sailorService.Find(interview.SailorID);
                interview.SailorName = sailor.Name;
                _interviewService.Update(interview);
                return RedirectToAction("Index");
            }
            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", interview.SailorID);
            return View(interview);
        }

        // GET: /Interview/Delete/5
        public ActionResult Delete(int? id)
        {
            return PartialView();
        }

        // POST: /Interview/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _interviewService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
