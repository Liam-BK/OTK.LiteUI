namespace OTK.LiteUI.UI.Interfaces
{
    /// <summary>
    /// Defines a component that can navigate to another page when activated.
    /// </summary>
    public interface ILinkable
    {
        /// <summary>
        /// The Identifier of the Page to navigate to.
        /// </summary>
        public string PageLink { get; set; }

        /// <summary>
        /// Whether the parent page should be hidden when the link is activated.
        /// </summary>
        public bool HideParentPageOnClick { get; set; }
    }
}