using System.Collections.Generic;
using System.Net.Http;

namespace Litdex.Security.RNG.TRNG
{
	/// <summary>
	///		TRNG from https://makemeapassword.ligos.net based on Terninger.
	/// </summary>
	public class Ligos : TrueRandom
	{
		#region Constructor & Destructor

		/// <summary>
		///		TRNG from https://makemeapassword.ligos.net.
		/// </summary>
		/// <param name="entropySize">
		///		The size of internal state for RNG in <see cref="byte"/>.
		///	</param>
		private Ligos(ushort entropySize = 2048)
		{
			this._Entropy = new List<byte>(entropySize);
		}

		/// <summary>
		///		TRNG from https://makemeapassword.ligos.net.
		/// </summary>
		/// <param name="client">
		///		Client for sending and receive http response.
		/// </param>
		public Ligos(HttpClient client = null) : this(2048)
		{
			if (client == null)
			{
				this._IsSupplied = false;
				this._HttpClient = new HttpClient();
				this.AddHttpUserAgent();
			}
			else
			{
				this._IsSupplied = true;
				this._HttpClient = client;
			}
		}

		~Ligos()
		{
			if (this._IsSupplied == false)
			{
				this._HttpClient.Dispose();
			}
			this._Entropy.Clear();
		}

		#endregion Constructor & Destructor

		#region Public Method

		/// <inheritdoc/>
		public override string AlgorithmName()
		{
			return "Ligos True Random Generator";
		}

		/// <inheritdoc/>
		public override void Reseed()
		{
			this._Entropy.Clear();
			var hex = this.GetStringResponseAsync("https://makemeapassword.ligos.net/api/v1/hex/plain?c=32&l=128").GetAwaiter().GetResult();
			hex.Replace("\n", "");
			this._Entropy.AddRange(DecodeBase16(hex));
		}

		#endregion Public Method
	}
}
