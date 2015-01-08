using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Wangaland4B.Auth.Models
{
    // ApplicationUser 클래스에 더 많은 속성을 추가하여 사용자에 대한 프로필 데이터를 추가할 수 있습니다. 자세히 알아보려면 http://go.microsoft.com/fwlink/?LinkID=317594를 방문하십시오.
    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // authenticationType은 CookieAuthenticationOptions.AuthenticationType에 정의된 항목과 일치해야 합니다.
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 여기에 사용자 지정 사용자 클레임 추가
            return userIdentity;
        }

        [Display(Name = "도/시")]
        public string State { get; set; }
        [Display(Name = "시/구/군")]
        public string City { get; set; }
        [Display(Name = "상세 주소")]
        public string Address { get; set; }

        // Use a sensible display name for views:
        [Display(Name = "우편 번호")]
        public string PostalCode { get; set; }

        // Concatenate the address info for display in tables and such:
        public string DisplayAddress
        {
            get
            {
                string dspState = string.IsNullOrWhiteSpace(this.State) ? "" : this.State;
                string dspCity = string.IsNullOrWhiteSpace(this.City) ? "" : this.City;
                string dspAddress = string.IsNullOrWhiteSpace(this.Address) ? "" : this.Address;
                string dspPostalCode = string.IsNullOrWhiteSpace(this.PostalCode) ? "" : this.PostalCode;

                return string.Format("{0} {1} {2} {3}", dspState, dspCity, dspAddress, dspPostalCode);
            }
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim> 
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        // TODO: 추가된거 검토
        static ApplicationDbContext()
        {
            // Set the database intializer which is run once during application start
            // This seeds the database with admin user credentials and admin role
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class CustomUserRole : IdentityUserRole<int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }

    [MetadataType(typeof(CustomRoleMetaData))]
    public class CustomRole : IdentityRole<int, CustomUserRole>
    {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
        // 추가 컬럼
        [Display(Name="역할 설명")]
        public string Description { get; set; }
    }

    // 메타데이터
    internal sealed class CustomRoleMetaData
    {
        [Display(Name="역할 이름")]
        public string Name { get; set; }
    }

    public class CustomUserStore : UserStore<ApplicationUser, CustomRole, int,
        CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public CustomUserStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole>
    {
        public CustomRoleStore(ApplicationDbContext context)
            : base(context)
        {
        }
    } 
}