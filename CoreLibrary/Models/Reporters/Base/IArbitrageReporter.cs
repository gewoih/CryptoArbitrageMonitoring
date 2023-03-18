using CoreLibrary.Models.Trading;

namespace CoreLibrary.Models.Reporters.Base
{
    public interface IArbitrageReporter
    {
        public Task StartReporting();
        public Task SendSignal(ArbitrageChain chain);
        public Task SendOpenedTrade(ArbitrageTrade trade);
        public Task SendClosedTrade(ArbitrageTrade trade);
        public Task RemoveSignal(ArbitrageChain chain);
    }
}
