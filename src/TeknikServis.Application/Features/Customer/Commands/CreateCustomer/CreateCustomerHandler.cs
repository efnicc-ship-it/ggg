using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Exceptions;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Application.Features.Customer.Commands.CreateCustomer;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IEncryptionService _encryption;

    public CreateCustomerHandler(IApplicationDbContext db, IEncryptionService encryption)
    {
        _db = db;
        _encryption = encryption;
    }

    public async Task<int> Handle(CreateCustomerCommand req, CancellationToken ct)
    {
        // Telefon benzersizliği kontrolü (tenant scope — GlobalQueryFilter devrede)
        var exists = await _db.Customers.AnyAsync(c => c.Phone == req.Phone, ct);
        if (exists)
            throw new InvalidOperationException("Bu telefon numarası zaten kayıtlı.");

        var customer = new Domain.Entities.Customer.Customer
        {
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            Phone = req.Phone.Trim(),
            Phone2 = req.Phone2?.Trim(),
            Email = req.Email?.Trim().ToLowerInvariant(),
            Address = req.Address,
            City = req.City,
            CommunicationPreference = req.CommunicationPreference,
            SmsOptOut = req.SmsOptOut,
            WhatsAppOptOut = req.WhatsAppOptOut,
            EmailOptOut = req.EmailOptOut
        };

        // KVKK — TC Kimlik şifrele
        if (!string.IsNullOrWhiteSpace(req.NationalId))
            customer.NationalIdEncrypted = _encryption.Encrypt(req.NationalId.Trim());

        // Kara liste kontrolü (aynı telefon daha önce kara listede miydi?)
        var blacklisted = await _db.BlacklistedCustomers
            .AnyAsync(b => b.Customer != null && b.Customer.Phone == req.Phone && b.IsActive, ct);
        if (blacklisted)
        {
            customer.IsBlacklisted = true;
            customer.BlacklistReason = "Sistem kara listesinden otomatik eklendi.";
        }

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        return customer.Id;
    }
}
