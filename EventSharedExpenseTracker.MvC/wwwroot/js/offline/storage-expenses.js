
async function createOfflineExpense(expense) {

    return await addToStore(EXPENSES_STORE, expense);
}

async function getAllOfflineExpenses() {

    return await getAllFromStore(EXPENSES_STORE);
}

async function getOfflineExpensesForTrip(tripId) {

    const expenses = await getAllOfflineExpenses();
    return expenses.filter(x => Number(x.tripId) === Number(tripId));
}

async function getOfflineExpense(localId) {

    return await getFromStore(EXPENSES_STORE, localId);
}

async function deleteOfflineExpense(localId) {

    return await deleteFromStore(EXPENSES_STORE, localId);
}

async function updateOfflineExpense(draft) {
    return await putInStore(EXPENSES_STORE, draft);
}

async function getTripWithMostOfflineExpenses() {

    const expenses = await getAllOfflineExpenses();

    const tripCounts = {};

    for (const expense of expenses) {

        tripCounts[expense.tripId] ??= {
            tripId: expense.tripId,
            tripName: expense.tripName,
            count: 0
        };

        tripCounts[expense.tripId].count++;
    }

    // find trip with most pending expenses
    let maxTrip = null;
    for (const trip of Object.values(tripCounts)) {
        if (!maxTrip || trip.count > maxTrip.count) {
            maxTrip = trip;
        }
    }

    return maxTrip;
}

console.log("offline 2/10 - storage-expenses.js loaded");