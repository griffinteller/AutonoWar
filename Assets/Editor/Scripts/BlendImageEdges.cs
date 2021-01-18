using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    public class BlendImageEdges : ScriptableWizard
    {
        public Texture2D image;
        public int       vertPixelRange;
        public int       horiPixelRange;
        
        [MenuItem("Assets/Blend Image Edges")]
        public static void CreateWizard()
        {
            DisplayWizard<BlendImageEdges>("Blend Image Edges", "Blend");
        }

        public void OnWizardCreate()
        {
            BlendVertical();
            BlendHorizontal();
            image.Apply();
            
            EditorUtility.SetDirty(image);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void BlendHorizontal()
        {
            int width  = image.width;
            int height = image.height;

            for (int row = 0; row < height; row++) // 0 on bottom, goes up on image
            for (int col = 0; col < horiPixelRange; col++)
            {
                float weight = (float) (col + horiPixelRange / 2) / horiPixelRange;
                
                Color left = image.GetPixel(col, row);
                Color right    = image.GetPixel(width - 1 - col, row);
                
                Color leftWeightedAverage = weight * left + (1 - weight) * right;
                Color rightWeightedAverage    = weight * right    + (1 - weight) * left;
                
                image.SetPixel(col, row, leftWeightedAverage);
                image.SetPixel(width - 1 - col, row, rightWeightedAverage);
            }
        }

        private void BlendVertical()
        {
            int width  = image.width;
            int height = image.height;

            for (int row = 0; row < vertPixelRange; row++) // 0 on bottom, goes up on image
            for (int col = 0; col < width; col++)
            {
                float weight = (float) (row + vertPixelRange / 2) / vertPixelRange;
                
                Color bottom = image.GetPixel(col, row);
                Color top    = image.GetPixel(col, height - 1 - row);
                
                Color bottomWeightedAverage = weight * bottom + (1 - weight) * top;
                Color topWeightedAverage    = weight * top    + (1 - weight) * bottom;
                
                image.SetPixel(col, row, bottomWeightedAverage);
                image.SetPixel(col, height - 1 - row, topWeightedAverage);
            }
        }

        public void OnWizardUpdate()
        {
            
        }
    }
}