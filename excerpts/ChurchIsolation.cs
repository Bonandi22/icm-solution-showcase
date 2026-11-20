// Excerpts from ICM Solution (private repository) — ExpensesServices.Application/Services/ExpensesService.cs
// Multi-tenant isolation: the church a user may access comes from the JWT (`churchIds` claim),
// never from what the client sends. Excerpts are trimmed; validation, caching and mapping omitted.

// 1) A list query narrows the requested churches to the ones the user actually belongs to.
public async Task<List<ExpenseDto>> GetByChurchIdAsync(List<int> churchIds) =>
    await ExecuteSafeAsync(async () =>
    {
        ValidateChurchIds(churchIds);

        var allowedChurchIds = CanViewAllChurches()
            ? churchIds
            : churchIds.Intersect(GetCurrentUserChurchIds()).ToList();

        if (allowedChurchIds.Count == 0)
            return new List<ExpenseDto>();

        // ... query only allowedChurchIds
    });

// 2) Loading a single record: a record from another church is reported as "not found",
//    so the API does not even reveal that it exists.
private async Task<Expense> GetExpenseById(int id)
{
    var expense = await _expenseRepository.GetByIdAsync(id);

    if (expense == null || (!CanViewAllChurches() && !GetCurrentUserChurchIds().Contains(expense.ChurchId)))
        throw new NotFoundException($"Expense with ID {id} not found");

    return expense;
}

private List<int> GetCurrentUserChurchIds() => _currentUserService.ChurchIds?.ToList() ?? new List<int>();
