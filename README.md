# Recipe Management System — Student Starter

Starter repository for Parts A and B. Implement `RecipeManager` in Core; the Application menu and JSON loader are supplied.

## What is supplied

- `RecipeManagement.Core/Models/` — recipe model classes
- `RecipeManagement.Core/RecipeLoader.cs` — reads `data/recipes.json`
- `RecipeManagement.Core/IRecipeManager.cs` — public API
- `RecipeManagement.Application/` — console menu (options labelled PartA / PartB)
- `RecipeManagement.Tests/` — example tests
- `data/recipes.json` — recipe dataset

## What you implement

**Part A** — `RecipeManager.cs` using:

- `Dictionary<int, Recipe>`
- `List<string>`
- `LinkedList<int>`
- `Stack<int>`
- `Queue<string>`

**Part B** — LINQ searches, protein report, saved-recipe collection, `Design.md`, and more tests.

## Build and run

Open `StudentPackage/RecipeManagement.sln`:

```bash
dotnet build
dotnet test
dotnet run --project RecipeManagement.Application -- data/recipes.json
```

Until you implement `RecipeManager`, menu options print a **Not implemented** message.

## AI acknowledgement

I used ChatGPT/Codex to help me understand what `?` means in C#. I learned that `Recipe?` means the result can be a recipe or null, while `condition ? value : null` chooses what to return based on the condition. This helped me understand how `FindRecipe` returns a recipe when the ID exists and null when it does not.