using System;

namespace TestClient.Crypto
{
	// start @ ReceiverData__InitCrypto
	public class CryptoSystem
	{
		private uint seed;
		private Rand isaac;

		public uint Seed
		{
			get { return this.seed; }
			set { CreateRandomGen(value); }
		}

		public CryptoSystem(uint seed)
		{
			CreateRandomGen(seed);
		}

		public uint GetSendKey()
		{
			return unchecked((uint)isaac.val());
		}

		private void CreateRandomGen(uint seed)
		{
			this.seed = seed;
			int signed_seed = unchecked((int)seed);
			this.isaac = new Rand(signed_seed, signed_seed, signed_seed);
		}
	}
}