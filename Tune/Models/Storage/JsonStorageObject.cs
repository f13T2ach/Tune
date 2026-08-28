using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tune.Models.Storage
{
    /// <summary>
    /// We need a class whose child classes are able to convert to JObject.
    /// However, the `FromJObjectToJsonStorageObject` need to be implied by the class own.
    /// </summary>
    public abstract class JsonStorageObject
    {
        public abstract JObject? ToJObject();
    }
}
