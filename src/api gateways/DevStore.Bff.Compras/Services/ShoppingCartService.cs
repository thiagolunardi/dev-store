using DevStore.Bff.Compras.Extensions;
using DevStore.Bff.Compras.Models;
using DevStore.Core.Communication;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevStore.Bff.Compras.Services
{
    public interface IShoppingCartService
    {
        Task<ShoppingCartDto> GetShoppingCart();
        Task<ResponseResult> AddItem(ShoppingCartItemDto produto);
        Task<ResponseResult> UpdateItem(Guid produtoId, ShoppingCartItemDto carrinho);
        Task<ResponseResult> RemoveItem(Guid produtoId);
        Task<ResponseResult> ApplyVoucher(VoucherDTO voucher);
    }

    public class ShoppingCartService : Service, IShoppingCartService
    {
        private readonly HttpClient _httpClient;

        public ShoppingCartService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.ShoppingCartUrl);
        }

        public async Task<ShoppingCartDto> GetShoppingCart()
        {
            var response = await _httpClient.GetAsync("/shopping-cart");

            ManageHttpResponse(response);

            return await DeserializeResponse<ShoppingCartDto>(response);
        }

        public async Task<ResponseResult> AddItem(ShoppingCartItemDto produto)
        {
            var itemContent = GetContent(produto);

            var response = await _httpClient.PostAsync("/shopping-cart", itemContent);

            if (!ManageHttpResponse(response)) return await DeserializeResponse<ResponseResult>(response);

            return Ok();
        }

        public async Task<ResponseResult> UpdateItem(Guid produtoId, ShoppingCartItemDto carrinho)
        {
            var itemContent = GetContent(carrinho);

            var Response = await _httpClient.PutAsync($"/shopping-cart/{carrinho.ProductId}", itemContent);

            if (!ManageHttpResponse(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return Ok();
        }

        public async Task<ResponseResult> RemoveItem(Guid produtoId)
        {
            var Response = await _httpClient.DeleteAsync($"/shopping-cart/{produtoId}");

            if (!ManageHttpResponse(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return Ok();
        }

        public async Task<ResponseResult> ApplyVoucher(VoucherDTO voucher)
        {
            var itemContent = GetContent(voucher);

            var Response = await _httpClient.PostAsync("/shopping-cart/apply-voucher/", itemContent);

            if (!ManageHttpResponse(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return Ok();
        }
    }
}