let deferredInstallPrompt = null;
// INSTALL
window.addEventListener("beforeinstallprompt", event => {
    if (window.App.isPwa()) {
        return;
    }

    event.preventDefault();
    deferredInstallPrompt = event;

    const button = document.querySelector("#installAppButton");
    button?.classList.remove("d-none");
});

document.addEventListener("click", async event => {
    const button = event.target.closest("#installAppButton");

    if (!button || !deferredInstallPrompt) {
        return;
    }

    deferredInstallPrompt.prompt();

    const choice = await deferredInstallPrompt.userChoice;

    if (choice.outcome === "accepted") {
        button.classList.add("d-none");
    }

    deferredInstallPrompt = null;
});

console.log("pwa 2/2 - install.js loaded");