using System.Windows;


namespace SMT.SAAS.Platform
{
    public interface IMainPanel
    {
        void Navigation(UIElement Content, string Titel);

        void SetTitel(string titel);

        UIElement DefaultContent { get; set; }
    }
}
