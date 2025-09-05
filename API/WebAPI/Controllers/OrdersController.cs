using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var orders = await orderService.GetPagedAsync(pageNumber, pageSize, status, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderVm>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderVm>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await orderService.CreateAsync(request, cancellationToken);
     
        return Ok();
    }

    [HttpPost("{id}/pay")]
    public async Task<ActionResult<OrderVm>> PayOrder(Guid id, CancellationToken cancellationToken)
    {
        await orderService.PayOrderAsync(id, cancellationToken);
        return Ok();
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderVm>> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        await orderService.CancelOrderAsync(id, cancellationToken);
        return Ok();
    }
}