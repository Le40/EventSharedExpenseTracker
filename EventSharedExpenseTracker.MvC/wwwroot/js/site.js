
// UI/UX
document.body.addEventListener("htmx:beforeSwap", event => {
    const target = event.detail.target;

    if (!target?.matches("[data-restorable='true']")) {
        return;
    }
    console.log("saving", target);
    target.dataset.originalHtml = target.innerHTML;
});

function restoreOriginal(button) {
    const target = button.closest("[data-restorable='true']");

    if (!target) {
        return;
    }
    console.log("restoring", target);
    target.innerHTML = target.dataset.originalHtml || "";
}
