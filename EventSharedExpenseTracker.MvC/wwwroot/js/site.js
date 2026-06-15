
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
document.body.addEventListener("click", event => {
    const button = event.target.closest("[data-requires-online='true']");

    if (!button) {
        return;
    }

    if (navigator.onLine) {
        return;
    }

    event.preventDefault();
    event.stopPropagation();

    alert("This action needs an internet connection.");
}, true);

console.log("site.js loaded");


