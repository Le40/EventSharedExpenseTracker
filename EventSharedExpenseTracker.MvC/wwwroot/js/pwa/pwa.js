
// PWA LISTENERS
// detect PWA
document.addEventListener("DOMContentLoaded", () => {
    if (window.App.isPwa()) {
        document.body.classList.add("pwa");
    }
});

// inject isPWA to login input, so if app then user is remembered automatically
document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("isPwaInput");

    if (!input) {
        return;
    }

    input.value = window.App.isPwa();
});

function isRunningAsPwa() {
    return window.matchMedia("(display-mode: standalone)").matches ||
        window.navigator.standalone === true;
}

// status helper
window.App = {
    isPwa: isRunningAsPwa
};


// NAVBAR LISTENERS
// pending sync count
document.addEventListener("click", async event => {
    const button = event.target.closest("#syncNowButton");

    if (!button) {
        return;
    }

    await syncPendingExpenseDrafts()
});

// offline/online
document.addEventListener("DOMContentLoaded", updateConnectionStatus);

// detect online / offline
window.addEventListener("online", updateConnectionStatus);
window.addEventListener("offline", updateConnectionStatus);

console.log("pwa 1/2 - pwa.js loaded");
