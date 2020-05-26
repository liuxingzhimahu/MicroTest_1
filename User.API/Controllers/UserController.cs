using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using User.API.Data;
using User.API.Filters;
using Microsoft.AspNetCore.JsonPatch;
using User.API.Models;
using User.API.Dtos;

namespace User.API.Controllers
{
    [Produces("application/json")]//删除时check-or-create拿不到phone
    ///[ApiController]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private UserContext _userContext;
        private ILogger<UserController> _logger;
        
        public UserController(UserContext userContext, ILogger<UserController> logger)
        {
            _userContext = userContext;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var user = await _userContext.Users
                .AsNoTracking()
                .Include(u=>u.UserProperties)
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);
            if (user == null)
            {
                throw new UserOperationException($"错误的用户上下文Id{ UserIdentity.UserId}");
            }
            return Json(user);
        }

        [HttpPatch("")]
        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<APPUser> patch)
        {
            var user =await _userContext.Users
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);
            patch.ApplyTo(user);

            _userContext.SaveChanges();
            return Json(user);
        }

        /// <summary>
        /// 当用户不存在创建用户
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost("check-or-create")]
        public async Task<IActionResult> CheckOrCreate(string phone)
        {

            var user = _userContext.Users.SingleOrDefault(u=>u.Phone ==phone);
            if (user == null)
            {
                user = new APPUser { Phone = phone };
                _userContext.Users.Add(user);
                await _userContext.SaveChangesAsync();
            }

            return Ok(new 
            {
                user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar
            });
        }

        /// <summary>
        /// 获取用户标签选项数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("tags")]
        public async Task<IActionResult> GetUserTags()
        {
            return Ok(await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync());
        }

        /// <summary>
        /// 根据手机号码查找用户资料
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost("search")]
        public async Task<IActionResult> Search(string phone)
        {
            return Ok(await _userContext.Users.Include(u => u.UserProperties).SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId));
        }

        /// <summary>
        /// 更新用户标签数据
        /// </summary>
        /// <returns></returns>
        [HttpPut("tags")]
        public async Task<IActionResult> UpdateUserTags([FromBody] List<string> tags)
        {
            var originTags = await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync();
            var newTags = tags.Except(originTags.Select(t => t.Tag));

            await _userContext.UserTags.AddRangeAsync(newTags.Select(t => new UserTag
            {
                CreatedTime = DateTime.Now,
                UserId = UserIdentity.UserId,
                Tag = t
            }));
            await _userContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("baseUserInfo/{userId}")]
        public async Task<IActionResult> BaseUserInfo(int userId)
        {
            ///TBD 检查用户是否好友关系
            var appUser = await _userContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (appUser == null) return NotFound();
            var baseUserInfo = new UserIdentity()
            {
                Avatar = appUser.Avatar,
                Company = appUser.Company,
                Name = appUser.Name,
                Title = appUser.Title,
                UserId = appUser.Id
            };
            return Ok(baseUserInfo);
        }
    }
}