using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Dtos;
using System.Linq;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _itemsRepository;

    public ItemsController(IRepository<InventoryItem> itemsRepository)
    {
        _itemsRepository = itemsRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if(userId ==  Guid.Empty)
        {
            return BadRequest(nameof(userId));
        }

        var items = (await _itemsRepository.GetAllAsync(item => item.UserId == userId))
            .Select(item => item.AsDto());

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
    {
        var inventoryItem = await _itemsRepository.GetAsync(
            item => item.UserId == grantItemsDto.UserId && item.Id == grantItemsDto.CatalogItemId);

        if(inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = grantItemsDto.CatalogItemId,
                UserId = grantItemsDto.UserId,
                Quantity = grantItemsDto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await _itemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsDto.Quantity;
            await _itemsRepository.UpdateAsync(inventoryItem);
        }

        return Ok();
    }
}
