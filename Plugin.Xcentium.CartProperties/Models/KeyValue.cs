namespace Plugin.Xcentium.CartProperties.Models
{
    /// <summary>
    /// To mimic c#'s key value pair since OData does not like the native KeyValue pair
    /// </summary>
    public class KeyValue
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public object Value { get; set; }
    }
}
