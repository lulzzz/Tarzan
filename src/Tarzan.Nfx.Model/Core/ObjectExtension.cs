namespace Tarzan.Nfx.Model.Core
{
    public abstract class ObjectExtension
    {
        /// <summary>
        /// The type property identifies the type of Object Extension.
        /// </summary>
        public abstract string Type { get; }
        /// <summary>
        /// References Id of the based object extended by this extension.
        /// </summary>
        public string RefId { get; set; }
    }
}