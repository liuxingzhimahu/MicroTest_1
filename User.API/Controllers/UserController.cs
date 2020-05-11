﻿using System;
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

namespace User.API.Controllers
{
    [ApiController]
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

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userContext.Users
                .AsNoTracking()
                .Include(u=>u.UserProperties)
                .SingleOrDefaultAsync(u => u.Id == UserIderntity.UserId);
            if (user == null)
            {
                throw new UserOperationException($"错误的用户上下文Id{ UserIderntity.UserId}");
            }
            return Json(user);
        }

        [HttpPatch("")]
        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<APPUser> patch)
        {
            var user =await _userContext.Users
                .SingleOrDefaultAsync(u => u.Id == UserIderntity.UserId);
            patch.ApplyTo(user);

            _userContext.SaveChanges();
            return Json(user);
        }
    }
}