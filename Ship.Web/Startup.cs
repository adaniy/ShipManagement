﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ship.Infrastructure.Data;
using Ship.Infrastructure.Dependency;
using Ship.Infrastructure.Services;
using Ship.Web.Models;
using Ship.Web.Validation;
using Ship.Web.Resources;

namespace Ship.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DefaultDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly("Ship.Web")));

            services.AddSingleton<IScopedServiceResolver, RequestScopedServiceResolver>();
            ServiceLocator.Instance.SetServiceCollection(services);

            services.AddScoped<CertificateService>();
            services.AddScoped<CertificateTypeService>();
            services.AddScoped<CompanyService>();
            services.AddScoped<ContractService>();
            services.AddScoped<ExamService>();
            services.AddScoped<ExperienceService>();
            services.AddScoped<FamilyService>();
            services.AddScoped<InterviewService>();
            services.AddScoped<LaborSupplyService>();
            services.AddScoped<NoticeService>();
            services.AddScoped<SailorService>();
            services.AddScoped<ServiceRecordService>();
            services.AddScoped<ShipownerService>();
            services.AddScoped<SysCompanyService>();
            services.AddScoped<TitleService>();
            services.AddScoped<TraineeService>();
            services.AddScoped<TrainingClassService>();
            services.AddScoped<UploadFileService>();
            services.AddScoped<VesselAccountService>();
            services.AddScoped<VesselCertificateService>();
            services.AddScoped<VesselService>();
            services.AddScoped<WageService>();

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(Configuration.GetConnectionString("IdentityConnection")));

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
                //options.User.AllowedUserNameCharacters = "qwertyuiopasdfghjklzxcvbnm";
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies(options => { });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddMvc(o =>
            {
                o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                   value =>
                    SiteResources.CustomImplicitRequired);
                o.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
                   (value, fieldName) => string.Format(SiteResources.CustomInvalidValue, value));
                o.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
                    fieldName => string.Format(SiteResources.CustomNumeric, fieldName));
                o.ModelMetadataDetailsProviders.Add(
                    new CustomValidationMetadataProvider("Ship.Web.Resources.SiteResources",
                        typeof(SiteResources)));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            ServiceLocator.Instance.SetApplicationServiceProvider(app.ApplicationServices);
            ApplicationDbContext.CreateAdminAccount(app.ApplicationServices, Configuration).Wait();
        }
    }
}
