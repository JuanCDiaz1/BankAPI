using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;
using TestBankAPI.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
namespace BankAPI.Controllers;


[Authorize(Policy = "Client")]
[ApiController]
[Route("api/[controller]")]
public class BankTransactionController : ControllerBase{

 private readonly BankTransactionService bankTransactionService;
 private readonly AccountService accountService;
 private readonly TransactionTypeService transactionTypeService;


  public BankTransactionController(BankTransactionService bankTransactionService,AccountService accountService, TransactionTypeService transactionTypeService) { 
    {
        this.bankTransactionService = bankTransactionService; this.accountService = accountService;this.transactionTypeService= transactionTypeService;
    }}


[HttpGet("getaccounts/{id}")] //obtener cuentas a partir de id de cliente
    public async Task<IEnumerable<Account>> GetAccounts(int id){
        var accounts = await bankTransactionService.GetAccounts(id);
    return accounts; 
  }

    
[HttpGet("gettransactions/{id}")]//obtener transacciones a partir de id de cuenta
    public async Task<IEnumerable<BankTransaction>> GetBankTransactions(int id){
        var transactions = await bankTransactionService.GetTransactions(id);
        return transactions;
    }

[HttpPost("withdrawal/cash")]//retiro en efectivo
    public async Task<IActionResult> CashWhithdrawal(BankTransactionDtoIn transactionDtoIn){           
        string ValRel = await ValidateAccount(transactionDtoIn);
       if(!ValRel.Equals("Cuenta asociada encontrada.")){return BadRequest(new {message = ValRel});}
       if(transactionDtoIn.TransactionType!=2){return BadRequest(new{message = "La opción 2 es retiro en efectivo, verifique que haya introducido los datos correctamente."});}
       if(transactionDtoIn.Amount <= 0 ){return BadRequest(new{ message= "No se puede retirar 0 o menos."});}
       if(transactionDtoIn.ExternalAccount is not null){return BadRequest(new {message = "Al ser un retiro en efectivo, no se requiere una cuenta externa"});}
       var account = await accountService.GetById(transactionDtoIn.AccountId);
       decimal TransactionResult = (decimal)(account.Balance - transactionDtoIn.Amount);
            if(TransactionResult<0){return BadRequest(new {message = $"No hay fondos suficientes para realizar el retiro.\nSaldo: {account.Balance} Reitro: {transactionDtoIn.Amount}"});}
            else 
            {  AccountDtoIn UpdatedAccount = new AccountDtoIn();
                UpdatedAccount.AccountType = account.AccountType; UpdatedAccount.Balance = TransactionResult;UpdatedAccount.ClientId = account.ClientId;UpdatedAccount.Id = account.Id;
                await accountService.Update(UpdatedAccount);
                var newTransaction = await bankTransactionService.AddTransaction(transactionDtoIn);
                return CreatedAtAction(nameof(GetAccounts), new{ id = newTransaction.Id}, newTransaction); }   
    }


[HttpPost("withdrawal/transfer")]// retiro via transferencia (con cuentas asociadas)
    public async Task<IActionResult>  TransferWithdrawal(BankTransactionDtoIn transactionDtoIn){
       


        string ValRel = await ValidateAccount(transactionDtoIn);
        if(!ValRel.Equals("Cuenta asociada encontrada.")){return BadRequest(new {message = ValRel});}
        if(transactionDtoIn.TransactionType!=4){return BadRequest(new { message = "La opción 4 es retiro via transferencia, verifique que haya introducido los datos correctamente." });}
        if(transactionDtoIn.Amount <= 0 ){return BadRequest(new{ message= "No se puede retirar 0 o menos."});}
        if(transactionDtoIn.ExternalAccount is null){return BadRequest(new {message = "Es necesaria otra cuenta para realizar la transferencia."});}
        var account = await accountService.GetById(transactionDtoIn.AccountId);
        var account2 = await accountService.GetById((int)transactionDtoIn.ExternalAccount);
        

        decimal TransactionResult = (decimal)(account.Balance - transactionDtoIn.Amount);
        decimal TransactionResult2=  (decimal)(account2.Balance + transactionDtoIn.Amount);
        if(TransactionResult < 0){return BadRequest(new { message = $"No hay fondos suficientes para realizar el retiro.\nSaldo: {account.Balance} Reitro: {transactionDtoIn.Amount}"});}
        else
        {
          
          AccountDtoIn UpdatedExternalAccount = new AccountDtoIn();
           UpdatedExternalAccount.AccountType = account2.AccountType; UpdatedExternalAccount.Balance = TransactionResult2; UpdatedExternalAccount.ClientId = account2.ClientId; UpdatedExternalAccount.Id = account2.Id;
          await accountService.Update(UpdatedExternalAccount);
    
            
          AccountDtoIn UpdatedAccount = new AccountDtoIn();
          UpdatedAccount.AccountType = account.AccountType; UpdatedAccount.Balance = TransactionResult; UpdatedAccount.ClientId = account.ClientId; UpdatedAccount.Id = account.Id;
          await accountService.Update(UpdatedAccount);
          var newTransaction = await bankTransactionService.AddTransaction(transactionDtoIn);
          return CreatedAtAction(nameof(GetAccounts), new{ id = newTransaction.Id}, newTransaction);}
    }


