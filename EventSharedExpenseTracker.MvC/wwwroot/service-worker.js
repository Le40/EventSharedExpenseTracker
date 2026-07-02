// SW can only discover server is unavailable
// by trying request and it timing out
// when such state happens, it stores it locally and messages it to pages
// it then relies on pages to poll the server, and message back if server is back online.

const CACHE_NAME = "expense-tracker-v2";
const NETWORK_TIMEOUT_MS = 1500;
const MSG_SERVER_AVAILABLE = "SERVER_AVAILABLE";
const MSG_SERVER_UNAVAILABLE = "SERVER_UNAVAILABLE";
let isServerAvailable = true; // also fallback if sw never recieves backonline message from pages.

// LISTEN to pages for server coming back online.
self.addEventListener("message", event => {
    if (event.data?.type === MSG_SERVER_AVAILABLE) {
        isServerAvailable = true;
        console.log("SW server available:", isServerAvailable);
    }
});

self.addEventListener("install", event => {
    console.log("Service worker installed");
    self.skipWaiting();
});

self.addEventListener("activate", event => {
    console.log("Service worker activated");

    event.waitUntil(
        caches.keys().then(cacheNames =>
            Promise.all(
                cacheNames
                    .filter(name => name !== CACHE_NAME)
                    .map(name => caches.delete(name))
            )
        )
    );

    self.clients.claim();
});

self.addEventListener("fetch", event => {
    const request = event.request;
    const url = new URL(request.url);

    // Only handle same-origin requests
    if (url.origin !== self.location.origin) {
        return;
    }

    // Never cache or intercept health checks
    if (url.pathname === "/health/ping") {
        return;
    }

    // Never touch non-GET requests
    // Important for login, logout, forms, antiforgery, create/edit/delete
    if (request.method !== "GET") {
        return;
    }

    // Never touch ASP.NET Identity
    if (url.pathname.startsWith("/Identity/")) {
        return;
    }

    // Never touch auth/security-related endpoints
    if (
        url.pathname.includes("Login") ||
        url.pathname.includes("Logout") ||
        url.pathname.includes("Register") ||
        url.pathname.includes("Manage") ||
        url.pathname.includes("Account")
    ) {
        return;
    }

    // Never cache development/debug files
    if (
        url.pathname.includes("browserLink") ||
        url.pathname.includes("_framework") ||
        url.pathname.includes("hotreload")
    ) {
        return;
    }

    // Static files: cache-first
    if (
        url.pathname.startsWith("/css/") ||
        url.pathname.startsWith("/js/") ||
        url.pathname.startsWith("/lib/") ||
        url.pathname.startsWith("/icons/") ||
        url.pathname === "/manifest.json"
    ) {
        event.respondWith(cacheFirst(request));
        return;
    }

    // MVC pages: network-only for now
    // This keeps your app behavior normal
    event.respondWith(networkFirst(request));
});

async function networkFirst(request) {
    console.log("networkFirst", request.url, "server:", isServerAvailable);
    console.log("SW isServerAvailable:", isServerAvailable);
    // check if there is cache for curent page
    const cachedResponse = await caches.match(request);

    // server unavailable -> load from cache
    if (!isServerAvailable && cachedResponse) {
        return cachedResponse;
    }

    // server available ->
    // -> if there is cache, then if timeout set server as unavailable
    // -> if there is no cache, just wait for server to come back online - nothing better to do.
    try {
        let networkResponse;
        if (cachedResponse) {
            networkResponse = await fetchWithTimeout(request);
        } else {
            networkResponse = await fetch(request);
        }
       
        if (networkResponse.ok) {
            const cache = await caches.open(CACHE_NAME);
            cache.put(request, networkResponse.clone());
        }

        return networkResponse;
     // if the request takes more then TIMEOUT, set server as unavailable. Load from cache.
    } catch (error) {
        console.error(error);
        setServerStatusUnavailable();
        return await getCachedResponse(request);
    }
}

async function cacheFirst(request) {
    const cachedResponse = await caches.match(request);

    if (cachedResponse) {
        return cachedResponse;
    }

    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            const cache = await caches.open(CACHE_NAME);
            cache.put(request, networkResponse.clone());
        }

        return networkResponse;
    } catch {
        return await getCachedResponse(request);
    }
}

async function getCachedResponse(request) {
    const cachedResponse = await caches.match(request);

    if (cachedResponse) {
        return cachedResponse;
    }

    return new Response("You are offline and this page was not cached yet.", {
        status: 503,
        headers: { "Content-Type": "text/plain" }
    });
}

async function fetchWithTimeout(request, timeoutMs = NETWORK_TIMEOUT_MS) {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

    try {
        return await fetch(request, {
            signal: controller.signal
        });
    } finally {
        clearTimeout(timeoutId);
    }
}


function setServerStatusUnavailable() {
    if (isServerAvailable === false) return;

    isServerAvailable = false;

    notifyPagesServerUnavailable();
}

function notifyPagesServerUnavailable() {

    self.clients.matchAll({ type: "window" }).then(clients => {
        for (const client of clients) {
            client.postMessage({
                type: MSG_SERVER_UNAVAILABLE,
            });
        }
    });
}