using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Wangaland4B.Auth.Models
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // 전자 메일을 보낼 전자 메일 서비스를 여기에 플러그 인으로 추가합니다.
            // return Task.FromResult(0);

            // Credentials:
            var credentialUserName = "ngdz@naver.com";
            var sentFrom = "ngdz@naver.com";
            var pwd = System.Configuration.ConfigurationManager.AppSettings["MailPassword"];

            // Configure the client:
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.naver.com");

            client.Port = 587;
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;

            // Creatte the credentials:
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(credentialUserName, pwd);

            client.EnableSsl = true;
            client.Credentials = credentials;

            // Create the message:
            var mail = new System.Net.Mail.MailMessage(sentFrom, message.Destination);

            mail.Subject = message.Subject;
            mail.Body = message.Body;

            // Send:
            return client.SendMailAsync(mail);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // 텍스트 메시지를 보낼 SMS 서비스를 여기에 플러그 인으로 추가합니다.
            return Task.FromResult(0);
        }
    }

    // 이 응용 프로그램에서 사용되는 응용 프로그램 사용자 관리자를 구성합니다. UserManager는 ASP.NET Identity에서 정의하며 응용 프로그램에서 사용됩니다.
    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new CustomUserStore(context.Get<ApplicationDbContext>()));
            // 사용자 이름에 대한 유효성 검사 논리 구성
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
            {
                // 영문자/숫자만 허용
                AllowOnlyAlphanumericUserNames = false,
                // 이메일 중복 불가 여부
                RequireUniqueEmail = true
            };

            // 암호에 대한 유효성 검사 논리 구성
            manager.PasswordValidator = new PasswordValidator
            {
                // 암호 최소 길이
                RequiredLength = 6,
                // 특수 문자 필요 여부
                RequireNonLetterOrDigit = false,
                // 숫자 필요 여부
                RequireDigit = false,
                // 영문소문자 필요 여부
                RequireLowercase = false,
                // 영문대문자 필요 여부
                RequireUppercase = false,
            };

            // 사용자 잠금 기본값 구성
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // 2단계 인증 공급자를 등록합니다. 이 응용 프로그램은 사용자 확인 코드를 받는 단계에서 전화 및 전자 메일을 사용합니다.
            // 공급자 및 플러그 인을 여기에 쓸 수 있습니다.
            manager.RegisterTwoFactorProvider("전화 코드", new PhoneNumberTokenProvider<ApplicationUser, int>
            {
                MessageFormat = "보안 코드는 {0}입니다."
            });
            manager.RegisterTwoFactorProvider("전자 메일 코드", new EmailTokenProvider<ApplicationUser, int>
            {
                Subject = "보안 코드",
                BodyFormat = "보안 코드는 {0}입니다."
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // 이 응용 프로그램에서 사용되는 응용 프로그램 로그인 관리자를 구성합니다.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, int>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    // TODO: 추가된 거 검토
    // 이 응용 프로그램에서 사용되는 응용 프로그램 역할 관리자를 구성합니다. RoleManager는 ASP.NET Identity core assembly에서 정의됩니다.
    public class ApplicationRoleManager : RoleManager<CustomRole, int>
    {
        public ApplicationRoleManager(IRoleStore<CustomRole, int> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new CustomRoleStore(context.Get<ApplicationDbContext>()));
        }
    }

    // TODO: 추가된 거 검토
    // This is useful if you do not want to tear down the database each time you run the application.
    // public class ApplicationDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    // This example shows you how to create a new database if the Model changes
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            base.Seed(context);
        }

        //Create User=wanga@wangaland4b.com with password=Wanga@12345 in the Admin role        
        public static void InitializeIdentityForEF(ApplicationDbContext db)
        {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            const string name = "wanga@wangaland4b.com";
            const string password = "Wanga@12345";
            const string roleName = "Admin";

            //Create Role Admin if it does not exist
            var role = roleManager.FindByName(roleName);
            if (role == null)
            {
                role = new CustomRole(roleName);
                var roleresult = roleManager.Create(role);
            }

            var user = userManager.FindByName(name);
            if (user == null)
            {
                user = new ApplicationUser { UserName = name, Email = name };
                var result = userManager.Create(user, password);
                result = userManager.SetLockoutEnabled(user.Id, false);
            }

            // Add user admin to Role Admin if not already added
            var rolesForUser = userManager.GetRoles(user.Id);
            if (!rolesForUser.Contains(role.Name))
            {
                var result = userManager.AddToRole(user.Id, role.Name);
            }
        }
    }
}
