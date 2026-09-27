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
