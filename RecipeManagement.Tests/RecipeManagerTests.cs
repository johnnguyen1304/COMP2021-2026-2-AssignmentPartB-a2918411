using System.Collections.Generic;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

/// <summary>
/// Example tests from the assignment specification. Add your own tests as you work.
/// </summary>
public sealed class RecipeManagerTests
{
    [Fact]
    public void Constructor_BuildsRecipeDictionary()
    {
        var manager = CreateManager();
        Assert.Equal(2, manager.RecipeCount);
        Assert.Equal("Recipe A", manager.FindRecipe(10)?.Title);
    }

    [Fact]
    public void InstructionsAreCompletedInFileOrder()
    {
        var manager = CreateManager();
        Assert.True(manager.StartCooking(10));
        Assert.Equal("First step", manager.PeekNextInstruction());
        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal("Second step", manager.PeekNextInstruction());
    }

    [Fact]
    public void RemovedRecipesAreRestoredLastInFirstOut()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(20);
        Assert.Equal(20, manager.PeekLastRemovedRecipe());
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 20 }, manager.GetCookingPlan());
    }

    [Fact]
    public void Constructor_RejectsNullCollection()
    {
        Assert.Throws<System.ArgumentNullException>(() => new RecipeManager(null!));
    }

    [Theory]
    [InlineData(0, "Recipe A")]
    [InlineData(-1, "Recipe A")]
    [InlineData(10, "")]
    [InlineData(10, " ")]
    [InlineData(10, null)]
    public void Constructor_RejectsInvalidRecipe(int id, string? title)
    {
        var recipes = new[]
        {
            new Recipe { Id = id, Title = title! }
        };

        Assert.Throws<System.ArgumentException>( () => new RecipeManager(recipes));
    }

    [Fact]
    public void Constructor_RejectsDuplicateIds()
    {
        var recipes = new[]
        {
            new Recipe { Id = 10, Title = "Recipe A"},
            new Recipe { Id = 10, Title = "Recipe B"}
        };

        Assert.Throws<System.ArgumentException>( () => new RecipeManager(recipes));
    }

    [Fact]
    public void Constructor_AcceptsEmptyCollection()
    {
        var manager = new RecipeManager(System.Array.Empty<Recipe>());

        Assert.Equal(0, manager.RecipeCount);
        Assert.Null(manager.FindRecipe(10));
    }

    [Fact]
    public void FindRecipe_ReturnsNullForMissingId()
    {
        var manager = CreateManager();

        Assert.Null(manager.FindRecipe(999));
    }

    [Fact]
    public void AddRecipe_AddsValidRecipe()
    {
        var manager = CreateManager();
        var recipe = new Recipe {Id = 30, Title = "Recipe C"};

        bool result = manager.AddRecipe(recipe);

        Assert.True(result);
        Assert.Equal(3, manager.RecipeCount);
        Assert.Same(recipe, manager.FindRecipe(30));
    }
    
    [Fact]
    public void AddRecipe_RejectDuplicateIdWithoutReplacingOriginal()
    {
        var manager = CreateManager();
        var original = manager.FindRecipe(10);
        var duplicate = new Recipe { Id = 10, Title = "Replacement"};

        bool result = manager.AddRecipe(duplicate);

        Assert.False(result);
        Assert.Equal(2, manager.RecipeCount);
        Assert.Same(original, manager.FindRecipe(10));
    }

    [Fact]
    public void AddRecipe_RejectNull()
    {
        var manager = CreateManager();

        Assert.Throws<System.ArgumentNullException>( () => manager.AddRecipe(null!));

        Assert.Equal(2, manager.RecipeCount);
    }

    [Theory]
    [InlineData(0, "Recipe C")]
    [InlineData(-1, "Recipe C")]
    [InlineData(30, "")]
    [InlineData(30, " ")]
    [InlineData(30, null)]
    public void AddRecipe_RejectInvalidRecipe(int id, string? title)
    {
        var manager = CreateManager();
        var recipe = new Recipe {Id = id, Title = title!};

        bool result = manager.AddRecipe(recipe);

        Assert.False(result);
        Assert.Equal(2, manager.RecipeCount);
        Assert.Null(manager.FindRecipe(id));
    }

    [Fact]
    public void RemoveRecipe_RemovesExistingRecipe()
    {
        var manager = CreateManager();

        bool result = manager.RemoveRecipe(10);

        Assert.True(result);
        Assert.Equal(1, manager.RecipeCount);
        Assert.Null(manager.FindRecipe(10));
        Assert.Equal("Recipe B", manager.FindRecipe(20)?.Title);
    }
    
    [Fact]
    public void RemoveRecipe_ReturnsFalseForMissingId()
    {
        var manager = CreateManager();

        bool result = manager.RemoveRecipe(999);

        Assert.False(result);
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void RemoveRecipe_ReturnsFalseWhenAlreadyRemoved()
    {
        var manager = CreateManager();

        Assert.True(manager.RemoveRecipe(10));
        Assert.False(manager.RemoveRecipe(10));
        Assert.Equal(1, manager.RecipeCount);
    }

    [Fact]
    public void AddRecipe_CanReuseRemovedId()
    {
        var manager = CreateManager();
        var replacement = new Recipe
        {
            Id = 10,
            Title = "Replacement recipe"
        };

        Assert.True(manager.RemoveRecipe(10));
        Assert.True(manager.AddRecipe(replacement));
        Assert.Equal(2, manager.RecipeCount);
        Assert.Same(replacement, manager.FindRecipe(10));
    }

    [Fact]
    public void ShoppingList_StartsEmpty()
    {
        var manager = CreateManager();

        Assert.Equal(0, manager.ShoppingItemCount);
        Assert.Empty(manager.GetShoppingList());
    }

    [Fact]
    public void ShoppingList_CopiesIngridientsInOriginalOrder()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 30,
                Title = "Soup",
                Ingredients = new()
                {
                    "2 cups water", "1 tsp salt", "3 carrots"
                }
            }
        });

        int added = manager.AddIngredientsToShoppingList(30);

        Assert.Equal(3, added);
        Assert.Equal(3, manager.ShoppingItemCount);
        Assert.Equal(new[] {"2 cups water", "1 tsp salt", "3 carrots" }, manager.GetShoppingList());
    }

    [Fact]
    public void ShoppingList_AppendsIngredientsAndKeepsDuplicates()
    {
        var manager = CreateManager();

        manager.AddIngredientsToShoppingList(10);
        int added = manager.AddIngredientsToShoppingList(10);

        Assert.Equal(1, added);
        Assert.Equal(2, manager.ShoppingItemCount);
        Assert.Equal(new[] {"1 apple", "1 apple"}, manager.GetShoppingList());
    }

    [Fact]
    public void ShoppingList_MissingRecipeLeavesExistingItemsUnchanged()
    {
        var manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);

        int added = manager.AddIngredientsToShoppingList(999);

        Assert.Equal(0, added);
        Assert.Equal(1, manager.ShoppingItemCount);
        Assert.Equal(new[] {"1 apple"}, manager.GetShoppingList());
    }

    [Fact]
    public void ShoppingList_RecipeWithoutIngredientsAddsNothing()
    {
        var manager = CreateManager();

        int added = manager.AddIngredientsToShoppingList(20);

        Assert.Equal(0, added);
        Assert.Equal(0, manager.ShoppingItemCount);
        Assert.Empty(manager.GetShoppingList());
    }

    [Fact]
    public void ShoppingList_ClearRemovesAllItems()
    {
        var manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);

        manager.ClearShoppingList();

        Assert.Equal(0, manager.ShoppingItemCount);
        Assert.Empty(manager.GetShoppingList());
        Assert.Single(manager.FindRecipe(10)!.Ingredients);
    }

    [Fact]
    public void ShoppingList_ReturnedCopyCannotChangeInternalList()
    {
        var manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);
        var returnedList = manager.GetShoppingList();

        // Attempt a change if the returned collection allows it.
        if (returnedList is ICollection<string> collection && !collection.IsReadOnly)
        {
            collection.Clear();
        }

        Assert.Equal(1, manager.ShoppingItemCount);
        Assert.Equal(new[] {"1 apple"}, manager.GetShoppingList());
    }

    [Fact]
    public void CookingPlan_AppendsRecipesInOrder()
    {
        var manager = CreateManager();

        Assert.True(manager.AddRecipeToCookingPlan(20));
        Assert.True(manager.AddRecipeToCookingPlan(10));

        Assert.Equal(2, manager.CookingPlanCount);
        Assert.Equal(new[] { 20, 10 }, manager.GetCookingPlan());
    }

    [Fact]
    public void CookingPlan_RejectsMissingAndDuplicateRecipes()
    {
        var manager = CreateManager();

        Assert.False(manager.AddRecipeToCookingPlan(999));
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.False(manager.AddRecipeToCookingPlan(10));

        Assert.Equal(1, manager.CookingPlanCount);
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
    }

    [Fact]
    public void CookingPlan_ReturnedCopyCannotChangeInternalPlan()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        var returnedPlan = manager.GetCookingPlan();

        if (returnedPlan is ICollection<int> collection && !collection.IsReadOnly)
        {
            collection.Clear();
        }

        Assert.Equal(1, manager.CookingPlanCount);
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
    }

    [Fact]
    public void CookingPlan_RemovalPreservesOrderAndRecordsHistory()
    {
        var manager = CreateManager();
        manager.AddRecipe(new Recipe { Id = 30, Title = "Recipe C" });
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.AddRecipeToCookingPlan(30);

        Assert.True(manager.RemoveRecipeFromCookingPlan(20));

        Assert.Equal(new[] { 10, 30 }, manager.GetCookingPlan());
        Assert.Equal(2, manager.CookingPlanCount);
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Equal(20, manager.PeekLastRemovedRecipe());
        Assert.NotNull(manager.FindRecipe(20));
    }

    [Fact]
    public void CookingPlan_FailedRemovalDoesNotChangeHistory()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.False(manager.RemoveRecipeFromCookingPlan(20));
        Assert.False(manager.RemoveRecipeFromCookingPlan(999));

        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Equal(10, manager.PeekLastRemovedRecipe());
    }

    [Fact]
    public void RemovedHistory_EmptyOperationsAreSafe()
    {
        var manager = CreateManager();

        Assert.Null(manager.PeekLastRemovedRecipe());
        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Empty(manager.GetCookingPlan());
    }

    [Fact]
    public void RemovedHistory_PeekDoesNotRemoveEntry()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.Equal(10, manager.PeekLastRemovedRecipe());
        Assert.Equal(10, manager.PeekLastRemovedRecipe());
        Assert.Equal(1, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RemovedHistory_RestoresInReverseRemovalOrderAtEndOfPlan()
    {
        var manager = CreateManager();
        manager.AddRecipe(new Recipe { Id = 30, Title = "Recipe C" });
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.AddRecipeToCookingPlan(30);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(20);

        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 30, 20 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.RemovedRecipeCount);

        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 30, 20, 10 }, manager.GetCookingPlan());
        Assert.Equal(3, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RemovedHistory_DeletedRecipeCannotBeRestored()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);
        Assert.True(manager.RemoveRecipe(10));

        Assert.False(manager.RestoreLastRemovedRecipe());

        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Empty(manager.GetCookingPlan());
    }

    [Fact]
    public void RemovedHistory_AlreadyPlannedRecipeCannotBeRestored()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.AddRecipeToCookingPlan(10);

        Assert.False(manager.RestoreLastRemovedRecipe());

        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
    }

    [Fact]
    public void RemoveRecipe_RejectsPlannedRecipeUntilRemovedFromPlan()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);

        Assert.False(manager.RemoveRecipe(10));
        Assert.NotNull(manager.FindRecipe(10));
        Assert.Equal(2, manager.RecipeCount);

        Assert.True(manager.RemoveRecipeFromCookingPlan(10));
        Assert.True(manager.RemoveRecipe(10));
        Assert.Null(manager.FindRecipe(10));
        Assert.Equal(1, manager.RecipeCount);
    }

    [Fact]
    public void InstructionQueue_EmptyOperationsReturnNull()
    {
        var manager = CreateManager();

        Assert.Equal(0, manager.PendingInstructionCount);
        Assert.Null(manager.PeekNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void InstructionQueue_StartLoadsAllInstructions()
    {
        var manager = CreateManager();

        Assert.True(manager.StartCooking(10));

        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("First step", manager.PeekNextInstruction());
    }

    [Fact]
    public void InstructionQueue_PeekDoesNotRemoveInstruction()
    {
        var manager = CreateManager();
        manager.StartCooking(10);

        Assert.Equal("First step", manager.PeekNextInstruction());
        Assert.Equal("First step", manager.PeekNextInstruction());
        Assert.Equal(2, manager.PendingInstructionCount);
    }

    [Fact]
    public void InstructionQueue_CompletesOneInstructionAtATimeInOrder()
    {
        var manager = CreateManager();
        manager.StartCooking(10);

        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal(1, manager.PendingInstructionCount);

        Assert.Equal("Second step", manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);

        Assert.Null(manager.PeekNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());

        Assert.Equal(
            new[] { "First step", "Second step" },
            manager.FindRecipe(10)!.Instructions);
    }

    [Fact]
    public void InstructionQueue_ValidSessionReplacesPreviousInstructions()
    {
        var manager = CreateManager();
        manager.AddRecipe(new Recipe
        {
            Id = 30,
            Title = "Recipe C",
            Instructions = new() { "Mix", "Serve" }
        });
        manager.StartCooking(10);

        Assert.True(manager.StartCooking(30));

        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("Mix", manager.CompleteNextInstruction());
        Assert.Equal("Serve", manager.CompleteNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
    }

    [Theory]
    [InlineData(999)]
    [InlineData(20)]
    public void InstructionQueue_InvalidSessionPreservesCurrentQueue(int recipeId)
    {
        var manager = CreateManager();
        manager.StartCooking(10);

        Assert.False(manager.StartCooking(recipeId));

        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal("Second step", manager.CompleteNextInstruction());
    }

    [Fact]
    public void InstructionQueue_RestartBeginsAtFirstInstruction()
    {
        var manager = CreateManager();
        manager.StartCooking(10);
        manager.CompleteNextInstruction();

        Assert.True(manager.StartCooking(10));

        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("First step", manager.PeekNextInstruction());
    }
    [Theory]
    [InlineData("soup")]
    [InlineData("SOUP")]
    [InlineData("SoUp")]
    public void SearchByTitle_FindsPartialMatchesIgnoringCase(string searchText)
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe { Id = 1, Title = "Tomato Soup" },
            new Recipe { Id = 2, Title = "Chicken SOUP" },
            new Recipe { Id = 3, Title = "Apple Pie" }
        });

        var results = manager.SearchByTitle(searchText);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, recipe => recipe.Id == 1);
        Assert.Contains(results, recipe => recipe.Id == 2);
    }
    [Fact]
    public void SearchByTitle_NoMatchReturnsEmpty()
    {
        var manager = CreateManager();

        var results = manager.SearchByTitle("pizza");

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SearchByTitle_BlankSearchReturnsAllRecipes(string searchText)
    {
        var manager = CreateManager();

        var results = manager.SearchByTitle(searchText);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, recipe => recipe.Id == 10);
        Assert.Contains(results, recipe => recipe.Id == 20);
    }

    [Fact]
    public void SearchByTitle_EmptyCatalogueReturnsEmpty()
    {
        var manager = new RecipeManager(System.Array.Empty<Recipe>());

        Assert.Empty(manager.SearchByTitle("soup"));
    }

    [Fact]
    public void SearchByTitle_ReturnsRecipeOnceWhenWordAppearsTwice()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe { Id = 1, Title = "Soup with Soup Dumplings" }
        });

        var results = manager.SearchByTitle("soup");

        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    [Theory]
    [InlineData("tomato")]
    [InlineData("TOMATO")]
    [InlineData("ToMaTo")]
    public void SearchByIngredient_FindsPartialMatchesIgnoringCase(
        string searchText)
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Soup",
                Ingredients = new() { "2 cups water", "3 chopped tomatoes" }
            },
            new Recipe
            {
                Id = 2,
                Title = "Pasta",
                Ingredients = new() { "1 tbsp TOMATO paste", "200 g pasta" }
            },
            new Recipe
            {
                Id = 3,
                Title = "Apple Pie",
                Ingredients = new() { "2 apples", "100 g flour" }
            }
        });

        var results = manager.SearchByIngredient(searchText);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, recipe => recipe.Id == 1);
        Assert.Contains(results, recipe => recipe.Id == 2);
    }

    [Fact]
    public void SearchByIngredient_ReturnsRecipeOnceForMultipleMatches()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Tomato Soup",
                Ingredients = new()
                {
                    "3 tomatoes",
                    "1 tbsp tomato paste",
                    "200 ml tomato juice"
                }
            }
        });

        var results = manager.SearchByIngredient("tomato");

        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    [Fact]
    public void SearchByIngredient_DoesNotMatchTitleAlone()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Apple Pie",
                Ingredients = new() { "100 g flour", "50 g sugar" }
            }
        });

        Assert.Empty(manager.SearchByIngredient("apple"));
    }

    [Fact]
    public void SearchByIngredient_NoMatchReturnsEmpty()
    {
        var manager = CreateManager();

        Assert.Empty(manager.SearchByIngredient("chicken"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SearchByIngredient_BlankSearchReturnsAllRecipes(
        string searchText)
    {
        var manager = CreateManager();

        var results = manager.SearchByIngredient(searchText);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, recipe => recipe.Id == 10);
        Assert.Contains(results, recipe => recipe.Id == 20);
    }

    [Fact]
    public void SearchByIngredient_EmptyCatalogueReturnsEmpty()
    {
        var manager = new RecipeManager(System.Array.Empty<Recipe>());

        Assert.Empty(manager.SearchByIngredient("apple"));
    }

    [Fact]
    public void SearchByIngredient_RecipeWithoutIngredientsDoesNotMatch()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe { Id = 1, Title = "Recipe without ingredients" }
        });

        Assert.Empty(manager.SearchByIngredient("apple"));
    }

    [Fact]
    public void GetHighestProteinRecipes_ReturnsRequestedNumberInDescendingOrder()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Low protein",
                Nutrition = new NutritionInfo { ProteinG = 5 }
            },
            new Recipe
            {
                Id = 2,
                Title = "High protein",
                Nutrition = new NutritionInfo { ProteinG = 30 }
            },
            new Recipe
            {
                Id = 3,
                Title = "Medium protein",
                Nutrition = new NutritionInfo { ProteinG = 15 }
            }
        });

        var results = manager.GetHighestProteinRecipes(2);

        Assert.Equal(2, results.Count);
        Assert.Equal(2, results[0].Id);
        Assert.Equal(3, results[1].Id);
    }

    [Fact]
    public void GetHighestProteinRecipes_ExcludesMissingNutritionAndProtein()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Missing nutrition",
                Nutrition = null
            },
            new Recipe
            {
                Id = 2,
                Title = "Missing protein",
                Nutrition = new NutritionInfo { ProteinG = null }
            },
            new Recipe
            {
                Id = 3,
                Title = "Known protein",
                Nutrition = new NutritionInfo { ProteinG = 12 }
            }
        });

        var results = manager.GetHighestProteinRecipes(10);

        Assert.Single(results);
        Assert.Equal(3, results[0].Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetHighestProteinRecipes_NonPositiveCountReturnsEmpty(int count)
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Known protein",
                Nutrition = new NutritionInfo { ProteinG = 20 }
            }
        });

        Assert.Empty(manager.GetHighestProteinRecipes(count));
    }

    [Fact]
    public void GetHighestProteinRecipes_LargeCountReturnsAllAvailableInOrder()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Lower protein",
                Nutrition = new NutritionInfo { ProteinG = 8 }
            },
            new Recipe
            {
                Id = 2,
                Title = "Higher protein",
                Nutrition = new NutritionInfo { ProteinG = 25 }
            }
        });

        var results = manager.GetHighestProteinRecipes(10);

        Assert.Equal(2, results.Count);
        Assert.Equal(2, results[0].Id);
        Assert.Equal(1, results[1].Id);
    }

    [Fact]
    public void GetHighestProteinRecipes_ZeroProteinIsIncluded()
    {
        var manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Zero protein",
                Nutrition = new NutritionInfo { ProteinG = 0 }
            }
        });

        var results = manager.GetHighestProteinRecipes(1);

        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    [Fact]
    public void GetHighestProteinRecipes_AllProteinMissingReturnsEmpty()
    {
        var manager = CreateManager();

        Assert.Empty(manager.GetHighestProteinRecipes(5));
    }

    [Fact]
    public void GetHighestProteinRecipes_EmptyCatalogueReturnsEmpty()
    {
        var manager = new RecipeManager(System.Array.Empty<Recipe>());

        Assert.Empty(manager.GetHighestProteinRecipes(5));
    }

    [Fact]
    public void SavedRecipes_StartEmpty()
    {
        var manager = CreateManager();

        Assert.Empty(manager.GetSavedRecipes());
        Assert.False(manager.IsRecipeSaved(10));
    }

    [Fact]
    public void SavedRecipes_AddExistingRecipe()
    {
        var manager = CreateManager();

        Assert.True(manager.AddSavedRecipe(10));

        Assert.True(manager.IsRecipeSaved(10));
        Assert.Single(manager.GetSavedRecipes());
        Assert.Contains(10, manager.GetSavedRecipes());
    }

    [Fact]
    public void SavedRecipes_RejectMissingRecipe()
    {
        var manager = CreateManager();

        Assert.False(manager.AddSavedRecipe(999));

        Assert.False(manager.IsRecipeSaved(999));
        Assert.Empty(manager.GetSavedRecipes());
    }

    [Fact]
    public void SavedRecipes_RejectDuplicateId()
    {
        var manager = CreateManager();

        Assert.True(manager.AddSavedRecipe(10));
        Assert.False(manager.AddSavedRecipe(10));

        Assert.Single(manager.GetSavedRecipes());
        Assert.True(manager.IsRecipeSaved(10));
    }

    [Fact]
    public void SavedRecipes_ReturnAllSavedIds()
    {
        var manager = CreateManager();

        Assert.True(manager.AddSavedRecipe(20));
        Assert.True(manager.AddSavedRecipe(10));

        var results = manager.GetSavedRecipes();

        Assert.Equal(2, results.Count);
        Assert.Contains(10, results);
        Assert.Contains(20, results);
    }

    [Fact]
    public void SavedRecipes_RemoveSavedRecipe()
    {
        var manager = CreateManager();
        manager.AddSavedRecipe(10);
        manager.AddSavedRecipe(20);

        Assert.True(manager.RemoveSavedRecipe(10));

        Assert.False(manager.IsRecipeSaved(10));
        Assert.True(manager.IsRecipeSaved(20));
        Assert.Single(manager.GetSavedRecipes());
        Assert.NotNull(manager.FindRecipe(10));
    }

    [Fact]
    public void SavedRecipes_RemovingUnsavedIdLeavesOthersUnchanged()
    {
        var manager = CreateManager();
        manager.AddSavedRecipe(10);

        Assert.False(manager.RemoveSavedRecipe(20));
        Assert.False(manager.RemoveSavedRecipe(999));

        Assert.Single(manager.GetSavedRecipes());
        Assert.True(manager.IsRecipeSaved(10));
    }

    [Fact]
    public void SavedRecipes_CanSaveAgainAfterRemoval()
    {
        var manager = CreateManager();
        manager.AddSavedRecipe(10);

        Assert.True(manager.RemoveSavedRecipe(10));
        Assert.False(manager.RemoveSavedRecipe(10));
        Assert.True(manager.AddSavedRecipe(10));

        Assert.True(manager.IsRecipeSaved(10));
        Assert.Single(manager.GetSavedRecipes());
    }

    [Fact]
    public void SavedRecipes_ReturnedCopyCannotChangeInternalCollection()
    {
        var manager = CreateManager();
        manager.AddSavedRecipe(10);
        var returnedIds = manager.GetSavedRecipes();

        if (returnedIds is ICollection<int> collection
            && !collection.IsReadOnly)
        {
            collection.Clear();
        }

        Assert.True(manager.IsRecipeSaved(10));
        Assert.Single(manager.GetSavedRecipes());
    }

    [Fact]
    public void PartBWorkflow_SearchSavePlanCookAndUnsaveRecipe()
    {
        IRecipeManager manager = new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 1,
                Title = "Chicken Soup",
                Ingredients = new() { "200 g chicken", "500 ml water" },
                Instructions = new() { "Combine ingredients", "Simmer" },
                Nutrition = new NutritionInfo { ProteinG = 30 }
            },
            new Recipe
            {
                Id = 2,
                Title = "Vegetable Soup",
                Ingredients = new() { "2 carrots", "500 ml water" },
                Instructions = new() { "Chop vegetables", "Simmer" },
                Nutrition = new NutritionInfo { ProteinG = 5 }
            }
        });

        // Find a recipe through the Part B searches and report.
        var titleResults = manager.SearchByTitle("CHICKEN");
        var selectedRecipe = Assert.Single(titleResults);

        Assert.Equal(1, selectedRecipe.Id);

        var ingredientResults = manager.SearchByIngredient("chicken");
        Assert.Equal(selectedRecipe.Id, Assert.Single(ingredientResults).Id);

        var proteinResults = manager.GetHighestProteinRecipes(1);
        Assert.Equal(selectedRecipe.Id, Assert.Single(proteinResults).Id);

        // Save the selected recipe.
        Assert.True(manager.AddSavedRecipe(selectedRecipe.Id));
        Assert.True(manager.IsRecipeSaved(selectedRecipe.Id));
        Assert.Contains(selectedRecipe.Id, manager.GetSavedRecipes());

        // Use the same recipe in the existing Part A workflow.
        Assert.True(manager.AddRecipeToCookingPlan(selectedRecipe.Id));
        Assert.Equal(
            new[] { selectedRecipe.Id },
            manager.GetCookingPlan());

        Assert.True(manager.StartCooking(selectedRecipe.Id));
        Assert.Equal("Combine ingredients", manager.CompleteNextInstruction());
        Assert.Equal("Simmer", manager.CompleteNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());

        // Removing a saved entry leaves the catalogue and plan intact.
        Assert.True(manager.RemoveSavedRecipe(selectedRecipe.Id));
        Assert.False(manager.IsRecipeSaved(selectedRecipe.Id));
        Assert.Empty(manager.GetSavedRecipes());
        Assert.NotNull(manager.FindRecipe(selectedRecipe.Id));
        Assert.Contains(selectedRecipe.Id, manager.GetCookingPlan());
    }

    private static RecipeManager CreateManager()
    {
        return new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 10,
                Title = "Recipe A",
                Ingredients = new() { "1 apple" },
                Instructions = new() { "First step", "Second step" }
            },
            new Recipe
            {
                Id = 20,
                Title = "Recipe B"
            }
        });
    }
}
