using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using User.API.Models;
using User.API.Data;
using User.API.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace User.API.Tests
{
    public class UserControllerTests
    {
        private Data.UserContext GetUserContext()
        {
            var options = new DbContextOptionsBuilder<Data.UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            var userContext = new Data.UserContext(options);
            userContext.Users.Add( new Models.APPUser { 
                Id = 1,
                Name = "Jesse"
            });
            userContext.SaveChanges();
            return userContext;
        }

        private (UserController controller , UserContext userContext) GetUserController()
        {
            var userContext = GetUserContext();
            var loggerMoq = new Mock<ILogger<Controllers.UserController>>();
            var logger = loggerMoq.Object;
            return (new UserController(userContext, logger), userContext);
        }

        [Fact]
        public async Task Get_ReturnRightUser_WithExpectedParameters()
        {
            (var controller, _) = GetUserController();

            var response = await controller.Get();
            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<Models.APPUser>().Subject;
            appUser.Id.Should().Be(1);
            appUser.Name.Should().Be("Jesse");
        }

        [Fact]
        public async Task Patch_ReturnNewName_WithExpectedNewNameParameters()
        {
            (var controller, var userContext) = GetUserController();
            var document = new JsonPatchDocument<APPUser>();
            document.Replace(user => user.Name, "lei");

            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser = result.Value.Should().BeAssignableTo<Models.APPUser>().Subject;
            appUser.Name.Should().Be("lei");

            //assert name in ef context
            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Should().NotBeNull();
            userModel.Name.Should().Be("lei");
        }

        [Fact]
        public async Task Patch_ReturnNewPropertiese_WitAddNewProperty()
        {
            (var controller, var userContext) = GetUserController();
            var document = new JsonPatchDocument<APPUser>();
            document.Replace(user => user.UserProperties,  new List<UserProperty> {  
               new UserProperty{  Key  = "fin_industry",Value = "»¥ÁªÍø",Text = "»¥ÁªÍø"}
            });

            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser = result.Value.Should().BeAssignableTo<Models.APPUser>().Subject;
            appUser.UserProperties.Count.Should().Be(1);
            appUser.UserProperties.First().Key.Should().Be("fin_industry");
            appUser.UserProperties.First().Value.Should().Be("»¥ÁªÍø");
            appUser.UserProperties.First().Text.Should().Be("»¥ÁªÍø");

            //assert name in ef context
            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.UserProperties.Count.Should().Be(1);
            userModel.UserProperties.First().Key.Should().Be("fin_industry");
            userModel.UserProperties.First().Value.Should().Be("»¥ÁªÍø");
            userModel.UserProperties.First().Text.Should().Be("»¥ÁªÍø");

        }

        [Fact]
        public async Task Patch_ReturnNewPropertiese_WitRemoveProperty()
        {
            (var controller, var userContext) = GetUserController();
            var document = new JsonPatchDocument<APPUser>();
            document.Replace(user => user.UserProperties, new List<UserProperty> {});

            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser = result.Value.Should().BeAssignableTo<Models.APPUser>().Subject;
            appUser.UserProperties.Should().BeEmpty();

            //assert name in ef context
            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.UserProperties.Should().BeEmpty();
        }


    }
}
