using UnityEngine;

namespace GameTemplate.Runtime.WGUI
{
    public static class NotficationHelper
    {
        public static void Show(string message)
        {
            UIManager.Instance.Show<UINotification>(
                ui => ui.SetMessage(message)
            );
        }
    }
}
