using System;

namespace TestClient.Crypto
{
	public class KeyExchange
	{
		public const string KeyBase = "dd80c2e508b630998076a9f7319c930d954f2866f53932baa2938467f25ed069";
		public const string KeyPrime = "dd80c2e508b630998076a9f7319c930d954f2866f53932baa2938467f2602bfb";

		private VLong b;
		private VLong p;

		private VLong privateKey;

		public void Init()
		{
			Init(
				VLong.Parse(KeyBase, true),
				VLong.Parse(KeyPrime, true));
		}

		public void Init(VLong sharedBase, VLong sharedPrime)
		{
			this.b = sharedBase;
			this.p = sharedPrime;
		}

		public void InitClient()
		{
			Init();
			GenerateClientPrivateKey();
		}

		public void InitServer()
		{
			Init();
			GenerateServerPrivateKey();
		}

		private void GenerateClientPrivateKey()
		{
			this.privateKey = new VLong(10);
		}

		private void GenerateServerPrivateKey()
		{
			this.privateKey = new VLong(11);
		}

		public VLong GeneratePublic()
		{
			return GeneratePublic(this.privateKey);
		}

		public VLong GeneratePublic(VLong privateKey)
		{
			return VLong.ModExp(this.b, privateKey, this.p);
		}

		public VLong GenerateSession(VLong exchangePublicKey)
		{
			return GenerateSession(exchangePublicKey, this.privateKey);
		}

		public VLong GenerateSession(VLong exchangePublicKey, VLong privateKey)
		{
			return VLong.ModExp(exchangePublicKey, privateKey, this.p);
		}
	}
}
