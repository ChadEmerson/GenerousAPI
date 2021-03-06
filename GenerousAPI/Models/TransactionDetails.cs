﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GenerousAPI.Models
{
    public class TransactionDetails
    {
        public decimal Amount { get; set; }

        public string PaymentProfileTokenId { get; set; }

        public string BankAccountForFundsTokenId { get; set; }

        public string TransactionReferenceNumber { get; set; }

        public bool IsTest { get; set; }

        public int OrganisationId { get; set; }

        //public List<EventTicketsWithPrice> EventTickets { get; set; }

        public byte PaymentMethodId { get; set; }

        public int ProcessRetryCounter { get; set; }

        public int NumberOfEventTickets { get; set; }

    }


    //public class EventTicketsWithPrice
    //{
    //    public int eventTicket { get; set; }

    //    public decimal TicketPriceAmount { get; set; }
    //}

}