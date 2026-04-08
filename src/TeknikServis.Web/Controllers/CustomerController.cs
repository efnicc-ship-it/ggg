using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknikServis.Application.Features.Customer.Commands.CreateCustomer;
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
}
