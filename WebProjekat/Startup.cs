using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebProjekat.DTO;
using WebProjekat.DTO.User;
using WebProjekat.Infrastructure;
using WebProjekat.Interfaces;
using WebProjekat.Mapping;
using WebProjekat.Repository;
using WebProjekat.Repository.Interfaces;
using WebProjekat.Services;

namespace WebProjekat
{
	public class Startup
	{
		private readonly string _cors = "cors";
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllers();
			services.AddDbContext<WSDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("WebShopDatabase")));
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebProjekat", Version = "v1" });
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					In = ParameterLocation.Header,
					Description = "Please enter token",
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					BearerFormat = "JWT",
					Scheme = "bearer"
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type=ReferenceType.SecurityScheme,
								Id="Bearer"
							}
						},
						new string[]{}
					}
				});

			});
			services.AddAuthentication(opt => {
				opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
		   .AddJwtBearer(options =>
		   {
			   options.TokenValidationParameters = new TokenValidationParameters //Podesavamo parametre za validaciju pristiglih tokena
				{
				   ValidateIssuer = true, //Validira izdavaoca tokena
					ValidateAudience = false, //Kazemo da ne validira primaoce tokena
					ValidateLifetime = true,//Validira trajanje tokena
					ValidateIssuerSigningKey = true, //validira potpis token, ovo je jako vazno!
					ValidIssuer = "https://localhost:44321", //odredjujemo koji server je validni izdavalac
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))//navodimo privatni kljuc kojim su potpisani nasi tokeni
				};
		   });
			services.AddCors(options =>
			{
				options.AddPolicy(name: _cors, builder => {
					builder.WithOrigins("http://localhost:3000")//Ovde navodimo koje sve aplikacije smeju kontaktirati nasu,u ovom slucaju nas Angular front
						   .AllowAnyHeader()
						   .AllowAnyMethod()
						   .AllowCredentials();
				});
			});


			var mapperConfig = new MapperConfiguration(mc =>
			{
				mc.AddProfile(new MappingProfile());
			});

			IMapper mapper = mapperConfig.CreateMapper();
			services.AddSingleton(mapper);
			services.AddMvc();
			//services.AddFluentValidationAutoValidation();
			/*services.AddScoped<IValidator<RegistrationUserDto>, RegisterUserDTOValidator>();
			services.AddScoped<IValidator<LogInDto>, LogInUserDTOValidator>();
			services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDTOValidator>();
			services.AddScoped<IValidator<PasswordDto>, ChangePasswordDTOValidator>();
			services.AddScoped<IValidator<ItemDto>, ProductDTOValidator>();
			services.AddScoped<IValidator<OrderItemDto>, OrderItemDTOValidator>();
			services.AddScoped<IValidator<OrderDto>, OrderDTOValidator>();*/

			services.AddScoped<IUserRepo, UserRepo>();
			services.AddScoped<IItemRepo, ItemRepo>();
			services.AddScoped<IOrderRepo, OrderRepo>();

			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IItemService, ItemService>();
			services.AddScoped<IOrderService, OrderService>();

		}



		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebProjekat v1"));
			}

			app.UseHttpsRedirection();
			app.UseCors(_cors);

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
