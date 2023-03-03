using CoreLibrary.Models;

namespace CoreLibrary.Utils
{
    public static class CoinsUtils
    {
        public static List<CryptoCoin> GetCoins()
        {
            return File.ReadAllLines("Coins.txt")
                    .Select(c => new CryptoCoin(c))
                    .Distinct()
                    .ToList();
        }
    }
}
