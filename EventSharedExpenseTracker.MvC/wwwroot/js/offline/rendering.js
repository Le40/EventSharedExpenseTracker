// ---------------------------------------------------------------------------
// RENDERING
// ---------------------------------------------------------------------------

// RENDER ALL
async function renderPendingExpensesForCurrentTrip() {
    removeOfflineDraftDom();

    const expenseList = document.querySelector("[data-expense-list='true']");
    if (!expenseList) return;

    const tripId = Number(expenseList.dataset.tripId);
    const tripDrafts = await getOfflineExpensesForTrip(tripId);

    for (const draft of tripDrafts) {
        addPendingExpenseCard(draft);
    }
}

// RENDER CARD
function addPendingExpenseCard(draft) {
    // does pending draft exist already
    if (document.querySelector(`[data-offline-draft-id='${draft.localId}']`)) {
        return;
    }

    const pendingCard = cloneExpenseCardTemplate();
    if (!pendingCard) return;

    // Remove collapse content
    const collapse = pendingCard.querySelector(".collapse");
    collapse?.remove();

    // Remove collapse trigger behavior
    const collapseTrigger = pendingCard.querySelector("[data-bs-toggle='collapse']");

    if (collapseTrigger) {
        collapseTrigger.removeAttribute("data-bs-toggle");
        collapseTrigger.removeAttribute("data-bs-target");
        collapseTrigger.removeAttribute("aria-expanded");
        collapseTrigger.removeAttribute("aria-controls");
    }

    // PREPARE PENDING CARD
    pendingCard.removeAttribute("id");

    /*pendingCard.querySelectorAll("[id]").forEach(element => {
        element.removeAttribute("id");
    });*/

    // REMOVE ONLINE HTMX EDIT FROM CLONED BUTTON
    const editButton = pendingCard.querySelector("[hx-get]");

    if (editButton) {
        editButton.removeAttribute("hx-get");
        editButton.removeAttribute("hx-target");
        editButton.removeAttribute("hx-trigger");
        editButton.removeAttribute("data-requires-server");

        editButton.dataset.offlineEdit = "true";
        editButton.dataset.offlineDraftId = draft.localId;
        editButton.textContent = "Edit";
    }

    pendingCard.classList.remove("border-warning", "border-danger", "opacity-75");

    // APPLY STATE
    const paidBy = pendingCard.querySelector("[data-expense-paid-by]");

    if (draft.syncState === "failedValidation") {
        pendingCard.classList.add("border", "border-danger", "opacity-75");
        if (paidBy) { paidBy.textContent = "Validation failed"; }
    } else {
        pendingCard.classList.add("border", "border-warning", "opacity-75");
        if (paidBy) { paidBy.textContent = "Pending sync"; }
    }

    // FILL CARD DISPLAY
    setText(pendingCard, "[data-expense-category]", draft.display?.category ?? "Pending")
    setText(pendingCard, "[data-expense-name]", draft.display?.name ?? "Unnamed expense")
    setText(pendingCard, "[data-expense-date]", draft.display?.formattedDate ?? "Pending")
    setText(pendingCard, "[data-expense-amount]", draft.display?.formattedAmount ?? "Pending")
    setText(pendingCard, "[data-expense-owed-count]", draft.display?.owedCount ?? "not included in balances yet")

    // MARK PENDING CARD
    pendingCard.dataset.offlineDraftId = draft.localId;
    pendingCard.dataset.pendingExpenseCard = "true";

    // INSERT PENDING CARD
    const expenseList = document.querySelector("[data-expense-list='true']");

    if (!expenseList) return;
    expenseList.prepend(pendingCard);
}

function cloneExpenseCardTemplate() {
    const templateCard = document.querySelector("[data-expense-card='true']");
    const li = templateCard?.closest("li");

    return li?.cloneNode(true) ?? null;
}

function setText(parent, selector, value) {
    const element = parent.querySelector(selector);
    if (element) element.textContent = value;
}

// RENDER validation errors
function renderValidationErrors(form, validationErrors) {
    if (!validationErrors) return;

    const messages = [];
    for (const [field, errors] of Object.entries(validationErrors)) {
        for (const error of errors) {
            messages.push(error);
        }
    }

    if (messages.length === 0)  return;

    const alert = document.createElement("div");
    alert.className = "alert alert-danger";
    alert.dataset.offlineValidationErrors = "true";
    alert.innerHTML = messages.map(m => `<div>${m}</div>`).join("");

    form.prepend(alert);
}

// NAVBAR PENDING COUNTER BADGE
async function updatePendingSyncUi() {

    const drafts = await getAllOfflineExpenses();

    const pendingCount = drafts.filter(d =>
        d.syncState === "pendingCreate" || d.syncState === "pendingReceiptParse"
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

    const showSyncButton = ServerStatus.isAvailable && pendingCount > 0;
    syncButton.classList.toggle("d-none", !showSyncButton);

    pendingBadge.textContent = `${pendingCount} Pending`;
    failedBadge.textContent = `${failedCount} Need review`;
}

// NAVBAR CONNECTION BADGE
async function updateConnectionStatus() {
    const status = document.getElementById("connectionStatus");

    if (!status) return;

    await updatePendingSyncUi();

    const online = ServerStatus.isAvailable;

    status.classList.toggle("d-none", online);
}

// REMOVE DRAFTS FROM DOM
function removeOfflineDraftDom() {
    document
        .querySelectorAll("[data-pending-expense-card='true'], [data-offline-draft-id]")
        .forEach(element => element.remove());
}
console.log("offline 6/10 - rendering.js loaded");