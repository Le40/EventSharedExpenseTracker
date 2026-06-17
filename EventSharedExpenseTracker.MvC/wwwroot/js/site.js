
// UI/UX

// store html with data-restorable
document.body.addEventListener("htmx:beforeSwap", event => {
    const target = event.detail.target;

    if (!target?.matches("[data-restorable='true']")) {
        return;
    }
    console.log("saving", target);
    target.dataset.originalHtml = target.innerHTML;
});
// reload html with data-restorable
function restoreOriginal(button) {
    const target = button.closest("[data-restorable='true']");

    if (!target) {
        return;
    }
    console.log("restoring", target);
    target.innerHTML = target.dataset.originalHtml || "";
}
// prevents htms messages from firing offline
document.body.addEventListener("htmx:beforeRequest", event => {
    const button = event.target.closest("[data-requires-online='true']");

    if (!button) {
        return;
    }

    if (navigator.onLine) {
        return;
    }

    event.preventDefault();
    event.stopPropagation();

    //alert("This action needs an internet connection.");
    Toast.show("This action needs an internet connection.", "info");
}, true);

// TRIP FORM so date to is set to the same as datefrom as default choice.
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



console.log("site.js loaded");


