
const serverHealthCheckIntervalMs = 15000;
const serverHealthTimeoutMs = 1500;


const ServerStatus = {
    isAvailable: navigator.onLine,
    lastCheckedAt: null
};


// MESSENGER TO SERVICE WORKER
function sendServerStatusToServiceWorker() {
    if (!navigator.serviceWorker?.controller) {
        return;
    }

    navigator.serviceWorker.controller.postMessage({
        type: "SERVER_STATUS_CHANGED",
        isAvailable: ServerStatus.isAvailable
    });
}


// SERVER HEALTH CHECK
async function checkServerHealth(timeoutMs = serverHealthTimeoutMs) {
    if (!navigator.onLine) {
        ServerStatus.isAvailable = false;
        ServerStatus.lastCheckedAt = Date.now();
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
    } finally {
        clearTimeout(timeoutId);
    }
}

// SERVER MONITORING
async function startServerHealthMonitoring() {
    await checkServerHealth();

    setInterval(async () => {
        await checkServerHealth();
    }, serverHealthCheckIntervalMs);
}

function setServerAvailable(value) {
    const wasAvailable = ServerStatus.isAvailable;

    ServerStatus.isAvailable = value;
    ServerStatus.lastCheckedAt = Date.now();

    console.log("Server available:", value);
    sendServerStatusToServiceWorker();

    if (!wasAvailable && value) {
        syncPendingDraftsIfOnline();
    }
}



// OFFLINE GLOBAL PAGE LOAD LISTENER
document.addEventListener("DOMContentLoaded", async () => {
    startServerHealthMonitoring();

    await updatePendingSyncUi();

    if (navigator.onLine) {
        const serverAvailable = await checkServerHealth();

        if (serverAvailable) {
            await syncPendingExpenseDrafts();
        }
    }
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
    ServerStatus.isAvailable = false;
});

console.log("offline 6/6 - status.js loaded");