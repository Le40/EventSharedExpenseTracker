const DB_NAME = "ExpenseTrackerOfflineDb";
const DB_VERSION = 1;

function openOfflineDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = event => {
            const db = event.target.result;

            if (!db.objectStoreNames.contains("expenses")) {
                db.createObjectStore("expenses", {
                    keyPath: "localId",
                    autoIncrement: true
                });
            }

            if (!db.objectStoreNames.contains("trips")) {
                db.createObjectStore("trips", {
                    keyPath: "id"
                });
            }

            if (!db.objectStoreNames.contains("receiptDrafts")) {
                db.createObjectStore("receiptDrafts", {
                    keyPath: "localId",
                    autoIncrement: true
                });
            }
        };

        request.onsuccess = event => {
            resolve(event.target.result);
        };

        request.onerror = event => {
            reject(event.target.error);
        };
    });
}

async function saveOfflineExpense(expense) {
    const db = await openOfflineDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction("expenses", "readwrite");
        const store = transaction.objectStore("expenses");

        const request = store.put(expense);

        request.onsuccess = () => resolve(request.result);
        request.onerror = event => reject(event.target.error);
    });
}

async function getAllOfflineExpenses() {
    const db = await openOfflineDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction("expenses", "readonly");
        const store = transaction.objectStore("expenses");

        const request = store.getAll();

        request.onsuccess = () => resolve(request.result);
        request.onerror = event => reject(event.target.error);
    });
}

async function getTripWithMostOfflineExpenses() {

    const db = await openOfflineDb();

    const tx = db.transaction("expenses", "readonly");
    const store = tx.objectStore("expenses");

    const expenses = await getAllOfflineExpenses();

    const tripCounts = {};

    for (const expense of expenses) {

        if (!["pendingCreate", "failedValidation"].includes(expense.syncState))
            continue;

        tripCounts[expense.tripId] ??= {
            tripId: expense.tripId,
            tripName: expense.tripName,
            count: 0
        };

        tripCounts[expense.tripId].count++;
    }

    return Object.values(tripCounts)
        .sort((a, b) => b.count - a.count)[0];
}

async function getOfflineExpenses(tripId) {
    const db = await openOfflineDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction("expenses", "readonly");
        const store = transaction.objectStore("expenses");

        const request = store.getAll();

        request.onsuccess = () => {
            const expenses = request.result
                .filter(x => x.tripId = tripId);
            resolve(request.expenses);
        };
        request.onerror = event => reject(event.target.error);
    });
}

async function deleteOfflineExpense(id) {
    const db = await openOfflineDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction("expenses", "readwrite");
        const store = transaction.objectStore("expenses");

        const request = store.delete(id);

        request.onsuccess = () => resolve(request.result);
        request.onerror = event => reject(event.target.error);
    });
}

console.log("offline 3/5 - storage.js loaded");