﻿using DTOS;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Service.Interface;
using System.Text.Json;

namespace Service.Implement
{
    public class PayOSService : IPayOSService
    {
        public async Task<string> CreatePaymentLink(int quantity, string urlCancel, string urlReturn)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var client = config["PayOS:ClientID"];
            var apiKey = config["PayOS:APIKey"];
            var checkSumKey = config["PayOS:CheckSumKey"];

            PayOS payOS = new PayOS(client, apiKey, checkSumKey);

            ItemData item = new ItemData("Cây", quantity, quantity * 2000);
            List<ItemData> items = new List<ItemData>();
            items.Add(item);

            Random rand = new Random();
            string orderID = "";
            PaymentLinkInformation paymentLinkInformation = null;
            try
            {
                do
                {
                    orderID = "";
                    for (int i = 0; i < 6; i++)
                    {
                        orderID += rand.Next(0, 10);
                    }

                    paymentLinkInformation = await payOS.getPaymentLinkInformation(int.Parse(orderID));
                } while (paymentLinkInformation != null);
                return "";
            }
            catch (Exception ex) {
                PaymentData paymentData = new PaymentData(int.Parse(orderID), quantity * 2000, "Thanh toan don hang",
            items, urlCancel, urlReturn);

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);
                return createPayment.checkoutUrl;
            }
        }

        public async Task<TransactionReturn> HandleCodeAfterPaymentQR(int code)
        {
            try
            {
                IConfigurationRoot config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", true, true)
               .Build();

                var client = config["PayOS:ClientID"];
                var apiKey = config["PayOS:APIKey"];
                var checkSumKey = config["PayOS:CheckSumKey"];

                PayOS payOS = new PayOS(client, apiKey, checkSumKey);
                PaymentLinkInformation paymentLinkInformation = await payOS.getPaymentLinkInformation(code);
                var inf = paymentLinkInformation.transactions.FirstOrDefault();
                var bankAccounts = GetBankAccount();
                var bank = bankAccounts.Result.FirstOrDefault(x => x.bin == inf.counterAccountBankId);
                var transaction = new TransactionReturn()
                {
                    AccountName = inf.counterAccountName,
                    AccountNumber = inf.counterAccountNumber,
                    Amount = inf.amount,
                    BankCode = bank.code,
                    BankName = bank.shortName,
                    Reference = inf.reference,
                    Description = inf.description,
                    TransactionDate = DateTime.Parse(inf.transactionDateTime)
                };
                return transaction;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<IEnumerable<BankAccount>> GetBankAccount()
        {
            var bankData = await File.ReadAllTextAsync("BankAccount.json");

            var banks = JsonSerializer.Deserialize<List<BankAccount>>(bankData);
            return banks;
        }
    }
}
