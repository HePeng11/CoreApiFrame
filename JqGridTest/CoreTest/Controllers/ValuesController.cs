using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTest.Controllers
{
    /// <summary>
    /// API示例
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,hepeng")]//or [Authorize(Policy="RequireAdmin")]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// 返回一个字符串数组
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// 返回一个字符串
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        //[HttpPost("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// put
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="value">VALUE</param>
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
