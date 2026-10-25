# Design

Complete this document for **Part B**.

## 1. Saved recipe collection

I chose HashSet<int> because it stores unique elements and does not require an order. It is suitable for saved recipes because each recipe ID should only appear once, and the saved recipes do not need to be sorted.

Before saving a recipe, the program checks if its ID exists in the recipe dictionary. Add saves the ID and returns false if it is already saved. Remove removes a saved ID, and Contains checks if the recipe is saved.

Adding an ID takes average amortised O(1) time, although resizing can make an individual addition take O(n). Removing and checking an ID take average O(1) time. A List would need to search through its items to check for duplicates, so HashSet is more suitable for this feature.

GetSavedRecipes returns a separate list of saved IDs. This means changes to the returned list do not change the internal HashSet.

## 2. Integration

The Part B features use the existing RecipeManager and recipe dictionary from Part A. The searches and protein report use LINQ to return matching recipes, while the saved recipe feature stores existing recipe IDs in a HashSet. I kept the supplied models, loader, interface and menu, along with the Part A collections and tests. The integrated test searches for a recipe, saves it, uses it in the cooking plan and instruction queue, then removes it from saved recipes. All 86 tests passed.

## 3. Basic complexity

| Operation | Structure | Expected complexity | Reason |
| --- | --- | --- | --- |
| Lookup recipe by ID | Dictionary | Average O(1) | Hash-based key lookup. |
| Traverse cooking plan | LinkedList | O(n) | Each planned recipe may need to be visited. |
| Complete next instruction | Queue | O(1) | The item at the front is removed. |
| LINQ title/ingredient search | Recipe collection | O(n) | Each recipe may need to be inspected. |
| Check whether a recipe is saved | HashSet | Average O(1) | It uses the ID's hash to check membership without searching through every saved ID. |
