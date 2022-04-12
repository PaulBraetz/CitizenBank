using CBData.Entities;
using CBData.Mapping;
using CitizenBank.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PBApp.Configuration;
using PBCommon;
using PBCommon.Extensions;
using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

			var emailData = Configuration.GetSection("EmailData");
			CBCommon.Settings.CitizenBank.EMAIL_SERVER = emailData.GetValue<String>("Server");
			CBCommon.Settings.CitizenBank.EMAIL_NOREPLY_ADDRESS = emailData.GetValue<String>("NoreplyAddress");
			CBCommon.Settings.CitizenBank.EMAIL_NOREPLY_USER = emailData.GetValue<String>("NoreplyUser");
			CBCommon.Settings.CitizenBank.EMAIL_NOREPLY_PASSWORD = emailData.GetValue<String>("NoreplyPassword");
			CBCommon.Settings.CitizenBank.EMAIL_SMTP_PORT = emailData.GetValue<Int32>("SmtpPort");
			CBCommon.Settings.CitizenBank.EMAIL_POP3_PORT = emailData.GetValue<Int32>("Pop3Port");

			//Enum.GetValues<ConsoleLogger.Code>().ForEach(ConsoleLogger.Unlock);
			//ConsoleLogger.Unlock(ConsoleLogger.Code.SRV);
			//ConsoleLogger.Unlock(ConsoleLogger.Code.MLN);
			ConsoleLogger.Unlock(ConsoleLogger.Code.RSD);
			ConsoleLogger.Unlock(ConsoleLogger.Code.RCV);

			services.ConfigurePBApp(c => c
			.ConfigurePBCommon(cc =>
			{
				cc.ConfigureSettingsInitializer(si =>
				{
					si.URL = Configuration.GetSection("SetupData").GetValue<String>("URL").ToString();
#if DEBUG
					si.URL = "https://localhost:5001";
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
						var superAdminClaim = c.GetSingle<IClaimEntity>(c => c.Rights.Contains(PBCommon.Configuration.Settings.OWNER_RIGHT) && c.ValueId == Guid.Empty);
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
