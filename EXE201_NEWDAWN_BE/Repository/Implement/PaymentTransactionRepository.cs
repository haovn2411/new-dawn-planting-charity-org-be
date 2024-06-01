﻿using AutoMapper;
using BussinessObjects.Models;
using DAO;
using DTOS.Payment;
using Repository.Interface;

namespace Repository.Implement
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly IMapper mapper;

        public PaymentTransactionRepository(IMapper mapper) 
        {
            this.mapper = mapper;
        }

        public async Task<int> CreatePaymentTransaction(PaymentCreate paymentTransaction)
        {
            var payment = mapper.Map<PaymentTransaction>(paymentTransaction);
            payment.Status = 0;
            payment.DateCreate = DateTime.Now;
            await PaymentTransactionDAO.Instance.CreateAsync(payment);
            return payment.TransactionID;
        }

        public async Task<IEnumerable<PaymentAdminView>> GetAllTransactions()
        {
            var payments = await PaymentTransactionDAO.Instance.GetAllPayments();
            return mapper.Map<IEnumerable<PaymentAdminView>>(payments);
        }

        public async Task<double> GetTotalProfit()
        {
            return await PaymentTransactionDAO.Instance.GetTotalProfit();
        }

        public async Task<IEnumerable<Top4Transaction>> Top4Transactions()
        {
            var payments = await PaymentTransactionDAO.Instance.Get4PaymentTransaction();
            var transactions = payments.Select(x => new Top4Transaction
            {
                TransactionID = x.TransactionID,
                Username = x.UserInformation.Username,
                Quantity = PaymentTransactionDetailDAO.Instance.GetQuantityTransaction(x.TransactionID),
                DateCreate = x.DateCreate
            }).ToList();
            return transactions;
        }
    }
}
