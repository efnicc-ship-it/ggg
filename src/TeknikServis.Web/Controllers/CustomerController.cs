using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknikServis.Application.Features.Customer.Commands.CreateCustomer;
using TeknikServis.Application.Features.Customer.Commands.UpdateCustomer;
using TeknikServis.Application.Features.Customer.Queries.GetCustomerDetail;
using TeknikServis.Application.Features.Customer.Queries.GetCustomers;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Index(string? q, bool? blacklisted, int page = 1)
    {
        var result = await _mediator.Send(new GetCustomersQuery(
            Page: page, PageSize: 25,
            Search: q, IsBlacklisted: blacklisted));

        ViewBag.Q = q;
        ViewBag.Blacklisted = blacklisted;
        return View(result);
    }

    [HttpGet]
    public IActionResult Create(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerCommand command, string? returnUrl)
    {
        try
        {
            var id = await _mediator.Send(command);
            TempData["Success"] = "Müşteri başarıyla oluşturuldu.";

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl + $"?customerId={id}");

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.ReturnUrl = returnUrl;
            return View(command);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var dto = await _mediator.Send(new GetCustomerDetailQuery(id));
        if (dto == null) return NotFound();
        ViewData["Title"] = dto.FullName;
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _mediator.Send(new GetCustomerDetailQuery(id));
        if (dto == null) return NotFound();
        ViewData["Title"] = $"{dto.FullName} — Düzenle";

        var cmd = new UpdateCustomerCommand(
            dto.Id, dto.FirstName, dto.LastName, dto.Phone, dto.Phone2,
            dto.Email, dto.Address, dto.City, dto.CommunicationPreference,
            dto.SmsOptOut, dto.WhatsAppOptOut, dto.EmailOptOut,
            dto.IsBlacklisted, dto.BlacklistReason);
        return View(cmd);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateCustomerCommand command)
    {
        try
        {
            await _mediator.Send(command);
            TempData["Success"] = "Müşteri bilgileri güncellendi.";
            return RedirectToAction(nameof(Detail), new { id = command.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(command);
        }
    }

    // GET: /musteriler/{id}/popup — AJAX quick-add for service/purchase forms
    [HttpGet]
    public IActionResult CreatePopup()
    {
        return PartialView("_CreateCustomerPartial", new CreateCustomerCommand(
            string.Empty, string.Empty, string.Empty, null, null, null, null,
            CommunicationPreference.WhatsApp, null, false, false, false));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePopup(CreateCustomerCommand command)
    {
        try
        {
            var id = await _mediator.Send(command);
            var dto = await _mediator.Send(new GetCustomerDetailQuery(id));
            return Json(new { success = true, id, fullName = dto!.FullName, phone = dto.Phone });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
}
