using DevStore.Core.Communication;
using DevStore.WebApp.MVC.Extensions;
using DevStore.WebApp.MVC.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevStore.WebApp.MVC.Services
{
    public interface IComprasBffService
    {
        // Carrinho
        Task<ShoppingCartViewModel> GetShoppingCart();
        Task<int> ObterQuantidadeCarrinho();
        Task<ResponseResult> AddShoppingCartItem(ShoppingCartItemViewModel carrinho);
        Task<ResponseResult> AtualizarItemCarrinho(Guid produtoId, ShoppingCartItemViewModel carrinho);
        Task<ResponseResult> RemoverItemCarrinho(Guid produtoId);
        Task<ResponseResult> AplicarVoucherCarrinho(string voucher);

        // Pedido
        Task<ResponseResult> FinalizarPedido(TransactionViewModel transaction);
        Task<OrderViewModel> ObterUltimoPedido();
        Task<IEnumerable<OrderViewModel>> ObterListaPorClienteId();
        TransactionViewModel MapearParaPedido(ShoppingCartViewModel shoppingCart, AddressViewModel address);
    }

    public class ComprasBffService : Service, IComprasBffService
    {
        private readonly HttpClient _httpClient;

        public ComprasBffService(HttpClient httpClient, IOptions<AppSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.ComprasBffUrl);
        }

        #region Carrinho

        public async Task<ShoppingCartViewModel> GetShoppingCart()
        {
            var response = await _httpClient.GetAsync("/orders/shopping-cart/");

            ManageResponseErrors(response);

            return await DeserializeResponse<ShoppingCartViewModel>(response);
        }
        public async Task<int> ObterQuantidadeCarrinho()
        {
            var Response = await _httpClient.GetAsync("/orders/shopping-cart/quantity/");

            ManageResponseErrors(Response);

            return await DeserializeResponse<int>(Response);
        }
        public async Task<ResponseResult> AddShoppingCartItem(ShoppingCartItemViewModel carrinho)
        {
            var itemContent = GetContent(carrinho);

            var Response = await _httpClient.PostAsync("/orders/shopping-cart/items/", itemContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }
        public async Task<ResponseResult> AtualizarItemCarrinho(Guid produtoId, ShoppingCartItemViewModel shoppingCartItem)
        {
            var itemContent = GetContent(shoppingCartItem);

            var Response = await _httpClient.PutAsync($"/orders/shopping-cart/items/{produtoId}", itemContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }
        public async Task<ResponseResult> RemoverItemCarrinho(Guid produtoId)
        {
            var Response = await _httpClient.DeleteAsync($"/orders/shopping-cart/items/{produtoId}");

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }
        public async Task<ResponseResult> AplicarVoucherCarrinho(string voucher)
        {
            var itemContent = GetContent(voucher);

            var Response = await _httpClient.PostAsync("/orders/shopping-cart/aplicar-voucher/", itemContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }

        #endregion

        #region Pedido

        public async Task<ResponseResult> FinalizarPedido(TransactionViewModel transaction)
        {
            var pedidoContent = GetContent(transaction);

            var Response = await _httpClient.PostAsync("/orders", pedidoContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }

        public async Task<OrderViewModel> ObterUltimoPedido()
        {
            var Response = await _httpClient.GetAsync("/orders/last");

            ManageResponseErrors(Response);

            return await DeserializeResponse<OrderViewModel>(Response);
        }

        public async Task<IEnumerable<OrderViewModel>> ObterListaPorClienteId()
        {
            var Response = await _httpClient.GetAsync("/orders/clients");

            ManageResponseErrors(Response);

            return await DeserializeResponse<IEnumerable<OrderViewModel>>(Response);
        }

        public TransactionViewModel MapearParaPedido(ShoppingCartViewModel shoppingCart, AddressViewModel address)
        {
            var pedido = new TransactionViewModel
            {
                Amount = shoppingCart.Total,
                Items = shoppingCart.Items,
                Discount = shoppingCart.Discount,
                HasVoucher = shoppingCart.HasVoucher,
                Voucher = shoppingCart.Voucher?.Codigo
            };

            if (address != null)
            {
                pedido.Address = new AddressViewModel
                {
                    StreetAddress = address.StreetAddress,
                    BuildingNumber = address.BuildingNumber,
                    Neighborhood = address.Neighborhood,
                    ZipCode = address.ZipCode,
                    SecondaryAddress = address.SecondaryAddress,
                    City = address.City,
                    State = address.State
                };
            }

            return pedido;
        }

        #endregion
    }
}