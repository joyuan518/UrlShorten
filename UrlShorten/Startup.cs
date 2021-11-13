using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UrlShorten.WebAPI.Models.Configuration;
using UrlShorten.DataAccess.Interface;
using UrlShorten.DataAccess.Mongo;
using UrlShorten.WebAPI.UrlTokenGenerator;

using Serilog;

namespace UrlShorten.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
             {
                var serverSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:ServerSecret"]));
                options.TokenValidationParameters = new
                                                    TokenValidationParameters
                                                        {
                                                            IssuerSigningKey = serverSecret,
                                                            ValidIssuer = Configuration["JWT:Issuer"],
                                                            ValidAudience = Configuration["JWT:Audience"]
                                                        };
             });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "URL Shorten API",
                    Description = "Web API to generate the shorten url of the internet websites.",
                    Contact = new OpenApiContact
                    {
                        Name = "John Yuan",
                        Email = "dongyuan518@outlook.com",
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration["Redis:ConnStr"];
                options.InstanceName = Configuration["Redis:InstanceName"];                
            });

            services.AddScoped<IUrlDataAccess, UrlDataAccess>(serviceProvider => new UrlDataAccess(Configuration["MongoDB:ConnStr"], Configuration["MongoDB:Database"]));
            services.AddScoped<IUserDataAccess, UserDataAccess>(serviceProvider => new UserDataAccess(Configuration["MongoDB:ConnStr"], Configuration["MongoDB:Database"]));

            services.AddTransient<IUrlTokenGenerator, Md5UrlTokenGenerator>();

            services.Configure<ApiConfiguration>(Configuration.GetSection("API"));
            services.Configure<JwtConfiguration>(Configuration.GetSection("JWT"));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

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
