using CBData.Entities;
using CBData.Mapping;
using CitizenBank.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PBApp.Configuration;
using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;
using PBServer.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace CitizenBank
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddServerSideBlazor();

			services.ConfigurePBApp(c => c
			.ConfigurePBCommon(cc =>
			{
				cc.ConfigureSettingsInitializer(si =>
				{
					si.Url = Configuration.GetSection("SetupData").GetSection("URL").GetValue<String>("Production").ToString();
#if DEBUG
					si.Url = Configuration.GetSection("SetupData").GetSection("URL").GetValue<String>("Development").ToString();
#endif

				});
				cc.AddLocalizationResource("CitizenBank.Resources.Strings", Assembly.GetExecutingAssembly());
			})
			.ConfigurePBDataAccess(cda =>
			{
				cda.UseMappingConfiguration(c => c.Add<AccountMappingBase<AccountEntityBase>>()
															.Add<DepartmentMappingBase<DepartmentEntityBase>>()
															.AddFromAssemblyOf<CitizenMapping>());

				String connectionString = Configuration.GetConnectionString("Production");
#if DEBUG
				Console.WriteLine("Use Production Connection? (Y)");
				if (!Console.ReadLine().Equals("Y"))
				{
					connectionString = Configuration.GetConnectionString("Development");
				}
#endif
				cda.UseConnectionString(connectionString);
				if (Configuration.GetSection("SetupData").GetValue<Boolean>("WipeDB"))
				{
					cda.DoWipe();
				}
#if DEBUG
				else
				{
					Console.WriteLine("Wipe Database? (Y)");
					if (Console.ReadLine().Equals("Y"))
					{
						cda.DoWipe();
					}
				}
#endif
				if (Configuration.GetSection("SetupData").GetValue<Boolean>("UpdateDB"))
				{
					cda.DoUpdate();
				}
#if DEBUG
				else
				{
					Console.WriteLine("Update Database? (Y)");
					if (Console.ReadLine().Equals("Y"))
					{
						cda.DoUpdate();
					}
				}
#endif
			})
			.ConfigurePBApplication(ca =>
			{
				IConfigurationSection adminData = Configuration.GetSection("SuperAdminData");
				ca.CreateSuperAdmin(adminData.GetValue<String>("Name"),
						adminData.GetValue<String>("Email"),
						adminData.GetValue<String>("Password"));
				ca.SetOnApplicationConfigured(c =>
				{
					if (!c.Query<CurrencyEntity>().Any())
					{
						var superAdminClaim = c.GetSingle<IClaimEntity>(c => c.Rights.Contains(PBCommon.Configuration.Settings.OwnerRight) && c.ValueId == Guid.Empty);
						var superAdmin = c.GetSingle<UserEntity>(superAdminClaim.HolderId);
						c.Insert(new CurrencyEntity(superAdmin, "aUEC", "aUEC", 0.005M)
						{
							IsActive = true
						});
						c.SaveChanges();
					}
				});
			})
			.ConfigurePBServer(cs =>
			{
				cs.SetDBConnectedServiceContextImplementation<CBObservingServiceContext>()
					.SetUseSignalR()
					.SetUseDefaultControllers(true);

				cs.ConfigureSettingsInitializer(si =>
				{
					var emailData = Configuration.GetSection("NoreplyEmailData");

					si.NoreplyPop3Configuration = new EmailConfiguration(emailData.GetSection("Pop3"));
					si.NoreplySmtpConfiguration = new EmailConfiguration(emailData.GetSection("Smtp"));
					si.DefaultVerifyLinkFormatString = "https://citizen-bank.net/v/{0}";
					si.UseEmailService = true;
				});
			})
			.ConfigurePBFrontend(cf =>
			{
				cf.ConfigureSettingsInitializer(i =>
				{
					i.DefaultPageFrame = PBFrontend.Classes.Formatting.Presets.BlursCss.PageFrame;
					i.DefaultPageContent = PBFrontend.Classes.Formatting.Presets.BlursCss.PageContent;
					i.DefaultPageHeader = PBFrontend.Classes.Formatting.Presets.BlursCss.PageHeader;
					i.DefaultPageFooter = PBFrontend.Classes.Formatting.Presets.BlursCss.PageFooter;

					i.DefaultBoxFrame = PBFrontend.Classes.Formatting.Presets.BlursCss.BoxFrame;

					i.DefaultInfoFrame = PBFrontend.Classes.Formatting.Presets.BlursCss.InfoFrame;
					i.DefaultWarningFrame = PBFrontend.Classes.Formatting.Presets.BlursCss.WarningFrame;
				});
			}));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHub<PBServer.Hubs.EventHub>(PBShared.Events.EventHubSettings.Route);
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
