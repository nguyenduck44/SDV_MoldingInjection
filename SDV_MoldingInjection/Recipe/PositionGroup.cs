using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipePositionManager : RecipePositionManagerBase<RecipeList>
    {
        public RecipePositionManager(RecipeList recipeList, IEnumerable<IMotion> motions)
            : base(recipeList, motions)
        {
        }
    }
}
