using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Bot.Services
{
    public static class TransactionService
    {
        /*public static Task DoTransactionAsync(ICommandContext Context, TransactionStruct transaction)
        {
            
        }*/

        public struct TransactionStruct
        {
            public bool Removal;
            public ulong Amount;
            public ulong SenderId;
            public ulong ReceiverId;
        }
    }
}
