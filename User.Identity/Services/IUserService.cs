﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Identity.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 检查手机号是否注册 如果没有就注册
        /// </summary>
        /// <param name="phone"></param>
        Task<int> CheckOrCreate(string phone);
    }
}
