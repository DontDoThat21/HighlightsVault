using HighlightsVault.Models;

namespace HighlightsVault.Helpers
{
    public static class HighlightsHelper
    {
        public static int previousGroupColorId;
        public static string previousGroupId;

        // Function to convert byte array to Base64 string
        public static string GetImageSrc(byte[] imageBytes)
        {
            if (imageBytes != null && imageBytes.Length > 0)
            {
                string base64String = Convert.ToBase64String(imageBytes);
                return $"data:image/jpeg;base64,{base64String}";
            }
            return "/path/to/default/image.jpg"; // Default image if no profile picture is available
        }

        public static string GetRowClass(Highlight highlight, ref int previousGroupColorId, ref string previousGroupId)
        {
            string[] availableGroupColors = { "skyblue", "lightgreen", "indianred" };
            string groupColor = "";

            bool isGroup = false;
            if (highlight.GroupId != null)
            {
                isGroup = !isGroup;
                if (previousGroupId == highlight.GroupId.ToString())
                {
                    groupColor = availableGroupColors[previousGroupColorId];
                }
                else
                {
                    previousGroupId = highlight.GroupId.ToString();
                    previousGroupColorId += 1;
                    if (previousGroupColorId > 2)
                    {
                        previousGroupColorId = 0;
                    }
                    groupColor = availableGroupColors[previousGroupColorId];
                }
            }

            if (isGroup)
            {
                return $"group-highlight-{groupColor}";
            }
            else
            {
                return "normal-highlight";
            }
        }
    }
}
