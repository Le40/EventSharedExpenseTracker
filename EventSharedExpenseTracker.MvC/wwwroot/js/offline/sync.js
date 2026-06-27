// sync lock
let syncInProgress = false;

// SYNC LOCK
function tryAcquireSyncLock() {
    const lockKey = "expense-sync-lock";
    const now = Date.now();
    const existing = Number(localStorage.getItem(lockKey));

    // lock valid for 30 seconds
    if (existing && now - existing < 30000) {
        return false;
    }

    localStorage.setItem(lockKey, String(now));
    return true;
}

function releaseSyncLock() {
    localStorage.removeItem("expense-sync-lock");
}


async function syncDrafts() {

    if (syncInProgress) {
        console.log("Sync already running in this tab.");
        return;
    }

    if (!tryAcquireSyncLock()) {
        console.log("Sync already running in another tab.");
        return;
    }

    syncInProgress = true;

    try {
        const drafts = await getAllOfflineExpenses();

        const syncableDrafts = drafts.filter(d => d.syncState != "failedValidation");

        if (syncableDrafts.length === 0) return;

        await syncPendingReceiptDrafts(syncableDrafts);
        await syncPendingExpenseDrafts(syncableDrafts);

        await refreshTripDetails();// no update of offline ui cause its done on page refresh.
        //await renderPendingExpensesForCurrentTrip();
        await refreshOfflineUi();
    }
    finally {
        syncInProgress = false;
        releaseSyncLock();
    }
}

async function syncPendingDraftsIfOnline() {
    if (!ServerStatus.isAvailable) {
        return;
    }
    await syncDrafts();
}

async function syncPendingExpenseDrafts(drafts) {

        const pendingDrafts = drafts.filter(d => d.syncState === "pendingCreate");

        if (pendingDrafts.length === 0) {
            return;
        }

        for (const draft of pendingDrafts) {
            const formData = new FormData();

            for (const [key, value] of draft.fields) {
                formData.append(key, value);
            }

            formData.append("IsOfflineSync", "true");
            replaceAntiforgeryToken(formData);

            const response = await fetch(draft.url, {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                await deleteOfflineExpense(draft.localId);
                notifyOfflineExpensesChanged();
            } else {
                const errorData = await response.json();
                draft.syncState = "failedValidation";
                draft.validationErrors = errorData.errors;
                draft.errorMessage = `Sync failed with status ${response.status}`;
                await updateOfflineExpense(draft);
                continue;
            }
        }
}

async function syncPendingReceiptDrafts(drafts) {

    const receiptDrafts = drafts.filter(d =>
        d.syncState === "pendingReceiptParse"
    );

    for (const draft of receiptDrafts) {
        await syncReceiptDraft(draft);
    }
}

async function syncReceiptDraft(draft) {
    console.log("Sync receipt draft:", draft.localId);

    const formData = new FormData();

    formData.append(
        "receiptImage",
        draft.receipt.blob,
        draft.receipt.name || "receipt.jpg"
    );

    formData.append("IsOfflineSync", "true");
    //formData.append("tripId", draft.tripId);
    replaceAntiforgeryToken(formData);

    const response = await fetch("/Expenses/ParseReceiptJson", {
        method: "POST",
        body: formData
    });

    console.log("Receipt parse response:", response.status);

    if (!response.ok) {
        draft.syncState = "failedValidation";
        draft.validationErrors = ["Receipt could not be parsed."];
        await updateOfflineExpense(draft);
        return;
    }

    const parsed = await response.json();

    applyParsedReceiptToFields(draft, parsed);
    draft.display = createDisplayFromFields(draft.fields);
    draft.syncState = "failedValidation";
    //draft.validationErrors = ["Please review the parsed receipt."];
    await updateOfflineExpense(draft);
}

function applyParsedReceiptToFields(draft, parsed) {
    setField(draft.fields, "Name", parsed.name);
    setField(draft.fields, "Date", parsed.date);
    setField(draft.fields, "CurrencyCode", parsed.currencyCode);
    setField(draft.fields, "Category", parsed.category);
    setFirstPaidAmountField(draft.fields, parsed.totalAmount);
}

function setField(fields, name, value) {
    if (value === null || value === undefined) return;

    const existing = fields.find(([key]) => key === name);

    if (existing) {
        existing[1] = String(value);
    } else {
        fields.push([name, String(value)]);
    }
}

function setFirstPaidAmountField(fields, amount) {
    if (amount === null || amount === undefined) return;

    const paidAmountField = fields.find(([key]) =>
        key.endsWith(".PaidAmount")
    );

    if (paidAmountField) {
        paidAmountField[1] = String(amount);
    }
}

async function refreshTripDetails() {
    const target = document.querySelector("#tripDetails-target");

    if (!target) {
        return;
    }
    await htmx.ajax("GET", window.location.href, {
        target: "#tripDetails-target",
        select: "#tripDetails-target",
        swap: "outerHTML"
    });
}

function replaceAntiforgeryToken(formData) {
    const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");

    if (!tokenInput) {
        console.warn("No fresh antiforgery token found on page.");
        return;
    }

    formData.set("__RequestVerificationToken", tokenInput.value);
}

console.log("offline 7/10 - sync.js loaded");