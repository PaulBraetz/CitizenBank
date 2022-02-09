using CBCommon.Extensions;
using CBCommon.Settings;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MimeKit;
using PBApplication.Context.Abstractions;
using PBApplication.Services;
using PBApplication.Services.Abstractions;
using PBCommon;
using PBCommon.Globalization;
using PBData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBApplication.Services
{
	public sealed class EmailService : DBConnectedService, IEmailService
	{
		private const String EMAIL_TEMPLATE = "<div style=\"flex: 1; display: flex !important; flex-direction: column !important;background-color: white;color: black; \"><div style=\"background-color: rgba(255,255,255,0.01); overflow: auto !important; flex-grow: 1 !important; box-shadow: 0 .5rem 1rem rgba(0,0,0,.15) !important; margin: .25rem !important;\"><div style=\"justify-content: center !important; display: flex !important; box-shadow: 0 .125rem .25rem rgba(0,0,0,.075) !important; padding: .25rem !important;\"><h1 style=\" margin: .25rem !important;\">{0}</h1></div><div class=\" padding: .25rem !important;\">{1}</div></div></div>";

		public EmailService(IServiceContext serviceContext) : base(serviceContext)
		{
		}

		private String GetLocalized(String value) => LocalizationManager.Get(value, Session.CultureName);

		private const String IGNORE_EMAIL_BODY_PART = "If you did not request this, please ignore this email.";
		private const String IGNORE_EMAIL_CHANGE_PASSWORD_BODY_PART = "If you did not request this, please ignore this email and consider changing your password.";
		private const String HEADER_USER_GREETINGS = "Hello, {0}";
		private const String EMAIL_PRIVACY_DISCLAIMER = "Your email address will not be stored in an unencrypted format and no record of this email will be kept.";

		private const String CHANGE_EMAIL_SUBJECT = "Change Email Confirmation";
		private const String CHANGEEMAIL_BODY_PART_1 = "Click this link to confirm your email change : {0}";
		private const String CHANGEEMAIL_BODY_PART_2 = "Your user email will be changed to : {0}";

		public async Task ChangeEmail(String name, String email, String verificationCode, String newEmail)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(ChangeEmail));

			var bodyPart1 = GetLocalized(CHANGEEMAIL_BODY_PART_1);
			var bodyPart1WithLink = String.Format(bodyPart1, verificationCode.ToVerifyLink());
			var bodyPart2 = GetLocalized(CHANGEEMAIL_BODY_PART_2);
			var bodyPart2WithEmail = String.Format(bodyPart2, newEmail);
			var ignoreEmailChangePasswordPart = GetLocalized(IGNORE_EMAIL_CHANGE_PASSWORD_BODY_PART);
			await SendEmail(name, email, CHANGE_EMAIL_SUBJECT, bodyPart1WithLink, bodyPart2WithEmail, ignoreEmailChangePasswordPart);
		}

		private const String REGISTER_SUBJECT = "Registration Confirmation";
		private const String REGISTER_BODY_PART = "Click this link to confirm your registration : {0}";
		public async Task Register(String name, String email, String verificationCode)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(Register));

			var bodyPart = GetLocalized(REGISTER_BODY_PART);
			var bodyPartWithLink = String.Format(bodyPart, verificationCode.ToVerifyLink());
			var ignoreEmailPart = GetLocalized(IGNORE_EMAIL_BODY_PART);
			await SendEmail(name, email, REGISTER_SUBJECT, bodyPartWithLink, ignoreEmailPart);
		}

		private const String DELETE_SUBJECT = "Deletion Notification";
		private const String DELETE_BODY_PART = "Your user has successfully been deleted.";
		public async Task DeleteUser(String name, String email)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(DeleteUser));

			var bodyPart = GetLocalized(DELETE_BODY_PART);
			await SendEmail(name, email, DELETE_SUBJECT,  bodyPart);
		}

		private const String PASSWORD_SUBJECT = "Change Password Confirmation";
		private const String PASSWORD_BODY_PART = "Click this link to confirm your password change : {0}";
		public async Task RequestPasswordChange(String name, String email, String verificationCode)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(RequestPasswordChange));

			var bodyPart = GetLocalized(PASSWORD_BODY_PART);
			var bodyPartWithLink = String.Format(bodyPart, verificationCode.ToVerifyLink());
			var ignoreEmailPart = GetLocalized(IGNORE_EMAIL_BODY_PART);
			await SendEmail(name, email, PASSWORD_SUBJECT, bodyPartWithLink, ignoreEmailPart);
		}

		private const String AUTH_SUBJECT = "Two Factor Authentication Confirmation";
		private const String AUTH_BODY_PART = "Your two factor authentication code is : {0}";
		public async Task TwoFactorAuthentication(String name, String email, String verificationCode)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(TwoFactorAuthentication));

			var bodyPart = GetLocalized(AUTH_BODY_PART);
			var bodyPartWithLink = String.Format(bodyPart, verificationCode);
			var ignoreEmailChangePasswordPart = GetLocalized(IGNORE_EMAIL_CHANGE_PASSWORD_BODY_PART);
			await SendEmail(name, email, AUTH_SUBJECT, bodyPartWithLink, ignoreEmailChangePasswordPart);
		}

		private async Task SendEmail(String name, String email, String subject, params String[] textBodyParts)
		{
			var localizedSubject = GetLocalized(subject);

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(CitizenBank.EMAIL_NOREPLY_USER, CitizenBank.EMAIL_NOREPLY_ADDRESS));
			message.To.Add(new MailboxAddress(name, email));
			message.Subject = localizedSubject;

			var localizedGreeting = String.Format(GetLocalized(HEADER_USER_GREETINGS), name);
			IEnumerable<String> bodyParts = textBodyParts.Prepend(localizedGreeting);

			var localizedDisclaimer = GetLocalized(EMAIL_PRIVACY_DISCLAIMER);
			bodyParts = bodyParts.Append(localizedDisclaimer);

			var bodyBuilder = new BodyBuilder();
			bodyBuilder.HtmlBody = String.Format(EMAIL_TEMPLATE, localizedSubject, $"<div style=\"white-space: pre-line;\">{String.Join('\n', bodyParts)}</div>");
			bodyBuilder.TextBody = $"{localizedSubject}\n{String.Join('\n', bodyParts)}";
			message.Body = bodyBuilder.ToMessageBody();

			using (var client = new SmtpClient())
			{
				await client.ConnectAsync(CitizenBank.EMAIL_SERVER, CitizenBank.EMAIL_SMTP_PORT);
				await client.AuthenticateAsync(CitizenBank.EMAIL_NOREPLY_ADDRESS, CitizenBank.EMAIL_NOREPLY_PASSWORD);
				await client.SendAsync(message);
				await client.DisconnectAsync(true);
			}

			using (var pop3Client = new Pop3Client())
			{
				await pop3Client.ConnectAsync(CitizenBank.EMAIL_SERVER, CitizenBank.EMAIL_POP3_PORT);
				await pop3Client.AuthenticateAsync(CitizenBank.EMAIL_NOREPLY_ADDRESS, CitizenBank.EMAIL_NOREPLY_PASSWORD);
				await pop3Client.DeleteAllMessagesAsync();
				await pop3Client.DisconnectAsync(true);
			}
		}
	}
}
