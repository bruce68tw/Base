using BaseEther.Models;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Account = Nethereum.Web3.Accounts.Account;

namespace BaseEther.Services
{
#pragma warning disable IDE1006 // 命名樣式
    public static class _Ether
#pragma warning restore IDE1006 // 命名樣式
    {

        /// <summary>
        /// log info only when _Fun.LogInfo flag is true !!
        /// </summary>
        /// <param name="msg">log msg</param>
        public static Contract? GetContract(string nodeUrl, string address, string abi)
        {
            var web3 = new Web3(nodeUrl);
            var contract = web3.Eth.GetContract(abi, address);
            return contract;
        }

        /// <summary>
        /// get contract for transaction
        /// </summary>
        /// <param name="nodeUrl"></param>
        /// <param name="address"></param>
        /// <param name="abi"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static Contract? GetTxContract(string nodeUrl, string address, string abi, string privateKey)
        {
            var account = new Account(privateKey);
            var web3 = new Web3(account, nodeUrl);
            return web3.Eth.GetContract(abi, address);
        }

        public static async Task<string> GetLogA(string nodeUrl, string txHash)
        {
            var web3 = new Web3(nodeUrl);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            if (receipt == null || receipt.Logs == null || receipt.Logs.Count == 0)
                return "";

            var log = receipt.Logs[0].ToObject<FilterLog>();    //jToken -> FilterLog
            var eventData = log.DecodeEvent<EventLogDto>();
            return eventData.Event.log;
        }

    }//class
}