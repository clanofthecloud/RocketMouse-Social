using System;
using CotcSdk;

public class GamerInfo
{
	Bundle Data;

	public GamerInfo (string persistedJsonData)
	{
		Data = Bundle.FromJson (persistedJsonData);
	}

	public GamerInfo(Gamer gamer) {
		GamerId = gamer.GamerId;
		GamerSecret = gamer.GamerSecret;
	}

	public string GamerId {
		get { return Data ["gamer_id"]; }
		set { Data ["gamer_id"] = value; }
	}

	public string GamerSecret {
		get { return Data ["gamer_secret"]; }
		set { Data ["gamer_secret"] = value; }
	}

	public string ToJson() {
		return Data.ToJson ();
	}
}

