
// SAVE EDIT LISTENER
document.body.addEventListener("click", async event => {
    // get button that called
    const button = event.target.closest("button[type='submit']");
    if (!button) {
        return;
    }
    const form = button.closest("form");
    if (!form?.dataset.offlineDraftId) {
        return; // do not prevent normal create
    }
    event.preventDefault();
    event.stopPropagation();

    await handleOfflineDraftEdit(button, form);
});

// SAVE CREATE LISTENER
// listener tries to to its htmx call, if it is fail, it goes here.
// offine every draft is created from createForm.
// so only those should have active save button offline.
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

    await handleOfflineDraftSave(button, form);
});


// EDIT DBLCLICK LISTENER
document.body.addEventListener("dblclick", async event => {
    const card = event.target.closest("[data-pending-expense-card='true']");

    if (!card) {
        return;
    }

    if (card.querySelector("form")) {
        return;
    }

    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();

    const localId = Number(card.dataset.offlineDraftId);

    if (!localId) {
        return;
    }

    await openOfflineDraftForEdit(localId, card);
}, true ); // true at the end makes it run before any htmx, so it should prevent any other dblclick on draft form from firing.

// DELETE LISTENER
document.body.addEventListener("click", async event => {
    const button = event.target.closest("[data-offline-draft-delete='true']");

    if (!button) {
        return;
    }

    event.preventDefault();
    event.stopPropagation();

    await handleOfflineDraftDelete(button);
});



// RENDERING LISTENERS
// page refresh
document.addEventListener("DOMContentLoaded", async () => {
    await renderPendingExpensesForCurrentTrip();
});

// to render pending drafts online after htmx swap or update
document.body.addEventListener("htmx:afterSwap", async event => {
    const target = event.detail.target;

    const expenseListWasReloaded =
        target?.matches("[data-expense-list='true']") ||
        target?.querySelector?.("[data-expense-list='true']");

    if (!expenseListWasReloaded)
        return;

    await renderPendingExpensesForCurrentTrip();
});



// SYNC LISTENERS
// back online
window.addEventListener("online", async () => {
    await syncPendingExpenseDrafts();
});
// page refresh
document.addEventListener("DOMContentLoaded", async () => {
    if (navigator.onLine) {
        await syncPendingExpenseDrafts();
    }
});
// pending sync count
document.addEventListener("DOMContentLoaded", async () => {
    await updatePendingSyncUi();
});
// manual sync now button
document.addEventListener("click", async event => {
    const button = event.target.closest("#syncNowButton");

    if (!button) {
        return;
    }
    // anti spam click
    button.disabled = true;

    try {
        await syncPendingExpenseDrafts();
    }
    finally {
        button.disabled = false;
    }
});


console.log("offline 5/5 - listeners.js loaded");