using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D365BCConnectorAPIGear.Entities;
using D365BCConnectorAPIGear;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Serilog;
using D365BCConnectorAPIGear.StatusCodes;

namespace D365BCConnectorAPIGear
{
    public class ItemService : BusinessCentralService, IItemService
    {
        public ItemService(string clientId, string clientSecret, string tenantId, string companyId) : base(clientId, clientSecret, tenantId, companyId)
        {
        }



        public Task<ApiResponse<Item>?> GetAsync(string id)
        {
            var url = $"https://api.businesscentral.dynamics.com/v2.0/{_tenantId}/Production/api/v2.0/companies({_companyId})/items/{id}";
            return GetDataAsync<Item>(url);
        }

        public Task<ApiResponse<Item>?> GetByNumberAsync(string number)
        {
            var url = $"https://api.businesscentral.dynamics.com/v2.0/{_tenantId}/Production/api/v2.0/companies({_companyId})/items?$filter=number eq '{number}'";
            return GetDataAsync<Item>(url);
        }



        public async Task<bool> AddAsync(Item item)
        {
            var url = $"https://api.businesscentral.dynamics.com/v2.0/{_tenantId}/Production/api/v2.0/companies({_companyId})/items";

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

            void AddToDictionaryIfNotNull(string key, object value)
            {
                if (value != null && !(value is Guid && (Guid)value == Guid.Empty))
                {
                    keyValuePairs.Add(key, value);
                }
            }

            AddToDictionaryIfNotNull("number", item.No);
            AddToDictionaryIfNotNull("displayName", item.DisplayName);
            AddToDictionaryIfNotNull("type", item.Type);
            AddToDictionaryIfNotNull("itemCategoryId", item.ItemCategoryId);
            AddToDictionaryIfNotNull("blocked", item.Blocked);
            AddToDictionaryIfNotNull("gtin", item.Gtin);
            AddToDictionaryIfNotNull("unitPrice", item.UnitPrice);
            AddToDictionaryIfNotNull("priceIncludesTax", item.PriceIncludesTax);
            AddToDictionaryIfNotNull("unitCost", item.UnitCost);
            AddToDictionaryIfNotNull("taxGroupId", item.TaxGroupId);
            AddToDictionaryIfNotNull("taxGroupCode", item.TaxGroupCode);
            AddToDictionaryIfNotNull("baseUnitOfMeasureId", item.BaseUnitOfMeasureId);
            AddToDictionaryIfNotNull("baseUnitOfMeasureCode", item.BaseUnitOfMeasureCode);
            AddToDictionaryIfNotNull("inventoryPostingGroupCode", item.InventoryPostingGroupCode);
            AddToDictionaryIfNotNull("generalProductPostingGroupCode", item.GeneralProductPostingGroupCode);

            return await AddAsync(url, keyValuePairs);
        }


        public Task<bool> AddAsync( Dictionary<string, object> keyValuePairs )
        {
            var url = $"https://api.businesscentral.dynamics.com/v2.0/{_tenantId}/Production/api/v2.0/companies({_companyId})/items";
            return AddAsync(url, keyValuePairs );
        }



        public Task<bool> UpdateAsync(Item item)
        {
            var url = $"https://api.businesscentral.dynamics.com/v2.0/{_tenantId}/Production/api/v2.0/companies({_companyId})/items({item.Id})";
            return UpdateDataAsync(url, item);
        }



        public Task<bool> UpdateByIdAsync(string id, Dictionary<string, object> keyValuePairs, string eTag)
        {
            var url = $"https://api.businesscentral.dynamics.com/v2.0/{_tenantId}/Production/api/v2.0/companies({_companyId})/items({id})";
            return UpdateDataAsync(url, keyValuePairs, eTag);
        }

    }
}
