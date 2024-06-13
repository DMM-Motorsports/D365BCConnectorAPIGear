using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D365BCConnectorAPIGear
{
    public class UnitOfMeasureExtention : BaseObject
    {
        [JsonProperty("itemNo")] public string? No { get; set; }

        [JsonProperty("length")] public decimal Length { get; set; }

        [JsonProperty("width")] public decimal Width { get; set; }
        [JsonProperty("height")] public decimal Height { get; set; }
      
        [JsonProperty("weight")] public decimal Weight { get; set; }

        

         [JsonProperty("qtyPerUnitOfMeasure")] public decimal QtyPerUnitOfMeasure { get; set; }
        [JsonProperty("code")] public string? Code { get; set; }

        [JsonProperty("shipsAlone")] public bool ShipsAlone { get; set; }

        [JsonProperty("systemId")] public Guid? SystemId { get; set; }



    }
}
