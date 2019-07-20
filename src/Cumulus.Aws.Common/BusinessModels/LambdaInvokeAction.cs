using Cumulus.Aws.Common.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulus.Aws.Common.BusinessModels
{
    public class LambdaInvokeAction
    {
        [JsonProperty(CumulusConstants.JsonField.Action)]
        public string Action { get; set; }
    }
}
