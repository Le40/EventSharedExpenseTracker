// SERVICE WORKER has set timeout, if networkFirst request take more than that, it will set server as unavailable and broadcast it to pages.
// First page to intercept it, stores it in storage as "server-is-available" = false.
// Page will then starts monitoring the server health, by pinging /health endpoint in set interval
// when it detects server is back online, responsive, it will set server as available and broadcast it to SERVICE WORKER, it will also change the value in storage.
// Storage used, so new page can know on load if server is available or not.

const MSG_SERVER_AVAILABLE = "SERVER_AVAILABLE";
const MSG_SERVER_UNAVAILABLE = "SERVER_UNAVAILABLE";
const serverHealthCheckIntervalMs = 15000;
const serverHealthTimeoutMs = 1500;
let serverHealthIntervalId = null; // to start and stop the same timer

// SERVER STATUS
const ServerStatus = {
    isAvailable: navigator.onLine,
    lastChangedAt: null
};

// LISTEN to service worker for when server is unavailable.
navigator.serviceWorker?.addEventListener("message", event => {

    if (event.data?.type !== MSG_SERVER_UNAVAILABLE)
        return;

    setServerAvailable(false);
});

// SEND message to service worker that server is available again.
function notifyServiceWorkerServerAvailable() {
    if (!navigator.serviceWorker?.controller) {
        return;
    }

    navigator.serviceWorker.controller.postMessage({
        type: MSG_SERVER_AVAILABLE
    });
}

// SERVER STATE MANAGER
function setServerAvailable(value) {
    if (ServerStatus.isAvailable === value) {
        return;
    }

    ServerStatus.isAvailable = value;
    ServerStatus.lastChangedAt = Date.now();

    console.log("Server available:", value);

    localStorage.setItem("server-is-available", String(value));

    if (value) {
        notifyServiceWorkerServerAvailable();
        stopServerHealthPolling();
        syncPendingDraftsIfOnline();
    } else {
        startServerHealthPolling();
    }
}

// SERVER HEALTH CHECK
async function checkServerHealth(timeoutMs = serverHealthTimeoutMs) {
    if (!navigator.onLine) {
        setServerAvailable(false);
        return false;
    }

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

    try {
        const response = await fetch("/health/ping", {
            method: "GET",
            cache: "no-store",
            signal: controller.signal
        });

        setServerAvailable(response.ok);
        return response.ok;
    } catch {
        setServerAvailable(false);
        return false;
    } finally {
        clearTimeout(timeoutId);
    }
}

// SERVER POLLING - START
function startServerHealthPolling() {
    // if there is monitoring already -> exit
    if (serverHealthIntervalId) {
        return;
    }

    serverHealthIntervalId = setInterval(async () => {
        await checkServerHealth();
    }, serverHealthCheckIntervalMs);
}

// SERVER POLLING - STOP
function stopServerHealthPolling() {
    // if there isnt monitoring already -> exit
    if (!serverHealthIntervalId) {
        return;
    }

    clearInterval(serverHealthIntervalId);
    serverHealthIntervalId = null;
}


// OFFLINE GLOBAL PAGE LOAD LISTENER
document.addEventListener("DOMContentLoaded", async () => {

    await updatePendingSyncUi();

    const storedServerStatus = localStorage.getItem("server-is-available");
    // if stored value is that server is unavailable, set the on startup true for serverStatus.isAvailable to false.
    // so page has current state of server.
    if (storedServerStatus === "false") {
        setServerAvailable(false);
        return;
    }

    await syncPendingExpenseDrafts();
});

// OFFLINE GLOBAL ONLINE LISTENER
window.addEventListener("online", async () => {
    console.log("Browser online");

    const serverAvailable = await checkServerHealth();

    if (serverAvailable) {
        await syncPendingExpenseDrafts();
    }
});

// OFFLINE GLOBAL OFFLINE LISTENER
window.addEventListener("offline", () => {
    console.log("Browser offline");
    setServerAvailable(false);
});

console.log("offline 6/6 - status.js loaded");