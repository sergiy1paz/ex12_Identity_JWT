using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ex12_Identity_JWT.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using ex12_Identity_JWT.Models;
using ex12_Identity_JWT.JWT.Interfaces;
using ex12_Identity_JWT.JWT.Implementations;
using Microsoft.AspNetCore.Authorization;
using ex12_Identity_JWT.Requirements;
using System.Security.Claims;
using ex12_Identity_JWT.Requirements.Claims;

namespace ex12_Identity_JWT
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
            // handler for age limit requirement
            services.AddTransient<IAuthorizationHandler, AgeHandler>();


            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationContext>();

            services.AddAuthentication(options => 
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                
            }).AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // jwt configuration
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["AuthOptions:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = Configuration["AuthOptions:Audience"],

                        ValidateLifetime = true,

                        IssuerSigningKey = IJWTGenerator.GetSymmetricSecurityKey(Configuration["AuthOptions:TokenKey"]),
                        ValidateIssuerSigningKey = true
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AgeLimit", policy =>
                {
                    policy.AddRequirements(new AgeRequirement(int.Parse(Configuration["AgeLimit"])));
                });

                options.AddPolicy("PaidContent", policy =>
                {
                    policy.RequireClaim(CustomClaims.SUBSCRIPTION, Subscriptions.PREMIUM, Subscriptions.VIP);
                    
                });

                options.AddPolicy("Developer", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        var user = context.User;
                        return user.IsInRole(Roles.MODERATOR) || user.IsInRole(Roles.ADMIN) && user.HasClaim(claim => claim.Type == CustomClaims.COMPANY);
                    });
                });
            });

            // add jwt generator injection
            services.AddScoped<IJWTGenerator, JWTGenerator>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ex12_Identity_JWT", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ex12_Identity_JWT v1"));
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
