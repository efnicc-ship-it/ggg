using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Exceptions;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Application.Features.Customer.Commands.UpdateCustomer;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateCustomerHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateCustomerCommand req, CancellationToken ct)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == req.Id, ct)
            ?? throw new NotFoundException("Müşteri bulunamadı.");

        // Telefon benzersizlik kontrolü (kendi kaydı hariç)
        var phoneConflict = await _db.Customers
            .AnyAsync(c => c.Phone == req.Phone.Trim() && c.Id != req.Id, ct);
        if (phoneConflict)
            throw new InvalidOperationException("Bu telefon numarası başka bir müşteriye ait.");

        customer.FirstName = req.FirstName.Trim();
        customer.LastName = req.LastName.Trim();
        customer.Phone = req.Phone.Trim();
        customer.Phone2 = req.Phone2?.Trim();
        customer.Email = req.Email?.Trim().ToLowerInvariant();
        customer.Address = req.Address;
        customer.City = req.City;
        customer.CommunicationPreference = req.CommunicationPreference;
        customer.SmsOptOut = req.SmsOptOut;
        customer.WhatsAppOptOut = req.WhatsAppOptOut;
        customer.EmailOptOut = req.EmailOptOut;
        customer.IsBlacklisted = req.IsBlacklisted;
        customer.BlacklistReason = req.IsBlacklisted ? req.BlacklistReason : null;

        await _db.SaveChangesAsync(ct);
    }
}
