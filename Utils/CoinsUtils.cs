using CryptoArbitrageMonitoring.Models;
using Newtonsoft.Json;

namespace CryptoArbitrageMonitoring.Utils
{
    public static class CoinsUtils
    {
        public static List<CryptoCoin> GetCoins()
        {
            return JsonConvert.DeserializeObject<List<CryptoCoin>>(File.ReadAllText("Coins.txt"));
        }
    }
}
