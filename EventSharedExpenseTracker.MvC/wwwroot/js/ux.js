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

function showAppOffcanvas() {
    getAppOffcanvas().show();
}

function hideAppOffcanvas() {
    getAppOffcanvas().hide();
}

function getAppOffcanvas() {
    const isMobile = window.matchMedia("(max-width: 991.98px)").matches;

    return bootstrap.Offcanvas.getOrCreateInstance(
        document.getElementById("appOffcanvas"),
        {
            backdrop: isMobile ? "static" : true,
            keyboard: !isMobile
        });
}


// UPDATES SITE HEADER HEIGHT FOR CSS CONTROLS
updateHeaderHeight(); // call once to get base
window.addEventListener('resize', updateHeaderHeight);
function updateHeaderHeight() {
    const header = document.getElementById('site-header');

    document.documentElement.style.setProperty(
        '--mobile-header-height',
        `${header.offsetHeight}px`
    );
}

function scrollToPendingExpenses() {
    document
        .getElementById("createExpense")
        ?.scrollIntoView({
            behavior: "smooth",
            block: "start"
        });
}

//floating controls with mobile keyboard shit.
function updateFloatingControlsForKeyboard() {
    if (!window.visualViewport) return;

    const keyboardOffset =
        window.innerHeight
        - window.visualViewport.height
        - window.visualViewport.offsetTop;

    document.documentElement.style.setProperty(
        "--keyboard-offset",
        `${Math.max(0, keyboardOffset)}px`
    );
}

if (window.visualViewport) {
    window.visualViewport.addEventListener("resize", updateFloatingControlsForKeyboard);
    window.visualViewport.addEventListener("scroll", updateFloatingControlsForKeyboard);
}

window.addEventListener("resize", updateFloatingControlsForKeyboard);

document.body.addEventListener("htmx:beforeRequest", e => {
    const title =
        e.detail.elt.dataset.offcanvasTitle;

    if (!title) return;

    document.getElementById("appOffcanvasTitle")
        .textContent = title;
});

console.log("ux.js loaded");