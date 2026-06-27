const DB_NAME = "ExpenseTrackerOfflineDb";
const DB_VERSION = 2;

const EXPENSES_STORE = "expenses";
const RECEIPTS_STORE = "receiptDrafts";
const TRIPS_STORE = "trips";

function openOfflineDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = event => {
            const db = event.target.result;

            if (!db.objectStoreNames.contains(EXPENSES_STORE)) {
                db.createObjectStore(EXPENSES_STORE, {
                    keyPath: "localId",
                    autoIncrement: true
                });
            }

            /*if (!db.objectStoreNames.contains(TRIPS_STORE)) {
                db.createObjectStore(TRIPS_STORE, {
                    keyPath: "id"
                });
            }

            if (!db.objectStoreNames.contains(RECEIPTS_STORE)) {
                db.createObjectStore(RECEIPTS_STORE, {
                    keyPath: "localId",
                    autoIncrement: true
                });
            }*/
        };

        request.onsuccess = event => {
            resolve(event.target.result);
        };

        request.onerror = event => {
            reject(event.target.error);
        };
    });
}

async function addToStore(storeName, item) {
    const db = await openOfflineDb();
    return new Promise((resolve, reject) => {

        const transaction = db.transaction(storeName, "readwrite");
        const store = transaction.objectStore(storeName);

        const request = store.add(item);

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function getAllFromStore(storeName) {
    const db = await openOfflineDb();
    return new Promise((resolve, reject) => {

        const transaction = db.transaction(storeName, "readonly");
        const store = transaction.objectStore(storeName);

        const request = store.getAll();

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function getFromStore(storeName, key) {
    const db = await openOfflineDb();
    return new Promise((resolve, reject) => {

        const transaction = db.transaction(storeName, "readonly");
        const store = transaction.objectStore(storeName);

        const request = store.get(key);

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function deleteFromStore(storeName, key) {
    const db = await openOfflineDb();
    return new Promise((resolve, reject) => {

        const transaction = db.transaction(storeName, "readwrite");
        const store = transaction.objectStore(storeName);

        const request = store.delete(key);

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function putInStore(storeName, item) {
    const db = await openOfflineDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction(storeName, "readwrite");
        const store = transaction.objectStore(storeName);

        const request = store.put(item);

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}
console.log("offline 1/10 - storage.js loaded");