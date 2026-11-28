using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyForge.Application.Authentication;
using StudyForge.Application.AI;
using StudyForge.Infrastructure.Authentication;
using StudyForge.Infrastructure.AI;
using StudyForge.Infrastructure.Identity;
using StudyForge.Infrastructure.Persistence;
namespace StudyForge.Infrastructure;
public static class DependencyInjection
{
 public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
 {
  var cs=configuration.GetConnectionString("StudyForgeDb");
  if(string.IsNullOrWhiteSpace(cs))throw new InvalidOperationException("Connection string 'StudyForgeDb' was not configured.");
  services.AddDbContext<StudyForgeDbContext>(o=>o.UseSqlServer(cs));
  services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
  services.Configure<AiGenerationOptions>(configuration.GetSection(AiGenerationOptions.SectionName));
  services.AddHttpClient<IQuestionGenerationService,AiQuestionGenerationService>((sp,c)=>{var ai=configuration.GetSection(AiGenerationOptions.SectionName).Get<AiGenerationOptions>()??new AiGenerationOptions();c.Timeout=TimeSpan.FromSeconds(Math.Clamp(ai.TimeoutSeconds,5,120));});
  services.AddIdentityCore<StudyForgeUser>(o=>{o.User.RequireUniqueEmail=true;o.Password.RequiredLength=8;o.Password.RequireDigit=true;o.Password.RequireLowercase=true;o.Password.RequireUppercase=true;o.Password.RequireNonAlphanumeric=false;o.Lockout.MaxFailedAccessAttempts=5;o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(10);o.Lockout.AllowedForNewUsers=true;}).AddRoles<IdentityRole>().AddEntityFrameworkStores<StudyForgeDbContext>().AddSignInManager().AddDefaultTokenProviders();
  services.AddScoped<IAuthService,AuthService>();return services;
 }
}