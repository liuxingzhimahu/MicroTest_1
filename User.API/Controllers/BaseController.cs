using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Dtos;

namespace User.API.Controllers
{
    public class BaseController: Controller
    {
        public UserIderntity UserIderntity 
            => new UserIderntity { UserId =1 ,Name = "jesse"};
    }
}
