using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Litdex.Security.RNG
{
	/// <summary>
	///		Base class for True Random Generator.
	/// </summary>
	public abstract class TrueRandom : Random
	{
		#region Member

		protected HttpClient _HttpClient;
		protected List<byte> _Entropy;
		protected bool _IsSupplied;
		protected const string _DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.85 Safari/537.36";

		#endregion Member

		#region Protected Member

		/// <summary>
		///		Decode hexadecimal string to array of <see cref="byte"/>.
		/// </summary>
		/// <param name="hexString">
		///		Hexadecimal string to decode.
		///	</param>
		/// <returns>
		///		Array of <see cref="byte"/> of decoded <paramref name="hexString"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="hexString"/> is null, empty or containing white spaces.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<paramref name="hexString"/> length is odd.
		/// </exception>
		protected static byte[] DecodeBase16(string hexString)
		{
			if (string.IsNullOrWhiteSpace(hexString))
			{
				throw new ArgumentNullException(nameof(hexString), "Hexadecimal string can't null, empty or containing white spaces.");
			}

			if (hexString.Length % 2 != 0)
			{
				throw new ArgumentOutOfRangeException(nameof(hexString), "The hexadecimal string is invalid because it has an odd length.");
			}

			var result = new byte[hexString.Length / 2];

			for (var i = 0; i < result.Length; i++)
			{
				int high = hexString[i * 2];
				int low = hexString[i * 2 + 1];
				high = (high & 0xf) + ((high & 0x40) >> 6) * 9;
				low = (low & 0xf) + ((low & 0x40) >> 6) * 9;

				result[i] = (byte)((high << 4) | low);
			}

			return result;
		}

		/// <summary>
		///		Add http user agent if not exist.
		/// </summary>
		/// <remarks>
		///		by default using browser user agent.
		/// </remarks>
		/// <param name="userAgent">
		///		User Agent value.
		///	</param>
		public void AddHttpUserAgent(string userAgent = "")
		{
			if (this._HttpClient == null)
			{
				return;
			}

			if (this._HttpClient.DefaultRequestHeaders.UserAgent.Count != 0)
			{
				this._HttpClient.DefaultRequestHeaders.UserAgent.Clear();
			}

			if (userAgent == null || userAgent.Trim() == "")
			{
				this._HttpClient.DefaultRequestHeaders.Add(
				"User-Agent",
				_DefaultUserAgent);
			}
			else
			{
				this._HttpClient.DefaultRequestHeaders.Add(
				"User-Agent",
				userAgent);
			}
		}

		/// <summary>
		///		Get JSON response from url.
		/// </summary>
		/// <typeparam name="T">
		///		The type of the object to deserialize.
		///	</typeparam>
		/// <param name="url">
		///		URL of the request. 
		/// </param>
		/// <returns>
		///		The instance of <typeparamref name="T"/> being deserialized.
		///	</returns>
		/// <exception cref="Exception">
		///		Unexpected error occured.
		/// </exception>
		/// <exception cref="HttpRequestException">
		///		The request failed due to an underlying issue such as network connectivity, DNS
		///     failure, server certificate validation or timeout.
		/// </exception>
		/// <exception cref="TaskCanceledException">
		///		The request failed due timeout.
		/// </exception>
		/// <exception cref="JsonException">
		///		The JSON is invalid.
		/// </exception>
		protected async Task<T> GetJsonResponseAsync<T>(string url)
		{
			try
			{
				using (var request = new HttpRequestMessage(HttpMethod.Get, url))
				using (var response = await this._HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
				using (var stream = await response.Content.ReadAsStreamAsync())
				{
					if (response.IsSuccessStatusCode)
					{
						try
						{
							return await JsonSerializer.DeserializeAsync<T>(stream);
						}
						catch (JsonException)
						{
							throw;
						}
					}

					throw new Exception(
						$"Unexpected error occured.\nStatus code = { (int)response.StatusCode }\nReason = { response.ReasonPhrase }.");
				}
			}
			catch (HttpRequestException)
			{
				throw;
			}
			catch (TaskCanceledException)
			{
				throw;
			}
		}

		/// <summary>
		///		Get <see cref="string"/> response from url.
		/// </summary>
		/// <param name="url">
		///		URL of request.
		/// </param>
		/// <returns>
		///		<see cref="string"/> response.
		///	</returns>
		/// <exception cref="Exception">
		///		Unexpected error occured.
		/// </exception>
		/// <exception cref="HttpRequestException">
		///		The request failed due to an underlying issue such as network connectivity, DNS
		///     failure, server certificate validation or timeout.
		/// </exception>
		/// <exception cref="TaskCanceledException">
		///		The request failed due timeout.
		/// </exception>
		protected async Task<string> GetStringResponseAsync(string url)
		{
			try
			{
				using (var request = new HttpRequestMessage(HttpMethod.Get, url))
				using (var response = await this._HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
				using (var stream = await response.Content.ReadAsStreamAsync())
				{
					if (response.IsSuccessStatusCode)
					{
						return await this.DeserializeStringFromStreamAsync(stream);
					}

					throw new Exception(
						$"Unexpected error occured.\nStatus code = { (int)response.StatusCode }\nReason = { response.ReasonPhrase }.");
				}
			}
			catch (HttpRequestException)
			{
				throw;
			}
			catch (TaskCanceledException)
			{
				throw;
			}
		}

		/// <summary>
		///		Deserializes response into string.
		/// </summary>
		/// <param name="stream">
		///		Array of <see cref="byte"/>s to deserialize.
		/// </param>
		/// <returns>
		///		<see cref="string"/> content.
		///	</returns>
		protected async Task<string> DeserializeStringFromStreamAsync(Stream stream)
		{
			if (stream != null)
			{
				using (var sr = new StreamReader(stream, Encoding.UTF8))
				{
					return await sr.ReadToEndAsync();
				}
			}

			return null;
		}

		#endregion Protected Member

		#region Public Method

		/// <inheritdoc/>
		public override string AlgorithmName()
		{
			return "True Random Generator";
		}

		/// <summary>
		///		Refill the entropy from the source.
		/// </summary>
		public override void Reseed()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool NextBoolean()
		{
			return this.NextByte() >> 7 == 0;
		}

		/// <inheritdoc/>
		public override byte NextByte()
		{
			if (this._Entropy.Count <= 0)
			{
				this.Reseed();
			}

			var temp = this._Entropy[0];
			this._Entropy.RemoveAt(0);
			return temp;
		}

		/// <inheritdoc/>
		public override byte[] NextBytes(int length)
		{
			if (length <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(length), $"The requested output size can't lower than 1.");
			}

			var bytes = new byte[length];

			for (var i = 0; i < length; i++)
			{
				//bytes[i] = this._Entropy[i];
				bytes[i] = this.NextByte();
			}
			return bytes;
		}

		/// <inheritdoc/>
		public override void Fill(byte[] bytes)
		{
			if (bytes.Length <= 0 || bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes), "Array length can't be lower than 1 or null.");
			}

			for (var i = 0; i < bytes.Length; i++)
			{
				bytes[i] = this.NextByte();
			}
		}

		/// <inheritdoc/>
		public override uint NextInt()
		{
			if (this._Entropy.Count < 4)
			{
				this.Reseed();
			}

			var temp = BitConverter.ToUInt32(this._Entropy.GetRange(0, 4).ToArray(), 0);
			this._Entropy.RemoveRange(0, 4);
			return temp;
		}

		/// <inheritdoc/>
		public override ulong NextLong()
		{
			if (this._Entropy.Count < 8)
			{
				this.Reseed();
			}

			var temp = BitConverter.ToUInt32(this._Entropy.GetRange(0, 8).ToArray(), 0);
			this._Entropy.RemoveRange(0, 8);
			return temp;
		}

		#endregion Public Method
	}
}
