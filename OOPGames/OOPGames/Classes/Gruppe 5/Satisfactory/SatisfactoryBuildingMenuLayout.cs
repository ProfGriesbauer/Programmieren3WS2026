namespace OOPGames
{
    internal static class SatisfactoryBuildingMenuLayout
    {
        internal const double Left = 12;
        internal const double Top = 64;
        internal const double Width = 280;
        internal const double Height = 340;
        internal const double RecipeTop = Top + 102;
        internal const double RecipeRowHeight = 28;

        internal static bool Contains(double x, double y)
        {
            return x >= Left && x <= Left + Width &&
                y >= Top && y <= Top + Height;
        }

        internal static bool IsCloseButton(double x, double y)
        {
            return x >= Left + Width - 32 && x <= Left + Width - 8 &&
                y >= Top + 6 && y <= Top + 28;
        }

        internal static bool TryGetRecipeIndex(
            double x,
            double y,
            int recipeCount,
            out int recipeIndex)
        {
            recipeIndex = -1;
            if (x < Left + 10 || x > Left + Width - 10 || y < RecipeTop)
            {
                return false;
            }

            int row = (int)((y - RecipeTop) / RecipeRowHeight);
            if (row < 0 || row >= recipeCount || row >= 5 ||
                y >= RecipeTop + (row + 1) * RecipeRowHeight)
            {
                return false;
            }

            recipeIndex = row;
            return true;
        }
    }
}
