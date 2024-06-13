using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D365BCConnectorAPIGear
{
    public class ItemExtention : BaseObject
    {

        [JsonProperty("no")] public string? No { get; set; }

        [JsonProperty("systemId")] public Guid? SystemId { get; set; }

        [JsonProperty("vendorItemNo")] public string? VendorItemNo { get; set; }

        [JsonProperty("vendorNo")] public string? VendorNo { get; set; }

        [JsonProperty("blocked")] public bool Blocked { get; set; }

        [JsonProperty("description")] public string? Description { get; set; }

        [JsonProperty("gtin")] public string? Gtin { get; set; }

        [JsonProperty("inventoryPostingGroup")] public string? InventoryPostingGroup { get; set; }

        [JsonProperty("map")] public decimal Map { get; set; }

        [JsonProperty("msrp")] public decimal Msrp { get; set; }

        [JsonProperty("unitPrice")] public decimal UnitPrice { get; set; }

        [JsonProperty("netWeight")] public decimal NetWeight { get; set; }

        [JsonProperty("mpn")] public string? Mpn { get; set; }


        [JsonProperty("unitCost")] public decimal UnitCost { get; set; }

        [JsonProperty("baseUnitOfMeasure")] public string? BaseUnitOfMeasure { get; set; }

        [JsonProperty("type")] public string? Type { get; set; }

        [JsonProperty("genProdPostingGroup")] public string? GenProdPostingGroup { get; set; }

        [JsonProperty("lifeCyleStatus")] public string? LifeCyleStatus { get; set; }
       
        [JsonProperty("launchDate")]  public DateTime LaunchDate { get; set; }

    }
}
