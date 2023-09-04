using ARD_Grup_WebApi.Data;
using ARD_Grup_WebApi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ARD_Grup_WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseWebRoot("wwwroot");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddDbContext<ARD_DbContext>(opts =>
            {
                opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
                opts.UseLazyLoadingProxies();
            });

            builder.Services.AddScoped<UserService>();

            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ARD_DbContext>();

                var adminExists = dbContext.Users.Any(u => u.Roles.Any(r => r.Name == "Admin"));
                if (!adminExists)
                {
                    var adminRole = new Role { Name = "Admin" };

                    var adminUser = new User
                    {
                        NameSurname = "Tolga Ödemiþ",
                        Email = "admin@example.com",
                        Password = "ardgrupadmin",
                        PhoneNumber = "1234567890",
                        Roles = new List<Role> { adminRole }
                    };

                    var team = new Team
                    {
                        TeamName = "Admin Team"
                    };
                    adminUser.Teams = team;

                    dbContext.Roles.Add(adminRole);
                    dbContext.Users.Add(adminUser);
                    dbContext.SaveChanges();

                    var adminUserId = adminUser.Id.ToString();
                   

                    if (adminRole != null)
                    {
                        adminUser.Roles.Add(adminRole);
                        dbContext.SaveChanges();
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
                    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, adminUserId),
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Role, "Admin"),
                };

                    var token = new JwtSecurityToken(
                             issuer: builder.Configuration["Jwt:Issuer"],
                             audience: builder.Configuration["Jwt:Audience"],
                             claims: claims,
                             expires: DateTime.Now.AddDays(7),
                             signingCredentials: credentials
                         );

                    var adminJwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                }

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.WithOrigins("http://localhost:4200", "http://localhost:44349", "https://localhost:4200", "https://localhost:44349", "http://localhost:7020", "https://localhost:7020")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
                });

                builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(opts =>
                {
                    opts.Cookie.Name = "ARDGroup.aut";
                    opts.ExpireTimeSpan = TimeSpan.Zero;
                    opts.SlidingExpiration = false;
                    opts.LoginPath = "/Account/Login";
                    opts.LogoutPath = "/Account/Logout";
                    opts.AccessDeniedPath = "/Home/AccessDenied";
                });

                builder.Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = true,
                        //ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        //ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });
                

                builder.Services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseDefaultFiles();
                app.UseStaticFiles();
                app.UseRouting();

                app.UseCors("CorsPolicy");

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Account}/{action=Login}/{id?}");

                app.Run();
            }
        }
    }
}
