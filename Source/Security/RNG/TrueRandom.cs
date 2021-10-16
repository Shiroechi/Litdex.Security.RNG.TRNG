using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Litdex.Utilities;

namespace Litdex.Security.RNG
{
	/// <summary>
	///		Base class for True Random Generator.
	/// </summary>
	public abstract class TrueRandom : IRNG
	{
		#region Member

		protected HttpClient _HttpClient;
		protected List<byte> _Entropy;
		protected bool _IsSupplied;
		protected const string _DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36.";

		#endregion Member

		#region Protected Method

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

		#endregion Protected Method

		#region Public Method

		/// <summary>
		///		Add http user agent if not exist.
		/// </summary>
		/// <remarks>
		///		by default using Google Chrome browser user agent.
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

			if (string.IsNullOrWhiteSpace(userAgent))
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

		/// <inheritdoc/>
		public virtual string AlgorithmName()
		{
			return "True Random Generator";
		}

		/// <summary>
		///		Refill the entropy from the source.
		/// </summary>
		public virtual void Reseed()
		{
			throw new NotImplementedException("True randon generator can't use Reseed.");
		}

		/// <inheritdoc/>
		public virtual bool NextBoolean()
		{
			return this.NextByte() >> 7 == 0;
		}

		/// <inheritdoc/>
		public virtual byte NextByte()
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
		public virtual byte NextByte(byte lower, byte upper)
		{
			if (lower >= upper)
			{
				throw new ArgumentException(nameof(lower), "The lower bound must not be greater than or equal to the upper bound.");
			}

			return (byte)this.NextInt(lower, upper);
		}

		/// <inheritdoc/>
		public virtual byte[] NextBytes(int length)
		{
			if (length <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(length), $"The requested output size can't lower than 1.");
			}

			var bytes = new byte[length];

			this.Fill(bytes);

			return bytes;
		}

		/// <inheritdoc/>
		public virtual byte[] NextBytesLittleEndian(int length)
		{
			return this.NextBytes(length);
		}

		/// <inheritdoc/>
		public virtual byte[] NextBytesBigEndian(int length)
		{
			return this.NextBytes(length);
		}

		/// <inheritdoc/>
		public virtual void Fill(byte[] bytes)
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
		public virtual void FillLittleEndian(byte[] bytes)
		{
			this.Fill(bytes);
		}

		/// <inheritdoc/>
		public virtual void FillBigEndian(byte[] bytes)
		{
			this.Fill(bytes);
		}

#if NET5_0_OR_GREATER

		/// <inheritdoc/>
		public virtual void Fill(Span<byte> bytes)
		{
			this.Fill(bytes);
		}

		/// <inheritdoc/>
		public virtual void FillLittleEndian(Span<byte> bytes)
		{
			this.Fill(bytes);
		}

		/// <inheritdoc/>
		public virtual void FillBigEndian(Span<byte> bytes)
		{
			this.Fill(bytes);
		}

#endif

		/// <inheritdoc/>
		public virtual uint NextInt()
		{
			var buffer = new byte[4];
			for (var i = 0; i < buffer.Length; i++)
			{
				buffer[i] = this.NextByte();
			}

			uint value = 0;
#if NET5_0_OR_GREATER
			System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(buffer);
#else
			value = BitConverter.ToUInt32(buffer, 0);
#endif
			return value;
		}

		/// <inheritdoc/>
		public virtual uint NextInt(uint lower, uint upper)
		{
			if (lower >= upper)
			{
				throw new ArgumentException(nameof(lower), "The lower bound must not be greater than or equal to the upper bound.");
			}

			// using unbiased lemire method
			// from https://www.pcg-random.org/posts/bounded-rands.html

			var range = upper - lower;
			uint x = this.NextInt();
			ulong m = (ulong)x * range;
			uint l = (uint)m;
			if (l < range)
			{
				uint t = uint.MaxValue - range;
				if (t >= range)
				{
					t -= range;
					if (t >= range)
					{
						t %= range;
					}
				}
				while (l < t)
				{
					x = this.NextInt();
					m = (ulong)x * range;
					l = (uint)m;
				}
			}
			return (uint)(m >> 32) + lower;
		}

		/// <inheritdoc/>
		public virtual ulong NextLong()
		{
			var buffer = new byte[8];
			for (var i = 0; i < buffer.Length; i++)
			{
				buffer[i] = this.NextByte();
			}

			ulong value = 0;
#if NET5_0_OR_GREATER
			System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(buffer);
#else
			value = BitConverter.ToUInt32(buffer, 0);
#endif
			return value;
		}

		/// <inheritdoc/>
		public virtual ulong NextLong(ulong lower, ulong upper)
		{
			if (lower >= upper)
			{
				throw new ArgumentException(nameof(lower), "The lower bound must not be greater than or equal to the upper bound.");
			}

			// using unbiased lemire method
			// from https://www.pcg-random.org/posts/bounded-rands.html

			var range = upper - lower;
			ulong x = this.NextLong();
			var (m, l) = Math128.Multiply(x, range);
			if (l < range)
			{
				ulong t = ulong.MaxValue - range;
				if (t >= range)
				{
					t -= range;
					if (t >= range)
					{
						t %= range;
					}
				}
				while (l < t)
				{
					x = this.NextLong();
					(m, l) = Math128.Multiply(x, range);
				}
			}
			return m;
		}

		/// <inheritdoc/>
		public virtual double NextDouble()
		{
			return (this.NextLong() >> 11) * (1.0 / (1L << 53));
		}

		/// <inheritdoc/>
		public virtual double NextDouble(double lower, double upper)
		{
			if (lower >= upper)
			{
				throw new ArgumentException(nameof(lower), "The lower bound must not be greater than or equal to the upper bound.");
			}

			var diff = upper - lower + 1;
			return lower + (this.NextDouble() % diff);
		}

		#endregion Public Method
	}
}
