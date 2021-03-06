﻿using Blog.Common.Net;
using Blog.Entities;
using Blog.Entities.Dtos;
using Blog.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Blog.Web.Controllers
{
    public class HomeController : BaseWebController
    {
        private readonly IBannerInfoService _bannerInfoService;
        private readonly IQQUserinfoService _qQUserinfoService;
        private readonly ILeavemsgService _leavemsgService;
        public HomeController(IBannerInfoService bannerInfoService, IQQUserinfoService qQUserinfoService, ILeavemsgService leavemsgService)
        {
            _bannerInfoService = bannerInfoService;
            _qQUserinfoService = qQUserinfoService;
            _leavemsgService = leavemsgService;
        }

        #region 视图
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(string code, string state)
        {
            List<BannerInfo> list = _bannerInfoService.QueryableCache(null, o => o.SortCode, false);
            return View(list);
        }

        /// <summary>
        /// 作品展示
        /// </summary>
        /// <returns></returns>
        public IActionResult Works()
        {
            return View();
        }

        /// <summary>
        /// 时光轴
        /// </summary>
        /// <returns></returns>
        public IActionResult TimeLine()
        {
            return View();
        }

        /// <summary>
        /// 关于
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            return View();
        }
        #endregion

        /// <summary>
        /// 留言列表
        /// </summary>
        /// <returns></returns>
        public IActionResult Msg(int page, int limit = 10, int climit = 3, string aid = null)
        {
            return Json(_leavemsgService.MsgList(page, limit, climit, aid), "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 回复分页
        /// </summary>
        /// <param name="pageindex">当前页</param>
        /// <param name="rootid">评论ID</param>
        /// <param name="aid">文章ID</param>
        /// <param name="limit">没有显示数量</param>
        /// <returns></returns>
        public IActionResult ReplyPage(int pageindex, string rootid, string aid = null, int limit = 3)
        {
            return Json(_leavemsgService.ReplyList(pageindex, limit, rootid, aid), "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 回复
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Reply(string rootid, string pid, string fromid, string content, string aid)
        {
            OperateResult result = new OperateResult();
            QQUserinfo user = HttpContextHelper.GetSession<QQUserinfo>("QQ_User");
            if (user == null)
            {
                result.Status = ResultStatus.SignOut;
                result.Message = "未登录";
            }
            else
            {
                result = _leavemsgService.Reply(rootid, pid, fromid, content, user.UserId, aid);
            }
            return Json(result);
        }

        /// <summary>
        /// 留言
        /// </summary>
        /// <param name="content">留言内容</param>
        /// <param name="aid">文章id</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Comment(string content, string aid)
        {
            OperateResult result = new OperateResult();
            QQUserinfo user = HttpContextHelper.GetSession<QQUserinfo>("QQ_User");
            if (user == null)
            {
                result.Status = ResultStatus.SignOut;
                result.Message = "未登录";
            }
            else
            {
                result = _leavemsgService.Comment(content, user.UserId, aid);
            }
            return Json(result);
        }

        /// <summary>
        /// QQ授权登录
        /// </summary>
        /// <returns></returns>
        public IActionResult Login(string code, string state)
        {
            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(state))
            {
                string referer = HttpContext.Request.Headers[HeaderNames.Referer].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(referer))
                {
                    referer = "/home/index";
                }
                return Json(_qQUserinfoService.Authorize(referer));
            }
            else
            {
                var user = _qQUserinfoService.Login(code, state);
                string url = HttpContext.Session.GetString("lib" + state);
                if (string.IsNullOrWhiteSpace(url))
                {
                    url = "/home/index";
                }
                if (user != null)
                {
                    string json = JsonConvert.SerializeObject(user);
                    HttpContext.Session.SetString("QQ_User", json);
                }
                HttpContext.Session.Remove("lib" + state);
                return Redirect(url);
            }

        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public IActionResult SignOut()
        {
            HttpContext.Session.Remove("QQ_User");
            return Json(new OperateResult("已安全退出", ResultStatus.Success));
        }
    }
}