// ---------------------------------------------------------------------------
// CANCEL BUTTON NOT MAKING CALLS TO SERVER
// ---------------------------------------------------------------------------

// UI/UX
const restoreCache = new Map();

// STORE HTML IN CACHE if data-restorable=true
// mainly for CANCEL, so one can cancel without call to server
// primarily used for expense cards, trip edits and so on.
///// WHEN ALL IN OFFCANVAS = NOT NEEDED
document.body.addEventListener("htmx:beforeRequest", event => {
    const target = event.detail.target;

    if (!target?.matches("[data-restorable='true']")) {
        return;
    }
    console.log("saving", target);
    //target.dataset.originalHtml = target.innerHTML;

    // storing in the cache at first entry, so send back validation errors from server dont affect stored version.
    if (!restoreCache.has(target.id)) {
        restoreCache.set(
            target.id,
            target.innerHTML
        );
    }
});
// Handles weather CANCEL button should close offcanvas or restore original.
function handleCancel(button) {
    const offcanvasEl = button.closest(".offcanvas");

    // replaces code on cancel button, only while old style cancel is still used.
    if (offcanvasEl) {
        hideAppOffcanvas();
        return;
    }
    restoreOriginal(button);
}
// Restore original/beforeswap content on CANCEL button.
function restoreOriginal(button) {
    const target = button.closest("[data-restorable='true']");

    if (!target) {
        return;
    }
    console.log("restoring", target);
    //target.innerHTML = target.dataset.originalHtml || "";
    target.innerHTML = restoreCache.get(target.id);
    htmx.process(target); // to make 'click once' able to fire again
    restoreCache.delete(target.id);
}

// ---------------------------------------------------------------------------
// TRIP FORM - dateFrom and dateTo not too far from each other
// ---------------------------------------------------------------------------
document.body.addEventListener("change", event => {
    const dateFrom = event.target.closest("[data-trip-date-from='true']");

    if (!dateFrom) {
        return;
    }

    const form = dateFrom.closest("form");
    const dateTo = form?.querySelector("[data-trip-date-to='true']");

    if (!dateTo) {
        return;
    }

    if (!dateTo.value) {
        dateTo.value = dateFrom.value;
        return;
    }

    const from = new Date(dateFrom.value);
    const to = new Date(dateTo.value);

    const diffDays = (to - from) / (1000 * 60 * 60 * 24);

    if (to < from || diffDays > 90) {
        dateTo.value = dateFrom.value;
    }
});


// ---------------------------------------------------------------------------
// PENDING BADGE CLICK REDIRECTS TO TRIP WITH MOST PENDING EXPENSES.
// ---------------------------------------------------------------------------
document
    .getElementById("failedValidationBadge")
    ?.addEventListener("click", handlePendingExpensesClick);

document
    .getElementById("pendingSyncBadge")
    ?.addEventListener("click", handlePendingExpensesClick);

async function handlePendingExpensesClick() {
    const tripSummary = await getTripWithMostOfflineExpenses();

    if (!tripSummary) {
        return;
    }

    const targetTripId = tripSummary.tripId;
    const currentTripId = getCurrentTripId();

    if (currentTripId === targetTripId) {
        scrollToPendingExpenses();
        return;
    }

    if (!ServerStatus.isAvailable) {
        const cachedTripUrl = `/Trips/Details/${targetTripId}`;

        const cachedResponse = await caches.match(cachedTripUrl);

        if (cachedResponse) {
            window.location.href = cachedTripUrl + "?focus=pending";
            return;
        }

        // no cached trip available
        Toast.show("This trip is not available offline yet.", "info");
        return;
    }

    window.location.href = `/Trips/Details/${targetTripId}?focus=pending`;
}

function getCurrentTripId() {
    const page = document.getElementById("tripDetailsPage");

    if (!page)
        return null;

    return Number(page.dataset.tripId);
}

// ---------------------------------------------------------------------------
// EXPENSE FORM - check if user selected category while ai was guessing it
// ---------------------------------------------------------------------------
document.body.addEventListener("htmx:beforeSwap", function (e) {
    const target = e.target;

    if (!target.id.startsWith("category-select-"))
        return;

    // User already selected a category
    if (target.value) {
        e.detail.shouldSwap = false;
    }
});

console.log("site.js loaded");


