﻿namespace Basket.API.EventHandlers;

public class ProductPriceChangedIntegrationEventHandler(BasketService service)
    : IConsumer<ProductPriceChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
    {
        // find products on basket and update price
        await service.UpdateBasketItemProductPrices
            (context.Message.ProductId, context.Message.Price);
    }
}
