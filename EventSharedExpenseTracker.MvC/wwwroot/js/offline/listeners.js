// ---------------------------------------------------------------------------
// HTMX: beforeRequest
// ---------------------------------------------------------------------------

// BLOCKING OFFLINE HTMX CALLS THAT REQUIRES SERVER
document.body.addEventListener("htmx:beforeRequest", event => {
    const trigger = event.detail.elt;
    const requiresServer = trigger?.closest("[data-requires-server='true']");
    const isSilent = trigger?.closest("[data-offline-silent='true']");

    if (!requiresServer) return;
    if (ServerStatus.isAvailable) return;

    event.preventDefault();
    //event.stopPropagation();

    if (!isSilent)
        Toast.show("This action requires an internet connection.", "info");
}, true); // to be run before other htmx:beforeReuest

// SAVE CREATE LISTENER
// offine every draft is created from createForm.
// so only those should have active save button offline.
document.body.addEventListener("htmx:beforeRequest", async event => {
    const button = event.detail.elt;
    const form = button.closest("form");

    const formId = form?.querySelector("[name='FormId']")?.value;
    const isCreateForm = formId === "expense-createForm";
    const isSubmitButton = button.matches("button[type='submit']");

    if (!isCreateForm || !isSubmitButton) return;
    if (ServerStatus.isAvailable) return;

    event.preventDefault();

    await handleExpenseDraftCreate(button, form);
});

// PREVENTING RECEIPT UPLOAD WHEN OFFLINE
document.body.addEventListener("htmx:beforeRequest", function (event) {
    const trigger = event.detail.elt;

    if (trigger?.id !== "receipt-upload") return;
    if (ServerStatus.isAvailable) return;

    event.preventDefault();

    console.log("Offline - preventing receipt upload.");
});

// ---------------------------------------------------------------------------
// HTMX: aferSwap
// ---------------------------------------------------------------------------
/*// DELETE: maybe no longer needed, now i should be doing full page reloads, after create or edit, and there drafts are rendered.
// RENDER PENDING DRAFTS ONLINE - after every swap into page.
document.body.addEventListener("htmx:afterSwap", async event => {
    console.log("afterSwap", event.detail.target);

    const target = event.detail.target;

    const expenseListWasReloaded =
        target?.matches("[data-expense-list='true']") ||
        target?.querySelector?.("[data-expense-list='true']");

    console.log("expenseListWasReloaded", expenseListWasReloaded);

    if (!expenseListWasReloaded)
        return;

    await renderPendingExpensesForCurrentTrip();
});*/

// ---------------------------------------------------------------------------
// CLICK LISTENERS
// ---------------------------------------------------------------------------

// EDIT LISTENER
document.body.addEventListener("click", async event => {
    const card = event.target.closest("[data-pending-expense-card='true']");

    if (!card) return;
    if (card.querySelector("form")) return;

    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();

    const localId = Number(card.dataset.offlineDraftId);

    if (!localId) return;

    await openOfflineDraftForEdit(localId, card);
}, true); // true at the end makes it run before any htmx, so it should prevent any other click on draft form from firing.

// SAVE EDIT LISTENER
document.body.addEventListener("click", async event => {
    // get button that called
    const button = event.target.closest("button[type='submit']");

    if (!button) return;
    const form = button.closest("form");

    if (!form?.dataset.offlineDraftId) return; // do not prevent normal create

    event.preventDefault();
    event.stopPropagation();

    await handleExpenseDraftEdit(button, form);
});

// DELETE LISTENER
document.body.addEventListener("click", async event => {
    const button = event.target.closest("[data-offline-draft-delete='true']");

    if (!button) return;

    event.preventDefault();
    event.stopPropagation();

    await handleExpenseDraftDelete(button);
});

// SYNC: MANUAL SYNC BUTTON
// rest of the sync is in server-status.js on DOMReload, Offline and Online listeners
document.addEventListener("click", async event => {
    const button = event.target.closest("#syncNowButton");

    if (!button) return;
    button.disabled = true;  // anti spam click

    try {
        await syncDrafts();
    }
    finally {
        button.disabled = false;
    }
});



/*// FOR DELETE: if all works later, delete this.
// SAVE CREATE LISTENER - old?
document.body.addEventListener("htmx:sendError", async event => {
    const button = event.detail.elt;
    const form = button.closest("form");

    const formId = form?.querySelector("[name='FormId']")?.value;
    const isCreateForm = formId === "expense-createForm";
    const isSubmitButton = button.matches("button[type='submit']");

    if (!isCreateForm || !isSubmitButton) {
        return;
    }

    event.preventDefault();

    await handleExpenseDraftCreate(button, form);
});*/














// OFFLINE DRAFTS CHANGED IN OTHER OPEN TAB
// update this one too, so no stale states.
window.addEventListener("offlineExpensesChanged", async () => {
    console.log("offlineExpensesChanged listener fired");

    await renderPendingExpensesForCurrentTrip();
    await updatePendingSyncUi();
});





console.log("offline 8/10 - listeners.js loaded");