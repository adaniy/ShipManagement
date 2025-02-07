﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ship.Infrastructure.Services;
using Ship.Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using Ship.Core.Entities;

namespace Ship.Web.Controllers
{
    public class ExamController : Controller
    {
        readonly ExamService _examService;
        readonly SailorService _sailorService;
        public ExamController(ExamService examService, SailorService sailorService)
        {
            _examService = examService;
            _sailorService = sailorService;
        }

        // GET: /Exam/
        public ActionResult Index(string SailorName, DateTime? BeginDate, DateTime? EndDate, int? Post, int? page)
        {
            var exams = _examService.GetEntities();
            if (BeginDate.HasValue)
            {
                exams = exams.Where(e => e.ExamDate >= BeginDate.Value);
                ViewBag.BeginDate = BeginDate.Value.ToString("yyyy-MM-dd");
            }
            if (EndDate.HasValue)
            {
                exams = exams.Where(e => e.ExamDate <= EndDate.Value);
                ViewBag.EndDate = EndDate.Value.ToString("yyyy-MM-dd");
            }
            if (Post.HasValue)
            {
                exams = exams.Where(e => e.ApplyPost == (Post)Post);
            }
            if (!String.IsNullOrWhiteSpace(SailorName))
            {
                exams = exams.Where(e => e.SailorName.Contains(SailorName));
            }
            exams = exams.OrderByDescending(e => e.ExamID);

            ViewBag.SailorName = SailorName;

            var posts = from Post p in Enum.GetValues(typeof(Post))
                        select new { ID = (int)p, Name = p.ToString() };
            ViewBag.Post = new SelectList(posts, "ID", "Name", Post);
            ViewBag.SelectedPost = Post;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(exams.ToPagedList(pageNumber, pageSize));
        }

        // GET: /Exam/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Exam exam = _examService.Find(id);
            if (exam == null)
            {
                return NotFound();
            }
            return View(exam);
        }

        // GET: /Exam/Create
        public ActionResult Create()
        {
            var SailorID = Request.Query["SailorID"];
            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", SailorID);
            ViewBag.medium = Request.Query["medium"];
            return View();
        }

        // POST: /Exam/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("ExamID,ApplyPost,ExamNo,ExamDate,Expense,ExpenseClaim,CertificateNo,IssueDate,Qualified,Remark,SailorID")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                var sailor = _sailorService.Find(exam.SailorID);
                exam.SailorName = sailor.Name;
                _examService.Add(exam);
                if ("Sailor".Equals(Request.Form["medium"]))
                {
                    return RedirectToAction("Details", "Sailor", new { id = exam.SailorID, tab = "tab_exam" });
                }
                return RedirectToAction("Index");
            }

            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", exam.SailorID);
            return View(exam);
        }

        // GET: /Exam/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Exam exam = _examService.Find(id);
            if (exam == null)
            {
                return NotFound();
            }
            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", exam.SailorID);
            return View(exam);
        }

        // POST: /Exam/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("ExamID,ApplyPost,ExamNo,ExamDate,Expense,ExpenseClaim,CertificateNo,IssueDate,Qualified,Remark,SailorID")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                var sailor = _sailorService.Find(exam.SailorID);
                exam.SailorName = sailor.Name;
                _examService.Update(exam);
                return RedirectToAction("Index");
            }
            ViewBag.SailorID = new SelectList(_sailorService.GetEntities(), "SailorID", "Name", exam.SailorID);
            return View(exam);
        }

        // GET: /Exam/Delete/5
        public ActionResult Delete(int? id)
        {
            return PartialView();
        }

        // POST: /Exam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _examService.Delete(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddItem(ExamItem item)
        {
            if (ModelState.IsValid)
            {
                _examService.AddItem(item);
                return Json("success");
            }
            return Json("error");
        }
    }
}