  [HttpPost("deposit")]// deposito (exclusivo en efectivo)
    public async Task<IActionResult> Deposit(BankTransactionDtoIn transactionDtoIn){
       string ValRel = await ValidateAccount(transactionDtoIn);
       if(!ValRel.Equals("Cuenta asociada encontrada.")){return BadRequest(new {message = ValRel});}
       if(transactionDtoIn.TransactionType!=1){return BadRequest(new{message = "La opción 1 es depósito en efectivo, verifique que haya introducido los datos correctamente."});}
       if(transactionDtoIn.Amount <= 0 ){return BadRequest(new{ message= "No se puede depositar 0 o menos."});}
       if(transactionDtoIn.ExternalAccount is not null){return BadRequest(new {message = "Al ser un depósito en efectivo, no se requiere una cuenta externa."});}
       var account = await accountService.GetById(transactionDtoIn.AccountId);
       decimal TransactionResult = (decimal)(account.Balance + transactionDtoIn.Amount);
       if(TransactionResult < 0){return BadRequest(new { message = $"Inlcuso al hacer un depósito, el balance es negativo, solicite ayuda al centro de atención."});}
       else{
                AccountDtoIn UpdatedAccount = new AccountDtoIn();
                UpdatedAccount.AccountType = account.AccountType; UpdatedAccount.Balance = TransactionResult; UpdatedAccount.ClientId = account.ClientId; UpdatedAccount.Id = account.Id;
                await accountService.Update(UpdatedAccount);
                var newTransaction = await bankTransactionService.AddTransaction(transactionDtoIn);
                return CreatedAtAction(nameof(GetAccounts), new{ id = newTransaction.Id}, newTransaction);}
    }

  [HttpDelete("delete/{accountId}")]//borrar cuenta de un cliente (borrar la cuenta, no al cliente)
    public async Task<IActionResult> Delete(int accountId){
        var accountToDelete = await accountService.GetById(accountId);
        if(accountToDelete is not null){
            if(accountToDelete.Balance != 0){ return BadRequest(new {message = "No es posible eliminar la cuenta ya que aún hay dinero dentro de ella."});}
            await accountService.Delete(accountId);
            return Ok();}
        else{
            return NotFound(new {message = $"El la cuenta con el ID ({accountId}) no se ha encontrado"});}
    }
 public async Task<string> ValidateAccount(BankTransactionDtoIn bankTransactionDtoIn){
        string ValRel = "Cuenta asociada encontrada.";
        var account = await accountService.GetById(bankTransactionDtoIn.AccountId);
        if(account is null){
            return $"No hay una cuenta con el ID ingresado ({bankTransactionDtoIn.AccountId}).";} 
        var accountType = await transactionTypeService.GetById(bankTransactionDtoIn.TransactionType);
        if(accountType is null){
            return $"El tipo de transaccion({bankTransactionDtoIn.TransactionType}) no es válida.";}
        return ValRel;
    }
    
 }//fin de la clase