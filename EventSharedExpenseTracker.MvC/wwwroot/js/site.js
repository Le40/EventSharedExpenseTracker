function isOffline() {
    return !navigator.onLine;
}

// UI/UX
const restoreCache = new Map();


// store html with data-restorable
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
// reload html with data-restorable
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



/*// prevents htmx from firing offline
document.body.addEventListener("htmx:beforeRequest", event => {

    if (!isOffline)
        return;

    const action = event.target.closest("[data-offline-capable='true']");

    if (action)
        return;

    event.preventDefault();
    event.stopPropagation();

    Toast.show("This action requires an internet connection.", "info");

}, true);*/

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
    Toast.show("This action requires an internet connection.", "info");
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
// UPDATES SITE HEADER HEIGHT FOR CSS CONTROLS
function updateHeaderHeight() {
    const header = document.getElementById('site-header');

    document.documentElement.style.setProperty(
        '--mobile-header-height',
        `${header.offsetHeight}px`
    );
}

updateHeaderHeight();

window.addEventListener('resize', updateHeaderHeight);



console.log("site.js loaded");


