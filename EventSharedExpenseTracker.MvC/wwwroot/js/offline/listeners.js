
// PWA LISTENERS
// detect online / offline
window.addEventListener("offline", () => {
    console.log("Offline");
});

window.addEventListener("online", () => {
    console.log("Online");
});

// detect PWA
document.addEventListener("DOMContentLoaded", () => {

    const isPwa =
        window.matchMedia("(display-mode: standalone)").matches ||
        window.navigator.standalone === true;

    if (isPwa) {
        document.body.classList.add("pwa");
    }
});

// status helper
window.App = {
    isPwa: () =>
        window.matchMedia("(display-mode: standalone)").matches ||
        window.navigator.standalone === true
};



// SAVE EDIT LISTENER
document.body.addEventListener("click", async event => {
    // get button that called
    const button = event.target.closest("[data-offline-expense-save='true']");
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
document.body.addEventListener("htmx:sendError", async event => {
    // get button that called htmx
    const button = event.detail.elt;
    if (button.dataset.offlineExpenseSave !== "true") {
        return;
    }
    event.preventDefault();

    await handleOfflineDraftSave(button);
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