
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

    if (formId !== "expense-createForm") {
        return;
    }

    event.preventDefault();

    await handleOfflineDraftSave(button, form);
});

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