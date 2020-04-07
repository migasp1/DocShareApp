using System;
using DocShareApp.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DocShareApp.Services;
using System.Text;
using DocShareApp.Mapper;
using MongoDB.Driver;

namespace DocShareApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        //Mine: Here we see dependency injection(services in the constructor)
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddScoped<IUserMapper, UserMapper>();

            services.AddControllers();

            services.AddCors();
           
            services.Configure<KeyOptions>(Configuration.GetSection("KeyOptions"));

            if (Configuration.GetValue<bool>("MongoConnection:UseMongoDb"))
            {
                services.AddSingleton(serviceProvider => new Random());
                var mongoConfSection = Configuration.GetSection("MongoConnection");
                MongoOptions mongoConfig = mongoConfSection.Get<MongoOptions>();
                services.Configure<MongoOptions>(mongoConfSection);
                services.AddSingleton<IMongoClient>(serviceProvider => new MongoClient(mongoConfig.MongoDbConnectionString));
                services.AddScoped<IUserService, MongoDbUserService>();
            }
            else
            {
                services.AddDbContext<DataContext, SqliteDataContext>();
                services.Configure<SqlOptions>(Configuration.GetSection("SqlOptions"));
                services.AddScoped<IUserService, SqlUserService>();
            }

            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("KeyOptions").Get<KeyOptions>().Secret);

            //Definig schema
            services.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.RequireHttpsMetadata = false;
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocShareApp");
            });

            app.UseCors(x => x
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
