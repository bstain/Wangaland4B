using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Moq;
using Microsoft.Owin.Security;
using Wangaland4B.Auth.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using Wangaland4B.Auth.Controllers;
using System.Web.Mvc;
using Microsoft.Owin;
using System.Linq;
using Microsoft.AspNet.Identity.Owin;
using Wangaland4B.Auth;

namespace Wangaland4B.Tests.Auth.Controllers
{
    [TestClass]
    public class AccountControllerTests
    {
        [TestMethod]
        public void TestSuccessfulLogin()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser, int>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authenticationManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);
            var loginModel = new LoginViewModel
            {
                Email = "a",
                Password = "b",
                RememberMe = false
            };
            var returnUrl = "/foo";
            var user = new ApplicationUser
            {
                Email = loginModel.Email,
                UserName = loginModel.Email
            };
            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

            userManager.Setup(um => um.FindAsync(loginModel.Email, loginModel.Password)).Returns(Task.FromResult(user));
            userManager.Setup(um => um.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie)).Returns(Task.FromResult(identity));

            signInManager.Setup(sm => sm.PasswordSignInAsync(loginModel.Email, loginModel.Password, loginModel.RememberMe, false)).Returns(Task<SignInStatus>.FromResult(SignInStatus.Success));

            var controller = new AccountController(userManager.Object, signInManager.Object);
            var helper = new MvcMockHelper(controller);

            // Act
            var actionResult = controller.Login(loginModel, returnUrl).Result;

            // Assert
            var redirectResult = actionResult as RedirectResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(returnUrl, redirectResult.Url);

            Assert.AreEqual(loginModel.Email, helper.OwinContext.Authentication.AuthenticationResponseGrant.Identity.Name);
            Assert.AreEqual(DefaultAuthenticationTypes.ExternalCookie, helper.OwinContext.Authentication.AuthenticationResponseRevoke.AuthenticationTypes.First());
        }

        [TestMethod]
        public void TestUnsuccessfulLogin()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser, int>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authenticationManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);
            var loginModel = new LoginViewModel
            {
                Email = "a",
                Password = "b",
                RememberMe = false
            };
            var returnUrl = "/foo";
            var user = new ApplicationUser
            {
                Email = loginModel.Email,
                UserName = loginModel.Email
            };
            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

            userManager.Setup(um => um.FindAsync(loginModel.Email, loginModel.Password)).Returns(Task.FromResult<ApplicationUser>(null));

            signInManager.Setup(sm => sm.PasswordSignInAsync(loginModel.Email, loginModel.Password, loginModel.RememberMe, false)).Returns(Task<SignInStatus>.FromResult(SignInStatus.Failure));

            var controller = new AccountController(userManager.Object, signInManager.Object);
            var helper = new MvcMockHelper(controller);

            // Act
            var actionResult = controller.Login(loginModel, returnUrl).Result;

            // Assert
            Assert.IsTrue(actionResult is ViewResult);
            var errors = controller.ModelState.Values.First().Errors;
            Assert.AreEqual(1, errors.Count());
        }

        [TestMethod]
        public void TestSuccessfulRegister()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser, int>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authenticationManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);
            var registerModel = new RegisterViewModel
            {
                Email = "a",
                Password = "b",
                ConfirmPassword = "b"
            };
            var user = new ApplicationUser { UserName = registerModel.Email, Email = registerModel.Email };
            var result = IdentityResult.Success;
            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            identity.AddClaim(new Claim(ClaimTypes.Name, registerModel.Email));

            userManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerModel.Password)).Returns(Task.FromResult(result));
            userManager.Setup(um => um.CreateIdentityAsync(It.IsAny<ApplicationUser>(), DefaultAuthenticationTypes.ApplicationCookie)).Returns(Task.FromResult(identity));

            signInManager.Setup(sm => sm.SignInAsync(user, false, false)).Returns(Task.FromResult(0));

            var controller = new AccountController(userManager.Object, signInManager.Object);
            var helper = new MvcMockHelper(controller);

            // Act
            var actionResult = controller.Register(registerModel).Result;

            // Assert
            var redirectResult = actionResult as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);

            Assert.AreEqual(registerModel.Email, helper.OwinContext.Authentication.AuthenticationResponseGrant.Identity.Name);
            Assert.AreEqual(DefaultAuthenticationTypes.ExternalCookie, helper.OwinContext.Authentication.AuthenticationResponseRevoke.AuthenticationTypes.First());
        }
    }
}
