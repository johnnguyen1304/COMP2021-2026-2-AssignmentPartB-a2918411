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
