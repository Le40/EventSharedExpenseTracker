
// RENDER all pending
async function renderPendingExpensesForCurrentTrip() {

    console.log("renderPendingExpensesForCurrentTrip");

    removeOfflineDraftDom();

    const expenseList = document.querySelector("[data-expense-list='true']");
    console.log("expenseList", expenseList);

    if (!expenseList) return;

    const tripId = Number(expenseList.dataset.tripId);
    console.log("tripId", tripId);

    const tripDrafts = await getOfflineExpensesForTrip(tripId);

    console.log("tripDrafts", tripDrafts);

    for (const draft of tripDrafts) {
        addPendingExpenseCard(draft);
    }
}

// RENDER one card
function addPendingExpenseCard(draft) {

    console.log("=== addPendingExpenseCard ===");
    console.log("draft", draft);

    if (document.querySelector(`[data-offline-draft-id='${draft.localId}']`)) {
        console.log("SKIP: card already exists");
        return;
    }

    const templateCard = document.querySelector("[data-expense-card='true']");
    console.log("templateCard", templateCard);

    if (!templateCard) {
        console.log("SKIP: templateCard not found");
        return;
    }

    const li = templateCard.closest("li");
    console.log("closest li", li);

    if (!li) {
        console.log("SKIP: templateCard has no parent li");
        return;
    }

    const pendingCard = li.cloneNode(true);
    console.log("pendingCard cloned", pendingCard);

    pendingCard.removeAttribute("id");

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

    console.log("category", category);
    console.log("name", name);
    console.log("date", date);
    console.log("amount", amount);

    if (name) { name.textContent = draft.display.name || "Unnamed expense"; }
    if (category) { category.textContent = draft.display.category || "Pending"; }
    if (date) { date.textContent = draft.display.formattedDate || "Pending"; }
    if (amount) { amount.textContent = draft.display.formattedAmount || "Pending"; }

    const owedCount = pendingCard.querySelector("[data-expense-owed-count]");

    if (owedCount) {
        owedCount.textContent = "not included in balances yet";
    }

    pendingCard.dataset.offlineDraftId = draft.localId;
    pendingCard.dataset.pendingExpenseCard = "true";

    const expenseList = document.querySelector("[data-expense-list='true']");

    if (!expenseList) {
        console.log("SKIP: expenseList not found");
        return;
    }

    expenseList.prepend(pendingCard);

    console.log("INSERTED");
    console.log(
        "pending cards in DOM",
        document.querySelectorAll("[data-pending-expense-card='true']").length
    );
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
        d.syncState === "pendingCreate" || d.syncState === "pendingReceiptParse"
    ).length;

    const failedCount = drafts.filter(d =>
        d.syncState === "failedValidation"
    ).length;

    const receipts = await getAllOfflineReceipts();

    const pendingBadge = document.getElementById("pendingSyncBadge");
    const failedBadge = document.getElementById("failedValidationBadge");
    const syncButton = document.getElementById("syncNowButton");

    if (!pendingBadge || !failedBadge || !syncButton) {
        return;
    }

    pendingBadge.classList.toggle("d-none", pendingCount === 0);
    failedBadge.classList.toggle("d-none", failedCount === 0);

    const showSyncButton = ServerStatus.isAvailable && pendingCount > 0;
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

    const online = ServerStatus.isAvailable;

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
console.log("offline 4/10 - rendering.js loaded");