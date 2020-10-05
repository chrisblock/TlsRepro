using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace TlsRepro.Broken
{
	public static class Program
	{
		private const string Url = "https://localhost";

		public static void Main(string[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback += ValidateCertificate;

			try
			{
				MakeHttpsRequest();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			PressAnyKeyToContinue();
		}

		private static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private static void MakeHttpsRequest()
		{
			using (var client = new HttpClient())
			{
				var getTask = client.GetAsync(Url);

				getTask.Wait();

				var response = getTask.Result;

				var contentTask = response.Content.ReadAsStringAsync();

				contentTask.Wait();

				var content = contentTask.Result;

				Console.WriteLine(content);
			}
		}

		[Conditional("DEBUG")]
		private static void PressAnyKeyToContinue()
		{
			Console.WriteLine("Press any key to continue.");

			Console.ReadKey();
		}
	}
}
