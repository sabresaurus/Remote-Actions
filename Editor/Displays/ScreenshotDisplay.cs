using JetBrains.Annotations;
using Sabresaurus.RemoteActions.Requests;
using UnityEditor;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    [UsedImplicitly]
    public class ScreenshotDisplay : CustomDisplay
    {
        private Texture2D lastTexture = null;

        public override void OnResponseReceived(BaseResponse response)
        {
            if (response is ScreenshotResponse screenshotResponse)
            {
                lastTexture = new Texture2D(2, 2);
                lastTexture.LoadImage(screenshotResponse.PNGBytes);
            }
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("Take Screenshot"))
            {
                APIManager.SendToPlayers(new ScreenshotRequest());
            }

            if (lastTexture != null && lastTexture.width != 2)
            {
                var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.currentViewWidth / (lastTexture.width / (float) lastTexture.height));
                GUI.DrawTexture(rect, lastTexture, ScaleMode.ScaleToFit);
            }
        }
    }
}