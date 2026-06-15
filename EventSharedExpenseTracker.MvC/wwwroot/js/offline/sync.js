// sync lock
let syncInProgress = false;

async function syncPendingDraftsIfOnline() {
    if (!navigator.onLine) {
        return;
    }

    await syncPendingExpenseDrafts();
}

async function syncPendingExpenseDrafts() {

    if (syncInProgress) {
        console.log("Sync already running.");
        return;
    }

    syncInProgress = true;

    try {
        const drafts = await getAllOfflineExpenses();

        const pendingDrafts = drafts.filter(d => d.syncState === "pendingCreate");

        if (pendingDrafts.length === 0) {
            return;
        }

        for (const draft of pendingDrafts) {
            const formData = new FormData();

            for (const [key, value] of draft.fields) {
                formData.append(key, value);
            }

            formData.append("IsOfflineSync", "true");
            replaceAntiforgeryToken(formData);

            const response = await fetch(draft.url, {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                await deleteOfflineExpense(draft.localId);
            } else {
                const errorData = await response.json();
                draft.syncState = "failedValidation";
                draft.validationErrors = errorData.errors;
                draft.errorMessage = `Sync failed with status ${response.status}`;
                await saveOfflineExpense(draft);
                continue;
            }
        }

        await refreshTripDetails();
        await updatePendingSyncUi();
    }
    finally {
        syncInProgress = false;
    }
}

async function refreshTripDetails() {
    const target = document.querySelector("#tripDetails-target");

    if (!target) {
        return;
    }
    await htmx.ajax("GET", window.location.href, {
        target: "#tripDetails-target",
        select: "#tripDetails-target",
        swap: "outerHTML"
    });
}

function replaceAntiforgeryToken(formData) {
    const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");

    if (!tokenInput) {
        console.warn("No fresh antiforgery token found on page.");
        return;
    }

    formData.set("__RequestVerificationToken", tokenInput.value);
}

console.log("offline 4/5 - sync.js loaded");