/*function isMobileView() {
    return window.matchMedia("(max-width: 767.98px)").matches;
}

let activeSearchInput = null;
//let hasScrolledForCurrentSearch = false;

document.addEventListener("focusin", function (e) {
    const input = e.target.closest("[data-scroll-target-id]");
    if (!input) return;

    activeSearchInput = input;
    hasScrolledForCurrentSearch = false;
});

document.body.addEventListener("htmx:afterSwap", function () {
    if (!activeSearchInput) return;
    if (!isMobileView()) return;
    //if (hasScrolledForCurrentSearch) return;

    const targetId = activeSearchInput.dataset.scrollTargetId;
    const target = document.getElementById(targetId);
    if (!target) return;

    setTimeout(() => {
        const rect = target.getBoundingClientRect();
        const viewportTop = window.visualViewport?.offsetTop ?? 0;

        const desiredTop =
            window.scrollY + rect.top - viewportTop;

        window.scrollTo({
            top: Math.max(0, desiredTop),
            behavior: "smooth"
        });

        //hasScrolledForCurrentSearch = true;
    }, 250);
});*/

function isMobileView() {
    return window.matchMedia("(max-width: 767.98px)").matches;
}

function isElementFullyInView(element) {
    const rect = element.getBoundingClientRect();

    const viewportTop = window.visualViewport?.offsetTop ?? 0;
    const viewportHeight = window.visualViewport?.height ?? window.innerHeight;
    const viewportBottom = viewportTop + viewportHeight;

    return rect.top >= viewportTop && rect.bottom <= viewportBottom;
}

function getFirstVisibleItem(collection) {
    return collection.querySelector(
        ".list-group-item, .card, [data-list-item]"
    );
}

let activeSearchInput = null;

document.addEventListener("focusin", function (e) {
    const input = e.target.closest("[data-scroll-target-id]");
    if (!input) return;

    activeSearchInput = input;
});

document.body.addEventListener("htmx:afterSwap", function () {
    if (!activeSearchInput) return;
    if (!isMobileView()) return;

    const targetId = activeSearchInput.dataset.scrollTargetId;
    const target = document.getElementById(targetId);
    if (!target) return;

    setTimeout(() => {
        const firstItem = getFirstVisibleItem(target);

        if (!firstItem) return;

        if (isElementFullyInView(firstItem)) {
            return;
        }

        const rect = firstItem.getBoundingClientRect();
        const viewportTop = window.visualViewport?.offsetTop ?? 0;

        const desiredTop =
            window.scrollY + rect.top - viewportTop -40;

        window.scrollTo({
            top: Math.max(0, desiredTop),
            behavior: "smooth"
        });
    }, 250);
});
// BOTTOM SHEET
document.addEventListener("click", function (e) {
    if (e.target.closest("[data-open-bottom-sheet]")) {
        document.getElementById("appBottomSheet")?.classList.add("is-open");
    }

    if (e.target.closest("[data-close-bottom-sheet]")) {
        document.getElementById("appBottomSheet")?.classList.remove("is-open");
    }
});
// OPEN OFFCANVAS AFTER SWAP
document.body.addEventListener("htmx:afterSwap", function (e) {
    if (e.detail.target.id !== "appOffCanvasBody") return;

    bootstrap.Offcanvas
        .getOrCreateInstance(document.getElementById("appOffcanvas"))
        .show();
});
// dial button
document.getElementById("fabMain")
    .addEventListener("click", function () {
        document
            .querySelector(".fab-container")
            .classList.toggle("open");
    });

console.log("ux.js loaded");