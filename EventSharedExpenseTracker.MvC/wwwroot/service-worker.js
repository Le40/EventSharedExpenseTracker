const CACHE_NAME = "expense-tracker-v1";

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
    event.respondWith(
        networkFirst(request).catch(() => {
            return new Response("You are offline.", {
                status: 503,
                headers: { "Content-Type": "text/plain" }
            });
        })
    );
});

async function networkFirst(request) {
    try {
        const networkResponse = await fetch(request);

        const cache = await caches.open(CACHE_NAME);
        cache.put(request, networkResponse.clone());

        return networkResponse;
    } catch {
        const cachedResponse = await caches.match(request);

        if (cachedResponse) {
            return cachedResponse;
        }

        return new Response("You are offline and this page was not cached yet.", {
            status: 503,
            headers: { "Content-Type": "text/plain" }
        });
    }
}

async function cacheFirst(request) {
    const cachedResponse = await caches.match(request);

    if (cachedResponse) {
        return cachedResponse;
    }

    try {
        const networkResponse = await fetch(request);

        const cache = await caches.open(CACHE_NAME);
        cache.put(request, networkResponse.clone());

        return networkResponse;
    } catch {
        return new Response("Offline and file not cached.", {
            status: 503,
            headers: { "Content-Type": "text/plain" }
        });
    }
}