using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreTest.Controllers
{
    /// <summary>
    /// user
    /// </summary>
    [Route("user")]
    [ApiController]
    [EnableCors("Any")]
    public class UserController : Controller
    {
        /// <summary>
        /// 返回一个字符串数组
        /// </summary>
        /// <returns></returns>
        [HttpGet("login")]
        public ActionResult<string> Login(string loginName, string password)
        {
            return JwtHelper.IssueJWT(new TokenModel() { Uid = 123, Project = "", Role = "hepeng", TokenType = "" });
        }
    }
}