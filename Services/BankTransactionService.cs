using BankAPI.Data;
using BankAPI.Data.BankModels;
using Microsoft.EntityFrameworkCore;
using TestBankAPI.Data.DTOs;
namespace BankAPI.Services;

public class BankTransactionService{

private readonly BankContext _context;

 public BankTransactionService(BankContext context){
    _context=context;
   }



  public async Task<IEnumerable<Account>> GetAccounts(int clientId)
    {
        List<Account> account = new List<Account>();
        account  = await _context.Accounts.ToListAsync();

        return account.Where(cuenta => cuenta.ClientId == clientId);   
    }

    
    public async Task<IEnumerable<BankTransaction>> GetTransactions(int accountId)
    {
        List<BankTransaction> transaction = new List<BankTransaction>();
        transaction = await _context.BankTransactions.ToListAsync();

        return transaction.Where(transaccion => transaccion.AccountId == accountId);
    }

 public async Task<BankTransaction> AddTransaction(BankTransactionDtoIn newTransactionDto)
    {
        var newTransaction = new BankTransaction();
        newTransaction.AccountId = newTransactionDto.AccountId;
        newTransaction.TransactionType = newTransactionDto.TransactionType;
        newTransaction.Amount = newTransactionDto.Amount;
        newTransaction.ExternalAccount = newTransactionDto.ExternalAccount;
        _context.BankTransactions.Add(newTransaction);
        await _context.SaveChangesAsync();
        return newTransaction;
    }
    

}// fin de la clase