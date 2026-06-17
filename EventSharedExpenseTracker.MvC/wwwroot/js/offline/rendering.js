
// RENDER all pending
async function renderPendingExpensesForCurrentTrip() {
    // ai suggested to make sure there are no double cards or something.
    removeOfflineDraftDom();
    // remove previous ones - cause htmx, aferswap will cause massive duplicaion
    document.querySelectorAll("[data-pending-expense-card='true']")
        .forEach(card => card.remove());

    const expenseList = document.querySelector("[data-expense-list='true']");

    if (!expenseList) {
        return;
    }

    const tripId = Number(expenseList.dataset.tripId);

    if (!tripId) {
        return;
    }

    const drafts = await getAllOfflineExpenses();
    const tripDrafts = drafts.filter(d =>
        Number(d.tripId) === tripId &&
        (d.syncState === "pendingCreate" || d.syncState === "failedValidation")
    );

    for (const draft of tripDrafts) {
        addPendingExpenseCard(draft);
    }
}

// RENDER one card
function addPendingExpenseCard(draft) {
    if (document.querySelector(`[data-offline-draft-id='${draft.localId}']`)) {
        return;
    }
    const templateCard = document.querySelector("[data-expense-card='true']");

    if (!templateCard) {
        return;
    }

    const pendingCard = templateCard.closest("li").cloneNode(true);

    pendingCard.removeAttribute("id");
    // remove all ids for cloned card, cause there is a target for edits of other expenses.
    pendingCard.querySelectorAll("[id]").forEach(element => {
        element.removeAttribute("id");
    });

    pendingCard.classList.remove("border-warning", "border-danger", "opacity-75");

    const paidBy = pendingCard.querySelector("[data-expense-paid-by]");

    if (draft.syncState === "failedValidation") {
        pendingCard.classList.add("border", "border-danger", "opacity-75");
        if (paidBy) { paidBy.textContent = "Validation failed"; }
    } else {
        pendingCard.classList.add("border", "border-warning", "opacity-75");
        if (paidBy) { paidBy.textContent = "Pending sync"; }
    }

    const category = pendingCard.querySelector("[data-expense-category]");
    const name = pendingCard.querySelector("[data-expense-name]");
    const date = pendingCard.querySelector("[data-expense-date]");
    const amount = pendingCard.querySelector("[data-expense-amount]");

    if (name) { name.textContent = draft.name || "Unnamed expense"; }
    if (category) { category.textContent = draft.category || "Pending"; }
    if (date) { date.textContent = draft.formattedDate || "Pending"; }
    if (amount) { amount.textContent = draft.formattedAmount || "Pending"; }

    const owedCount = pendingCard.querySelector("[data-expense-owed-count]");
    if (owedCount) {
        owedCount.textContent = "not included in balances yet";
    }

    // Add localId to Pending Card - for edit
    pendingCard.dataset.offlineDraftId = draft.localId;
    pendingCard.dataset.pendingExpenseCard = "true";

    const createRow = document.querySelector("[data-expense-create-row='true']");
    createRow?.insertAdjacentElement("afterend", pendingCard);
}

// RENDER validation errors
function renderOfflineValidationErrors(form, validationErrors) {
    if (!validationErrors) {
        return;
    }

    const messages = [];

    for (const [field, errors] of Object.entries(validationErrors)) {
        for (const error of errors) {
            messages.push(error);
        }
    }

    if (messages.length === 0) {
        return;
    }

    const alert = document.createElement("div");
    alert.className = "alert alert-danger";
    alert.dataset.offlineValidationErrors = "true";
    alert.innerHTML = messages.map(m => `<div>${m}</div>`).join("");

    form.prepend(alert);
}

// navbar counter
async function updatePendingSyncUi() {

    const drafts = await getAllOfflineExpenses();

    const pendingCount = drafts.filter(d =>
        d.syncState === "pendingCreate"
    ).length;

    const failedCount = drafts.filter(d =>
        d.syncState === "failedValidation"
    ).length;

    const pendingBadge = document.getElementById("pendingSyncBadge");
    const failedBadge = document.getElementById("failedValidationBadge");
    const syncButton = document.getElementById("syncNowButton");

    if (!pendingBadge || !failedBadge || !syncButton) {
        return;
    }

    pendingBadge.classList.toggle("d-none", pendingCount === 0);
    failedBadge.classList.toggle("d-none", failedCount === 0);

    const showSyncButton = navigator.onLine && pendingCount > 0;
    syncButton.classList.toggle("d-none", !showSyncButton);

    pendingBadge.textContent = `${pendingCount} Pending`;
    failedBadge.textContent = `${failedCount} Need review`;
}

// connection status in navbar
async function updateConnectionStatus() {
    const status = document.getElementById("connectionStatus");

    if (!status) {
        return;
    }

    await updatePendingSyncUi();

    const online = navigator.onLine;

    // ai receript parsing not available offline.
    const uploadReceiptButton = document.getElementById("receiptUploadButton");
    if (uploadReceiptButton) {
        uploadReceiptButton.classList.toggle("d-none", !online);
    }

    status.classList.toggle("d-none", online);
}

function removeOfflineDraftDom() {
    document
        .querySelectorAll("[data-pending-expense-card='true'], [data-offline-draft-id]")
        .forEach(element => element.remove());
}
console.log("offline 2/5 - rendering.js loaded");