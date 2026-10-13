namespace OTK.LiteUI.UI.Interfaces
{
    public interface ILinkable
    {
        public string PageLink { get; set; }
        public bool HideParentPageOnClick { get; set; }
    }
}